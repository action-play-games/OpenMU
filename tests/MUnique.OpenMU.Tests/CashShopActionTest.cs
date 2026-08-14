// <copyright file="CashShopActionTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions.CashShop;

/// <summary>
/// Verifies the server-authoritative cash shop economy in the in-memory persistence implementation.
/// </summary>
[TestFixture]
public class CashShopActionTest
{
    /// <summary>
    /// Ensures the catalog only accepts the exact tuple shipped with the client script.
    /// </summary>
    [Test]
    public void CatalogRequiresExactClientTuple()
    {
        var product = CashShopCatalog.Find(373, 34, 567, 7255, 0, 0);

        Assert.That(product, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(product!.ProductSequence, Is.EqualTo(488));
            Assert.That(product.Price, Is.EqualTo(200));
            Assert.That(product.Currency, Is.EqualTo(CashShopCurrency.GoblinPoints));
            Assert.That(CashShopCatalog.Find(373, 34, 568, 7255, 0, 0), Is.Null);
            Assert.That(CashShopCatalog.Find(373, 34, 567, 7255, 1, 0), Is.Null);
            Assert.That(CashShopCatalog.Find(373, 34, 567, 7255, 0, 1), Is.Null);
        });
    }

    /// <summary>
    /// Ensures a purchase creates one debit, one ledger entry and one storage entry.
    /// </summary>
    [Test]
    public async Task PurchaseIsAtomicAndImmediateReplayIsIdempotent()
    {
        var player = await CreateCashShopPlayerAsync(500).ConfigureAwait(false);
        var action = new CashShopAction();

        var firstResult = await action.BuyAsync(player, 373, 34, 567, 7255, 0, 0).ConfigureAwait(false);
        var replayResult = await action.BuyAsync(player, 373, 34, 567, 7255, 0, 0).ConfigureAwait(false);
        var storage = (await player.PersistenceContext.GetAsync<CashShopStorageItem>().ConfigureAwait(false)).ToList();
        var ledger = (await player.PersistenceContext.GetAsync<CashShopLedgerEntry>().ConfigureAwait(false)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(firstResult, Is.Zero);
            Assert.That(replayResult, Is.Zero);
            Assert.That(player.Account!.CashShopGoblinPoints, Is.EqualTo(300));
            Assert.That(player.Account.CashShopRevision, Is.EqualTo(1));
            Assert.That(storage, Has.Count.EqualTo(1));
            Assert.That(storage[0].State, Is.EqualTo(CashShopStorageState.Active));
            Assert.That(storage[0].ItemCode, Is.EqualTo(7255));
            Assert.That(ledger, Has.Count.EqualTo(1));
            Assert.That(ledger[0].Operation, Is.EqualTo(CashShopLedgerOperation.Purchase));
            Assert.That(ledger[0].Delta, Is.EqualTo(-200));
            Assert.That(ledger[0].BalanceAfter, Is.EqualTo(300));
        });
    }

    /// <summary>
    /// Ensures rejected requests leave balances and storage untouched.
    /// </summary>
    [Test]
    public async Task InvalidOrUnaffordablePurchaseDoesNotMutateEconomy()
    {
        var player = await CreateCashShopPlayerAsync(100).ConfigureAwait(false);
        var action = new CashShopAction();

        var insufficient = await action.BuyAsync(player, 373, 34, 567, 7255, 0, 0).ConfigureAwait(false);
        var alteredProduct = await action.BuyAsync(player, 373, 34, 568, 7255, 0, 0).ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(insufficient, Is.EqualTo(1));
            Assert.That(alteredProduct, Is.EqualTo(4));
            Assert.That(player.Account!.CashShopGoblinPoints, Is.EqualTo(100));
            Assert.That(player.PersistenceContext.GetAsync<CashShopStorageItem>().AsTask().Result, Is.Empty);
            Assert.That(player.PersistenceContext.GetAsync<CashShopLedgerEntry>().AsTask().Result, Is.Empty);
        });
    }

    /// <summary>
    /// Ensures a stale or forged shop session cannot mutate the economy outside a safe zone.
    /// </summary>
    [Test]
    public async Task PurchaseOutsideSafeZoneIsRejected()
    {
        var player = await CreateCashShopPlayerAsync(500).ConfigureAwait(false);
        player.CurrentMap!.Terrain.SafezoneMap[player.Position.X, player.Position.Y] = false;
        var action = new CashShopAction();

        var result = await action.BuyAsync(player, 373, 34, 567, 7255, 0, 0).ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(4));
            Assert.That(player.Account!.CashShopGoblinPoints, Is.EqualTo(500));
            Assert.That(player.PersistenceContext.GetAsync<CashShopStorageItem>().AsTask().Result, Is.Empty);
            Assert.That(player.PersistenceContext.GetAsync<CashShopLedgerEntry>().AsTask().Result, Is.Empty);
        });
    }

    /// <summary>
    /// Ensures two concurrent requests cannot spend the same balance.
    /// </summary>
    [Test]
    public async Task ConcurrentPurchasesCannotOverspend()
    {
        var player = await CreateCashShopPlayerAsync(300).ConfigureAwait(false);
        var action = new CashShopAction();

        var results = await Task.WhenAll(
            action.BuyAsync(player, 373, 34, 567, 7255, 0, 0).AsTask(),
            action.BuyAsync(player, 375, 34, 569, 7254, 0, 0).AsTask()).ConfigureAwait(false);
        var storage = (await player.PersistenceContext.GetAsync<CashShopStorageItem>().ConfigureAwait(false)).ToList();
        var ledger = (await player.PersistenceContext.GetAsync<CashShopLedgerEntry>().ConfigureAwait(false)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(results.Count(result => result == 0), Is.EqualTo(1));
            Assert.That(results.Count(result => result == 1), Is.EqualTo(1));
            Assert.That(player.Account!.CashShopGoblinPoints, Is.GreaterThanOrEqualTo(0));
            Assert.That(storage, Has.Count.EqualTo(1));
            Assert.That(ledger, Has.Count.EqualTo(1));
        });
    }

    /// <summary>
    /// Ensures a gift debits the payer and appears only in the receiver's gift storage.
    /// </summary>
    [Test]
    public async Task GiftUsesReceiverAccountAndAuditsPayer()
    {
        var player = await CreateCashShopPlayerAsync(500).ConfigureAwait(false);
        var receiver = player.PersistenceContext.CreateNew<Account>();
        receiver.LoginName = "receiver";
        var receiverCharacter = player.PersistenceContext.CreateNew<Character>();
        receiverCharacter.Name = "GiftTarget";
        receiver.Characters.Add(receiverCharacter);
        var action = new CashShopAction();

        var result = await action.GiftAsync(player, 374, 34, 568, 7253, 0, 0, "GiftTarget", "Lab gift").ConfigureAwait(false);
        var gifts = await player.PersistenceContext.GetCashShopStorageItemsAsync(receiver.LoginName, CashShopStorageKind.Gift, 0, 10).ConfigureAwait(false);
        var ledger = (await player.PersistenceContext.GetAsync<CashShopLedgerEntry>().ConfigureAwait(false)).Single();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Zero);
            Assert.That(player.Account!.CashShopGoblinPoints, Is.EqualTo(200));
            Assert.That(gifts, Has.Count.EqualTo(1));
            Assert.That(gifts[0].Account, Is.SameAs(receiver));
            Assert.That(gifts[0].GiftSender, Is.EqualTo("CashTester"));
            Assert.That(gifts[0].GiftMessage, Is.EqualTo("Lab gift"));
            Assert.That(ledger.Account, Is.SameAs(player.Account));
            Assert.That(ledger.Operation, Is.EqualTo(CashShopLedgerOperation.Gift));
        });
    }

    /// <summary>
    /// Ensures an unknown gift receiver never debits the payer or creates economy records.
    /// </summary>
    [Test]
    public async Task GiftToUnknownCharacterDoesNotMutateEconomy()
    {
        var player = await CreateCashShopPlayerAsync(500).ConfigureAwait(false);
        var action = new CashShopAction();

        var result = await action.GiftAsync(player, 374, 34, 568, 7253, 0, 0, "Unknown", "No receiver").ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(3));
            Assert.That(player.Account!.CashShopGoblinPoints, Is.EqualTo(500));
            Assert.That(player.PersistenceContext.GetAsync<CashShopStorageItem>().AsTask().Result, Is.Empty);
            Assert.That(player.PersistenceContext.GetAsync<CashShopLedgerEntry>().AsTask().Result, Is.Empty);
        });
    }

    /// <summary>
    /// Ensures gift text fits the fixed UTF-8 packet field without splitting a multi-byte character.
    /// </summary>
    [Test]
    public async Task GiftMessageIsBoundedByUtf8PacketLength()
    {
        var player = await CreateCashShopPlayerAsync(500).ConfigureAwait(false);
        var receiver = player.PersistenceContext.CreateNew<Account>();
        receiver.LoginName = "receiver";
        var receiverCharacter = player.PersistenceContext.CreateNew<Character>();
        receiverCharacter.Name = "GiftTarget";
        receiver.Characters.Add(receiverCharacter);
        var action = new CashShopAction();

        var result = await action.GiftAsync(player, 374, 34, 568, 7253, 0, 0, "GiftTarget", new string('\u00E1', 200)).ConfigureAwait(false);
        var gift = (await player.PersistenceContext.GetAsync<CashShopStorageItem>().ConfigureAwait(false)).Single();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Zero);
            Assert.That(System.Text.Encoding.UTF8.GetByteCount(gift.GiftMessage), Is.EqualTo(200));
            Assert.That(gift.GiftMessage, Has.Length.EqualTo(100));
        });
    }

    /// <summary>
    /// Ensures a claimed storage item is delivered once and remains unavailable to a replay.
    /// </summary>
    [Test]
    public async Task StorageClaimDeliversPhysicalItemExactlyOnce()
    {
        var player = await CreateCashShopPlayerAsync(500).ConfigureAwait(false);
        AddCatalogItemDefinition(player, 14, 87);
        var action = new CashShopAction();
        Assert.That(await action.BuyAsync(player, 373, 34, 567, 7255, 0, 0).ConfigureAwait(false), Is.Zero);
        var storage = (await player.PersistenceContext.GetAsync<CashShopStorageItem>().ConfigureAwait(false)).Single();

        var firstResult = await action.ConsumeAsync(player, (uint)storage.StorageIndex, (uint)storage.StorageIndex, 7255, (byte)'P').ConfigureAwait(false);
        var replayResult = await action.ConsumeAsync(player, (uint)storage.StorageIndex, (uint)storage.StorageIndex, 7255, (byte)'P').ConfigureAwait(false);
        var ledger = (await player.PersistenceContext.GetAsync<CashShopLedgerEntry>().ConfigureAwait(false)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(firstResult, Is.Zero);
            Assert.That(replayResult, Is.EqualTo(1));
            Assert.That(player.Inventory!.Items.Count(), Is.EqualTo(1));
            Assert.That(player.Inventory.Items.Single().Definition!.Group, Is.EqualTo(14));
            Assert.That(player.Inventory.Items.Single().Definition!.Number, Is.EqualTo(87));
            Assert.That(storage.State, Is.EqualTo(CashShopStorageState.Claimed));
            Assert.That(ledger.Count(entry => entry.Operation == CashShopLedgerOperation.Claim), Is.EqualTo(1));
        });
    }

    /// <summary>
    /// Ensures a full inventory preserves the active storage entry.
    /// </summary>
    [Test]
    public async Task FullInventoryDoesNotConsumeStorageEntry()
    {
        var player = await CreateCashShopPlayerAsync(500).ConfigureAwait(false);
        AddCatalogItemDefinition(player, 14, 87);
        var definition = player.GameContext.Configuration.Items.Single();
        foreach (var slot in player.Inventory!.FreeSlots.ToList())
        {
            var blocker = new TemporaryItem { Definition = definition, Durability = 1 };
            Assert.That(await player.Inventory.AddItemAsync(slot, blocker).ConfigureAwait(false), Is.True);
        }

        var action = new CashShopAction();
        Assert.That(await action.BuyAsync(player, 373, 34, 567, 7255, 0, 0).ConfigureAwait(false), Is.Zero);
        var storage = (await player.PersistenceContext.GetAsync<CashShopStorageItem>().ConfigureAwait(false)).Single();

        var result = await action.ConsumeAsync(player, (uint)storage.StorageIndex, (uint)storage.StorageIndex, 7255, (byte)'P').ConfigureAwait(false);
        var ledger = (await player.PersistenceContext.GetAsync<CashShopLedgerEntry>().ConfigureAwait(false)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(21));
            Assert.That(storage.State, Is.EqualTo(CashShopStorageState.Active));
            Assert.That(ledger.Count(entry => entry.Operation == CashShopLedgerOperation.Claim), Is.Zero);
        });
    }

    /// <summary>
    /// Ensures storage deletion is account-bound and produces one audit record even when replayed.
    /// </summary>
    [Test]
    public async Task StorageDeletionIsOwnedAndAuditedExactlyOnce()
    {
        var player = await CreateCashShopPlayerAsync(500).ConfigureAwait(false);
        var foreignAccount = player.PersistenceContext.CreateNew<Account>();
        foreignAccount.LoginName = "foreign";
        var foreignStorage = player.PersistenceContext.CreateNew<CashShopStorageItem>();
        foreignStorage.Account = foreignAccount;
        foreignStorage.StorageIndex = 42;
        foreignStorage.Kind = CashShopStorageKind.Normal;
        foreignStorage.State = CashShopStorageState.Active;
        var action = new CashShopAction();

        var foreignResult = await action.DeleteAsync(player, 42, 42, (byte)'P').ConfigureAwait(false);
        Assert.That(await action.BuyAsync(player, 373, 34, 567, 7255, 0, 0).ConfigureAwait(false), Is.Zero);
        var ownedStorage = (await player.PersistenceContext.GetAsync<CashShopStorageItem>().ConfigureAwait(false))
            .Single(item => item.Account == player.Account);
        var firstResult = await action.DeleteAsync(player, (uint)ownedStorage.StorageIndex, (uint)ownedStorage.StorageIndex, (byte)'P').ConfigureAwait(false);
        var replayResult = await action.DeleteAsync(player, (uint)ownedStorage.StorageIndex, (uint)ownedStorage.StorageIndex, (byte)'P').ConfigureAwait(false);
        var ledger = (await player.PersistenceContext.GetAsync<CashShopLedgerEntry>().ConfigureAwait(false)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(foreignResult, Is.Null);
            Assert.That(foreignStorage.State, Is.EqualTo(CashShopStorageState.Active));
            Assert.That(firstResult, Is.EqualTo(CashShopStorageKind.Normal));
            Assert.That(replayResult, Is.Null);
            Assert.That(ownedStorage.State, Is.EqualTo(CashShopStorageState.Deleted));
            Assert.That(ledger.Count(entry => entry.Operation == CashShopLedgerOperation.Delete), Is.EqualTo(1));
        });
    }

    private static async ValueTask<Player> CreateCashShopPlayerAsync(long goblinPoints)
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.Account!.LoginName = "cash-tester";
        player.Account.CashShopGoblinPoints = goblinPoints;
        player.SelectedCharacter!.Name = "CashTester";
        player.CurrentMap!.Terrain.SafezoneMap[player.Position.X, player.Position.Y] = true;
        return player;
    }

    private static void AddCatalogItemDefinition(Player player, byte group, short number)
    {
        player.GameContext.Configuration.Items.Add(new ItemDefinition
        {
            Group = group,
            Number = number,
            Width = 1,
            Height = 1,
            Durability = 1,
        });
    }
}

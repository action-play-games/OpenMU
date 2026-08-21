// <copyright file="CashShopAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.CashShop;

using System.Security.Cryptography;
using System.Text;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.Views.Inventory;

/// <summary>
/// Executes server-authoritative cash shop operations in the player's persistence unit of work.
/// </summary>
public sealed class CashShopAction
{
    /// <summary>
    /// Maximum number of active entries in each cash shop storage kind.
    /// </summary>
    public const int StorageCapacity = 100;

    /// <summary>
    /// Number of entries sent on one storage page.
    /// </summary>
    public const int StoragePageSize = 10;

    private static readonly TimeSpan ReplayWindow = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets one storage page for the authenticated account.
    /// </summary>
    /// <param name="player">The authenticated player.</param>
    /// <param name="requestedPageIndex">The one-based page index.</param>
    /// <param name="kind">The normal or gift storage kind.</param>
    /// <returns>The page metadata and active entries.</returns>
    public async ValueTask<(ushort TotalCount, ushort TotalPages, ushort PageIndex, IReadOnlyList<CashShopStorageItem> Items)> GetStoragePageAsync(
        Player player,
        uint requestedPageIndex,
        CashShopStorageKind kind)
    {
        if (player.Account is null || requestedPageIndex is 0 or > ushort.MaxValue)
        {
            return (0, 0, 1, Array.Empty<CashShopStorageItem>());
        }

        var pageIndex = (ushort)requestedPageIndex;
        var count = await player.PersistenceContext.GetCashShopStorageItemCountAsync(player.Account.LoginName, kind).ConfigureAwait(false);
        var totalPages = count == 0 ? (ushort)0 : (ushort)Math.Min(ushort.MaxValue, ((count - 1) / StoragePageSize) + 1);
        if (totalPages == 0 || pageIndex > totalPages)
        {
            return ((ushort)Math.Min(count, ushort.MaxValue), totalPages, pageIndex, Array.Empty<CashShopStorageItem>());
        }

        var items = await player.PersistenceContext.GetCashShopStorageItemsAsync(
            player.Account.LoginName,
            kind,
            (pageIndex - 1) * StoragePageSize,
            StoragePageSize).ConfigureAwait(false);
        return ((ushort)Math.Min(count, ushort.MaxValue), totalPages, pageIndex, items);
    }

    /// <summary>
    /// Buys an exact catalog product and places it in normal storage.
    /// </summary>
    /// <param name="player">The authenticated player.</param>
    /// <param name="packageMainIndex">The requested package sequence.</param>
    /// <param name="category">The requested display category.</param>
    /// <param name="productMainIndex">The requested price sequence.</param>
    /// <param name="itemIndex">The requested client item code.</param>
    /// <param name="coinIndex">The requested client cash type.</param>
    /// <param name="mileageFlag">The requested mileage flag.</param>
    /// <returns>The legacy client result code.</returns>
    public async ValueTask<byte> BuyAsync(
        Player player,
        uint packageMainIndex,
        uint category,
        uint productMainIndex,
        ushort itemIndex,
        uint coinIndex,
        byte mileageFlag)
    {
        var product = CashShopCatalog.Find(packageMainIndex, category, productMainIndex, itemIndex, coinIndex, mileageFlag);
        if (product is null || player.Account is null || player.SelectedCharacter is null || !player.IsAtSafezone())
        {
            return 4;
        }

        return await player.RunPersistenceExclusiveAsync(async () =>
        {
            var account = player.Account;
            var fingerprint = CreateFingerprint("buy", account.LoginName, packageMainIndex, category, productMainIndex, itemIndex, coinIndex, mileageFlag);
            if (IsReplay(account, fingerprint))
            {
                return (byte)0;
            }

            if (await player.PersistenceContext.GetCashShopStorageItemCountAsync(account.LoginName, CashShopStorageKind.Normal).ConfigureAwait(false) >= StorageCapacity)
            {
                return (byte)2;
            }

            var previousBalance = GetBalance(account, product.Currency);
            if (previousBalance < product.Price)
            {
                return (byte)1;
            }

            var operationId = Guid.NewGuid();
            var storage = player.PersistenceContext.CreateNew<CashShopStorageItem>();
            PopulateStorage(storage, account, product, CashShopStorageKind.Normal, operationId, string.Empty, string.Empty);
            var ledger = player.PersistenceContext.CreateNew<CashShopLedgerEntry>();
            PopulateLedger(ledger, account, product.Currency, -product.Price, previousBalance - product.Price, operationId, fingerprint, player.Name, CashShopLedgerOperation.Purchase, "Catalog purchase");

            var previousRequest = CaptureRequestState(account);
            SetBalance(account, product.Currency, previousBalance - product.Price);
            SetRequestState(account, fingerprint, operationId);
            try
            {
                if (!await player.PersistenceContext.SaveChangesAsync().ConfigureAwait(false))
                {
                    throw new InvalidOperationException("The cash shop purchase could not be committed.");
                }

                return (byte)0;
            }
            catch
            {
                SetBalance(account, product.Currency, previousBalance);
                RestoreRequestState(account, previousRequest);
                DetachNewEntry(player, storage);
                DetachNewEntry(player, ledger);
                throw;
            }
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Buys an exact catalog product as a gift for the account which owns the receiving character.
    /// </summary>
    /// <param name="player">The authenticated player.</param>
    /// <param name="packageMainIndex">The requested package sequence.</param>
    /// <param name="category">The requested display category.</param>
    /// <param name="productMainIndex">The requested price sequence.</param>
    /// <param name="itemIndex">The requested client item code.</param>
    /// <param name="coinIndex">The requested client cash type.</param>
    /// <param name="mileageFlag">The requested mileage flag.</param>
    /// <param name="receiverCharacterName">The receiving character name.</param>
    /// <param name="giftMessage">The optional gift message.</param>
    /// <returns>The legacy client result code.</returns>
    public async ValueTask<byte> GiftAsync(
        Player player,
        uint packageMainIndex,
        uint category,
        uint productMainIndex,
        ushort itemIndex,
        uint coinIndex,
        byte mileageFlag,
        string receiverCharacterName,
        string giftMessage)
    {
        var product = CashShopCatalog.Find(packageMainIndex, category, productMainIndex, itemIndex, coinIndex, mileageFlag);
        if (product is null || !product.IsGiftAllowed || player.Account is null || player.SelectedCharacter is null || !player.IsAtSafezone())
        {
            return 5;
        }

        var receiverName = NormalizeText(receiverCharacterName, 10);
        if (receiverName.Length == 0)
        {
            return 20;
        }

        return await player.RunPersistenceExclusiveAsync(async () =>
        {
            var payer = player.Account;
            var receiver = await player.PersistenceContext.GetAccountByCharacterNameAsync(receiverName).ConfigureAwait(false);
            if (receiver is null)
            {
                return (byte)3;
            }

            var fingerprint = CreateFingerprint("gift", payer.LoginName, packageMainIndex, category, productMainIndex, itemIndex, coinIndex, mileageFlag, receiverName, NormalizeText(giftMessage, 200));
            if (IsReplay(payer, fingerprint))
            {
                return (byte)0;
            }

            if (await player.PersistenceContext.GetCashShopStorageItemCountAsync(receiver.LoginName, CashShopStorageKind.Gift).ConfigureAwait(false) >= StorageCapacity)
            {
                return (byte)2;
            }

            var previousBalance = GetBalance(payer, product.Currency);
            if (previousBalance < product.Price)
            {
                return (byte)1;
            }

            var operationId = Guid.NewGuid();
            var storage = player.PersistenceContext.CreateNew<CashShopStorageItem>();
            PopulateStorage(storage, receiver, product, CashShopStorageKind.Gift, operationId, player.Name, NormalizeText(giftMessage, 200));
            var ledger = player.PersistenceContext.CreateNew<CashShopLedgerEntry>();
            PopulateLedger(ledger, payer, product.Currency, -product.Price, previousBalance - product.Price, operationId, fingerprint, player.Name, CashShopLedgerOperation.Gift, "Catalog gift");

            var previousRequest = CaptureRequestState(payer);
            SetBalance(payer, product.Currency, previousBalance - product.Price);
            SetRequestState(payer, fingerprint, operationId);
            try
            {
                if (!await player.PersistenceContext.SaveChangesAsync().ConfigureAwait(false))
                {
                    throw new InvalidOperationException("The cash shop gift could not be committed.");
                }

                return (byte)0;
            }
            catch
            {
                SetBalance(payer, product.Currency, previousBalance);
                RestoreRequestState(payer, previousRequest);
                DetachNewEntry(player, storage);
                DetachNewEntry(player, ledger);
                throw;
            }
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Claims one active storage entry into the character inventory exactly once.
    /// </summary>
    /// <param name="player">The authenticated player.</param>
    /// <param name="baseItemCode">The requested storage index.</param>
    /// <param name="mainItemCode">The repeated storage index.</param>
    /// <param name="itemIndex">The requested client item code.</param>
    /// <param name="productType">The expected physical-product discriminator.</param>
    /// <returns>The legacy client result code.</returns>
    public async ValueTask<byte> ConsumeAsync(Player player, uint baseItemCode, uint mainItemCode, ushort itemIndex, byte productType)
    {
        if (player.Account is null || player.SelectedCharacter is null || player.Inventory is null || !player.IsAtSafezone() || productType != (byte)'P' || baseItemCode != mainItemCode)
        {
            return 22;
        }

        return await player.RunPersistenceExclusiveAsync(async () =>
        {
            var storage = await FindStorageItemAsync(player, baseItemCode).ConfigureAwait(false);
            var product = CashShopCatalog.FindByItemCode(itemIndex);
            if (storage is null || product is null || storage.ItemCode != itemIndex || storage.ProductSequence != product.ProductSequence || storage.PriceSequence != product.PriceSequence)
            {
                return (byte)1;
            }

            var definition = player.GameContext.Configuration.Items.FirstOrDefault(item => item.Group == product.ItemGroup && item.Number == product.ItemNumber);
            if (definition is null)
            {
                return (byte)22;
            }

            var deliveredItem = player.PersistenceContext.CreateNew<Item>();
            deliveredItem.Definition = definition;
            deliveredItem.Durability = deliveredItem.IsStackable() ? 1 : definition.Durability;
            var slot = player.Inventory.CheckInvSpace(deliveredItem);
            if (slot is null)
            {
                player.PersistenceContext.Detach(deliveredItem);
                return (byte)21;
            }

            deliveredItem.ItemSlot = slot.Value;
            if (!await player.Inventory.AddItemAsync(slot.Value, deliveredItem).ConfigureAwait(false))
            {
                player.PersistenceContext.Detach(deliveredItem);
                return (byte)21;
            }

            var previousState = storage.State;
            var previousCompletedAt = storage.CompletedAt;
            storage.State = CashShopStorageState.Claimed;
            storage.CompletedAt = DateTime.UtcNow;
            var operationId = Guid.NewGuid();
            var ledger = player.PersistenceContext.CreateNew<CashShopLedgerEntry>();
            PopulateLedger(
                ledger,
                player.Account,
                storage.Currency,
                0,
                GetBalance(player.Account, storage.Currency),
                operationId,
                CreateFingerprint("claim", player.Account.LoginName, storage.StorageIndex, storage.ItemCode),
                player.Name,
                CashShopLedgerOperation.Claim,
                "Storage claim");
            ledger.StorageIndex = storage.StorageIndex;

            try
            {
                if (!await player.PersistenceContext.SaveChangesAsync().ConfigureAwait(false))
                {
                    throw new InvalidOperationException("The cash shop storage claim could not be committed.");
                }
            }
            catch
            {
                storage.State = previousState;
                storage.CompletedAt = previousCompletedAt;
                await player.Inventory.RemoveItemAsync(deliveredItem).ConfigureAwait(false);
                player.PersistenceContext.Detach(deliveredItem);
                DetachNewEntry(player, ledger);
                throw;
            }

            await player.InvokeViewPlugInAsync<IItemAppearPlugIn>(plugIn => plugIn.ItemAppearAsync(deliveredItem)).ConfigureAwait(false);
            return (byte)0;
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes one still-active storage entry of the authenticated account.
    /// </summary>
    /// <param name="player">The authenticated player.</param>
    /// <param name="baseItemCode">The requested storage index.</param>
    /// <param name="mainItemCode">The repeated storage index.</param>
    /// <param name="productType">The expected physical-product discriminator.</param>
    /// <returns>The affected storage kind when the deletion was committed; otherwise, <see langword="null"/>.</returns>
    public async ValueTask<CashShopStorageKind?> DeleteAsync(Player player, uint baseItemCode, uint mainItemCode, byte productType)
    {
        if (player.Account is null || player.SelectedCharacter is null || !player.IsAtSafezone() || productType != (byte)'P' || baseItemCode != mainItemCode)
        {
            return null;
        }

        return await player.RunPersistenceExclusiveAsync<CashShopStorageKind?>(async () =>
        {
            var storage = await FindStorageItemAsync(player, baseItemCode).ConfigureAwait(false);
            if (storage is null)
            {
                return null;
            }

            var previousState = storage.State;
            var previousCompletedAt = storage.CompletedAt;
            storage.State = CashShopStorageState.Deleted;
            storage.CompletedAt = DateTime.UtcNow;
            var operationId = Guid.NewGuid();
            var ledger = player.PersistenceContext.CreateNew<CashShopLedgerEntry>();
            PopulateLedger(
                ledger,
                player.Account,
                storage.Currency,
                0,
                GetBalance(player.Account, storage.Currency),
                operationId,
                CreateFingerprint("delete", player.Account.LoginName, storage.StorageIndex),
                player.Name,
                CashShopLedgerOperation.Delete,
                "Storage deletion");
            ledger.StorageIndex = storage.StorageIndex;

            try
            {
                if (!await player.PersistenceContext.SaveChangesAsync().ConfigureAwait(false))
                {
                    throw new InvalidOperationException("The cash shop storage deletion could not be committed.");
                }

                return storage.Kind;
            }
            catch
            {
                storage.State = previousState;
                storage.CompletedAt = previousCompletedAt;
                DetachNewEntry(player, ledger);
                throw;
            }
        }).ConfigureAwait(false);
    }

    private static async ValueTask<CashShopStorageItem?> FindStorageItemAsync(Player player, uint storageIndex)
    {
        var loginName = player.Account!.LoginName;
        return await player.PersistenceContext.GetCashShopStorageItemAsync(loginName, storageIndex, CashShopStorageKind.Normal).ConfigureAwait(false)
               ?? await player.PersistenceContext.GetCashShopStorageItemAsync(loginName, storageIndex, CashShopStorageKind.Gift).ConfigureAwait(false);
    }

    private static long GetBalance(Account account, CashShopCurrency currency) => currency switch
    {
        CashShopCurrency.WCoinC => account.CashShopWCoinC,
        CashShopCurrency.WCoinP => account.CashShopWCoinP,
        CashShopCurrency.GoblinPoints => account.CashShopGoblinPoints,
        _ => throw new ArgumentOutOfRangeException(nameof(currency)),
    };

    private static void SetBalance(Account account, CashShopCurrency currency, long value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        switch (currency)
        {
            case CashShopCurrency.WCoinC:
                account.CashShopWCoinC = value;
                break;
            case CashShopCurrency.WCoinP:
                account.CashShopWCoinP = value;
                break;
            case CashShopCurrency.GoblinPoints:
                account.CashShopGoblinPoints = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currency));
        }
    }

    private static bool IsReplay(Account account, string fingerprint)
    {
        return account.CashShopLastRequestFingerprint == fingerprint
               && account.CashShopLastRequestAt is { } lastRequest
               && DateTime.UtcNow - lastRequest <= ReplayWindow
               && account.CashShopLastOperationId.HasValue;
    }

    private static string CreateFingerprint(string operation, params object[] values)
    {
        var canonical = operation + "|" + string.Join('|', values.Select(value => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static string NormalizeText(string value, int maximumUtf8Bytes)
    {
        var normalized = new string(value.Where(character => !char.IsControl(character)).ToArray()).Trim();
        if (Encoding.UTF8.GetByteCount(normalized) <= maximumUtf8Bytes)
        {
            return normalized;
        }

        var result = new StringBuilder(normalized.Length);
        var byteCount = 0;
        foreach (var rune in normalized.EnumerateRunes())
        {
            if (byteCount + rune.Utf8SequenceLength > maximumUtf8Bytes)
            {
                break;
            }

            result.Append(rune.ToString());
            byteCount += rune.Utf8SequenceLength;
        }

        return result.ToString();
    }

    private static void PopulateStorage(
        CashShopStorageItem storage,
        Account receiver,
        CashShopCatalogProduct product,
        CashShopStorageKind kind,
        Guid operationId,
        string giftSender,
        string giftMessage)
    {
        storage.Account = receiver;
        storage.Kind = kind;
        storage.State = CashShopStorageState.Active;
        storage.ProductSequence = product.ProductSequence;
        storage.PriceSequence = product.PriceSequence;
        storage.ItemCode = product.ItemIndex;
        storage.StorageGroupCode = product.StorageGroupCode;
        storage.Price = product.Price;
        storage.Currency = product.Currency;
        storage.GiftSender = NormalizeText(giftSender, 10);
        storage.GiftMessage = NormalizeText(giftMessage, 200);
        storage.CreatedAt = DateTime.UtcNow;
        storage.CreatedByOperationId = operationId;
    }

    private static void PopulateLedger(
        CashShopLedgerEntry ledger,
        Account account,
        CashShopCurrency currency,
        long delta,
        long balanceAfter,
        Guid operationId,
        string fingerprint,
        string actor,
        CashShopLedgerOperation operation,
        string description)
    {
        ledger.OperationId = operationId;
        ledger.Account = account;
        ledger.Operation = operation;
        ledger.Currency = currency;
        ledger.Delta = delta;
        ledger.BalanceAfter = balanceAfter;
        ledger.RequestFingerprint = fingerprint;
        ledger.Actor = NormalizeText(actor, 32);
        ledger.Description = description;
        ledger.CreatedAt = DateTime.UtcNow;
    }

    private static (long Revision, string Fingerprint, DateTime? At, Guid? OperationId) CaptureRequestState(Account account)
    {
        return (account.CashShopRevision, account.CashShopLastRequestFingerprint, account.CashShopLastRequestAt, account.CashShopLastOperationId);
    }

    private static void SetRequestState(Account account, string fingerprint, Guid operationId)
    {
        account.CashShopRevision = checked(account.CashShopRevision + 1);
        account.CashShopLastRequestFingerprint = fingerprint;
        account.CashShopLastRequestAt = DateTime.UtcNow;
        account.CashShopLastOperationId = operationId;
    }

    private static void RestoreRequestState(Account account, (long Revision, string Fingerprint, DateTime? At, Guid? OperationId) state)
    {
        account.CashShopRevision = state.Revision;
        account.CashShopLastRequestFingerprint = state.Fingerprint;
        account.CashShopLastRequestAt = state.At;
        account.CashShopLastOperationId = state.OperationId;
    }

    private static void DetachNewEntry(Player player, CashShopStorageItem storage)
    {
        storage.Account = null!;
        player.PersistenceContext.Detach(storage);
    }

    private static void DetachNewEntry(Player player, CashShopLedgerEntry ledger)
    {
        ledger.Account = null!;
        player.PersistenceContext.Detach(ledger);
    }
}

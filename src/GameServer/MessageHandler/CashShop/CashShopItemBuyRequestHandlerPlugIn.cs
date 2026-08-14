// <copyright file="CashShopItemBuyRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.CashShop;

using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions.CashShop;
using MUnique.OpenMU.GameLogic.Views.CashShop;
using MUnique.OpenMU.Network.Packets.ClientToServer;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles purchases against the authoritative catalog and persisted economy.
/// </summary>
[PlugIn]
[Display(Name = "Cash Shop Buy Handler", Description = "Processes cash shop purchase requests.")]
[Guid("F4E7F1A5-30AF-4DF7-AD83-86621D14F836")]
[BelongsToGroup(CashShopGroupHandlerPlugIn.GroupKey)]
internal sealed class CashShopItemBuyRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public byte Key => CashShopItemBuyRequest.SubCode;

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < CashShopItemBuyRequestRef.Length || !player.IsCashShopOpened)
        {
            player.Logger.LogWarning("Rejected invalid cash shop purchase request from {Player}.", player);
            return;
        }

        CashShopItemBuyRequestRef request = packet.Span;
        var packageMainIndex = request.PackageMainIndex;
        var category = request.Category;
        var productMainIndex = request.ProductMainIndex;
        var itemIndex = request.ItemIndex;
        var coinIndex = request.CoinIndex;
        var mileageFlag = request.MileageFlag;
        var hasCatalogMatch = CashShopCatalog.Find(packageMainIndex, category, productMainIndex, itemIndex, coinIndex, mileageFlag) is not null;
        byte result;
        try
        {
            var action = new CashShopAction();
            result = await action.BuyAsync(player, packageMainIndex, category, productMainIndex, itemIndex, coinIndex, mileageFlag).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            var correlationId = Guid.NewGuid();
            player.Logger.LogError(exception, "Cash shop purchase failed. CorrelationId: {CorrelationId}; Player: {Player}.", correlationId, player);
            result = 255;
        }

        if (result != 0)
        {
            player.Logger.LogWarning(
                "Cash shop purchase rejected with result {Result}. Package: {PackageMainIndex}; Category: {Category}; Product: {ProductMainIndex}; Item: {ItemIndex}; Coin: {CoinIndex}; Mileage: {MileageFlag}; CatalogMatch: {CatalogMatch}; AccountPresent: {AccountPresent}; CharacterPresent: {CharacterPresent}; Safezone: {Safezone}.",
                result,
                packageMainIndex,
                category,
                productMainIndex,
                itemIndex,
                coinIndex,
                mileageFlag,
                hasCatalogMatch,
                player.Account is not null,
                player.SelectedCharacter is not null,
                player.IsAtSafezone());
        }

        await player.InvokeViewPlugInAsync<ICashShopViewPlugIn>(p => p.ShowBuyResultAsync(result)).ConfigureAwait(false);
        if (result == 0 && player.Account is { } account)
        {
            await player.InvokeViewPlugInAsync<ICashShopViewPlugIn>(p => p.ShowPointInfoAsync(account.CashShopWCoinC, account.CashShopWCoinP, account.CashShopGoblinPoints)).ConfigureAwait(false);
        }
    }
}

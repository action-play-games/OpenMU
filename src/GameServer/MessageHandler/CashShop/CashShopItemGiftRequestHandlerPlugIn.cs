// <copyright file="CashShopItemGiftRequestHandlerPlugIn.cs" company="MUnique">
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
/// Handles gifts against the authoritative catalog and persisted economy.
/// </summary>
[PlugIn]
[Display(Name = "Cash Shop Gift Handler", Description = "Processes cash shop gift requests.")]
[Guid("3A7742F9-B738-4151-BF86-6307150BAE16")]
[BelongsToGroup(CashShopGroupHandlerPlugIn.GroupKey)]
internal sealed class CashShopItemGiftRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public byte Key => CashShopItemGiftRequest.SubCode;

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < CashShopItemGiftRequestRef.Length || !player.IsCashShopOpened)
        {
            player.Logger.LogWarning("Rejected invalid cash shop gift request from {Player}.", player);
            return;
        }

        CashShopItemGiftRequestRef request = packet.Span;
        var packageMainIndex = request.PackageMainIndex;
        var category = request.Category;
        var productMainIndex = request.ProductMainIndex;
        var itemIndex = request.ItemIndex;
        var coinIndex = request.CoinIndex;
        var mileageFlag = request.MileageFlag;
        var receiverName = request.GiftReceiverName;
        var giftText = request.GiftText;
        byte result;
        try
        {
            var action = new CashShopAction();
            result = await action.GiftAsync(player, packageMainIndex, category, productMainIndex, itemIndex, coinIndex, mileageFlag, receiverName, giftText).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            var correlationId = Guid.NewGuid();
            player.Logger.LogError(exception, "Cash shop gift failed. CorrelationId: {CorrelationId}; Player: {Player}.", correlationId, player);
            result = 255;
        }

        await player.InvokeViewPlugInAsync<ICashShopViewPlugIn>(p => p.ShowGiftResultAsync(result)).ConfigureAwait(false);
        if (result == 0 && player.Account is { } account)
        {
            await player.InvokeViewPlugInAsync<ICashShopViewPlugIn>(p => p.ShowPointInfoAsync(account.CashShopWCoinC, account.CashShopWCoinP, account.CashShopGoblinPoints)).ConfigureAwait(false);
        }
    }
}

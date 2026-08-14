// <copyright file="CashShopItemBuyRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.CashShop;

using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Views.CashShop;
using MUnique.OpenMU.Network.Packets.ClientToServer;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Rejects purchase requests until the authoritative catalog and economy are active.
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

        const byte ProductNotAvailable = 4;
        await player.InvokeViewPlugInAsync<ICashShopViewPlugIn>(p => p.ShowBuyResultAsync(ProductNotAvailable)).ConfigureAwait(false);
    }
}

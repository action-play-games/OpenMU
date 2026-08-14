// <copyright file="CashShopItemGiftRequestHandlerPlugIn.cs" company="MUnique">
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
/// Rejects gift requests until the authoritative catalog and economy are active.
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

        const byte ProductNoLongerAvailable = 5;
        await player.InvokeViewPlugInAsync<ICashShopViewPlugIn>(p => p.ShowGiftResultAsync(ProductNoLongerAvailable)).ConfigureAwait(false);
    }
}

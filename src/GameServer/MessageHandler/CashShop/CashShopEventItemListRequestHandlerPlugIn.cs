// <copyright file="CashShopEventItemListRequestHandlerPlugIn.cs" company="MUnique">
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
/// Handles requests for event packages.
/// </summary>
[PlugIn]
[Display(Name = "Cash Shop Event Item Handler", Description = "Returns the active event package identifiers.")]
[Guid("8F097275-AB1C-41D4-B984-89E851FCE804")]
[BelongsToGroup(CashShopGroupHandlerPlugIn.GroupKey)]
internal sealed class CashShopEventItemListRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public byte Key => CashShopEventItemListRequest.SubCode;

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < CashShopEventItemListRequestRef.Length || !player.IsCashShopOpened)
        {
            player.Logger.LogWarning("Rejected invalid cash shop event-item request from {Player}.", player);
            return;
        }

        await player.InvokeViewPlugInAsync<ICashShopViewPlugIn>(p => p.ShowEventItemsAsync(Array.Empty<uint>())).ConfigureAwait(false);
    }
}

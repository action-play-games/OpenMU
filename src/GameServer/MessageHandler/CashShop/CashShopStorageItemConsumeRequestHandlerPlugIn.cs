// <copyright file="CashShopStorageItemConsumeRequestHandlerPlugIn.cs" company="MUnique">
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
/// Rejects storage consumption until item delivery is active.
/// </summary>
[PlugIn]
[Display(Name = "Cash Shop Storage Consume Handler", Description = "Processes cash shop storage item consumption requests.")]
[Guid("2F6725CF-C96D-487C-BB27-AB0221A3C488")]
[BelongsToGroup(CashShopGroupHandlerPlugIn.GroupKey)]
internal sealed class CashShopStorageItemConsumeRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public byte Key => CashShopStorageItemConsumeRequest.SubCode;

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < CashShopStorageItemConsumeRequestRef.Length || !player.IsCashShopOpened)
        {
            player.Logger.LogWarning("Rejected invalid cash shop storage-consume request from {Player}.", player);
            return;
        }

        const byte ItemCannotBeUsed = 22;
        await player.InvokeViewPlugInAsync<ICashShopViewPlugIn>(p => p.ShowConsumeResultAsync(ItemCannotBeUsed)).ConfigureAwait(false);
    }
}

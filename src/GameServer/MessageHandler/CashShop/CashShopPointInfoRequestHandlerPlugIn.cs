// <copyright file="CashShopPointInfoRequestHandlerPlugIn.cs" company="MUnique">
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
/// Handles requests for the account cash shop balances.
/// </summary>
[PlugIn]
[Display(Name = "Cash Shop Point Info Handler", Description = "Returns the authoritative cash shop balances.")]
[Guid("40F2C4B6-B75D-49F5-8FB7-697C5AA5F038")]
[BelongsToGroup(CashShopGroupHandlerPlugIn.GroupKey)]
internal sealed class CashShopPointInfoRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public byte Key => CashShopPointInfoRequest.SubCode;

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < CashShopPointInfoRequestRef.Length)
        {
            player.Logger.LogWarning("Rejected undersized cash shop point request from {Player}.", player);
            return;
        }

        await player.InvokeViewPlugInAsync<ICashShopViewPlugIn>(p => p.ShowPointInfoAsync(0, 0, 0)).ConfigureAwait(false);
    }
}

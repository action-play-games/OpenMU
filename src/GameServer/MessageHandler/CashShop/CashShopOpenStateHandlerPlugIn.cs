// <copyright file="CashShopOpenStateHandlerPlugIn.cs" company="MUnique">
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
/// Handles requests to open or close the cash shop.
/// </summary>
[PlugIn]
[Display(Name = "Cash Shop Open State Handler", Description = "Validates and updates the current cash shop session state.")]
[Guid("C9FE24F0-B9A9-43BF-BA6B-2AEF60DFCDCB")]
[BelongsToGroup(CashShopGroupHandlerPlugIn.GroupKey)]
internal sealed class CashShopOpenStateHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public byte Key => CashShopOpenState.SubCode;

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < CashShopOpenStateRef.Length)
        {
            player.Logger.LogWarning("Rejected undersized cash shop open-state request from {Player}.", player);
            return;
        }

        CashShopOpenStateRef request = packet.Span;
        if (request.IsClosed)
        {
            player.IsCashShopOpened = false;
            return;
        }

        var isAllowed = player.PlayerState.CurrentState == PlayerState.EnteredWorld
                        && player.SelectedCharacter is not null
                        && player.IsAtSafezone();
        player.IsCashShopOpened = isAllowed;
        await player.InvokeViewPlugInAsync<ICashShopViewPlugIn>(p => p.ShowOpenResultAsync(isAllowed)).ConfigureAwait(false);
    }
}

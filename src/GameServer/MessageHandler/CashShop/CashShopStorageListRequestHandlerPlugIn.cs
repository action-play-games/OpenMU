// <copyright file="CashShopStorageListRequestHandlerPlugIn.cs" company="MUnique">
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
/// Handles cash shop storage page requests.
/// </summary>
[PlugIn]
[Display(Name = "Cash Shop Storage Handler", Description = "Validates cash shop storage paging requests.")]
[Guid("D3A43705-93BC-460D-A1DE-16DFF2EED3AE")]
[BelongsToGroup(CashShopGroupHandlerPlugIn.GroupKey)]
internal sealed class CashShopStorageListRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public byte Key => CashShopStorageListRequest.SubCode;

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < CashShopStorageListRequestRef.Length)
        {
            player.Logger.LogWarning("Rejected undersized cash shop storage request from {Player}.", player);
            return;
        }

        CashShopStorageListRequestRef request = packet.Span;
        var pageIndex = request.PageIndex;
        var isStorageTypeValid = request.InventoryType is (byte)'S' or (byte)'G';
        if (!player.IsCashShopOpened || pageIndex == 0 || !isStorageTypeValid)
        {
            player.Logger.LogWarning("Rejected invalid cash shop storage request from {Player}.", player);
            return;
        }

        await player.InvokeViewPlugInAsync<ICashShopViewPlugIn>(p => p.ShowEmptyStorageAsync(pageIndex)).ConfigureAwait(false);
    }
}

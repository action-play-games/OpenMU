// <copyright file="CashShopDeleteStorageItemRequestHandlerPlugIn.cs" company="MUnique">
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
/// Handles requests to delete cash shop storage entries.
/// </summary>
[PlugIn]
[Display(Name = "Cash Shop Storage Delete Handler", Description = "Validates cash shop storage deletion requests.")]
[Guid("3D546B32-D129-4B5F-8566-63C15FD0E819")]
[BelongsToGroup(CashShopGroupHandlerPlugIn.GroupKey)]
internal sealed class CashShopDeleteStorageItemRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public byte Key => CashShopDeleteStorageItemRequest.SubCode;

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < CashShopDeleteStorageItemRequestRef.Length || !player.IsCashShopOpened)
        {
            player.Logger.LogWarning("Rejected invalid cash shop storage-delete request from {Player}.", player);
            return;
        }

        CashShopDeleteStorageItemRequestRef request = packet.Span;
        var baseItemCode = request.BaseItemCode;
        var mainItemCode = request.MainItemCode;
        var productType = request.ProductType;
        try
        {
            var action = new CashShopAction();
            if (await action.DeleteAsync(player, baseItemCode, mainItemCode, productType).ConfigureAwait(false) is { } kind)
            {
                var page = await action.GetStoragePageAsync(player, 1, kind).ConfigureAwait(false);
                await player.InvokeViewPlugInAsync<ICashShopViewPlugIn>(p => p.ShowStorageAsync(page.TotalCount, page.TotalPages, page.PageIndex, page.Items, kind)).ConfigureAwait(false);
            }
        }
        catch (Exception exception)
        {
            var correlationId = Guid.NewGuid();
            player.Logger.LogError(exception, "Cash shop storage deletion failed. CorrelationId: {CorrelationId}; Player: {Player}.", correlationId, player);
        }
    }
}

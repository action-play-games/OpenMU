// <copyright file="CashShopViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.CashShop;

using System.ComponentModel;
using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Views.CashShop;
using MUnique.OpenMU.Network.Packets.ServerToClient;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Sends the cash shop view contract to a Season 6 game client.
/// </summary>
[PlugIn]
[Display(Name = "Cash Shop View", Description = "Implements the Season 6 C1 D2 cash shop response protocol.")]
[Guid("9AC0E1FC-7DC5-449B-B202-D3D7F8F81BF0")]
public sealed class CashShopViewPlugIn : ICashShopViewPlugIn
{
    /// <summary>
    /// The local catalog version shipped with the current client runtime.
    /// </summary>
    internal static readonly (ushort Zone, ushort Year, ushort Id) CatalogVersion = (512, 2012, 76);

    /// <summary>
    /// The local banner version shipped with the current client runtime.
    /// </summary>
    internal static readonly (ushort Zone, ushort Year, ushort Id) BannerVersion = (583, 2011, 1);

    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="CashShopViewPlugIn"/> class.
    /// </summary>
    /// <param name="player">The remote player.</param>
    public CashShopViewPlugIn(RemotePlayer player)
    {
        this._player = player;
    }

    /// <inheritdoc/>
    public async ValueTask InitializeAsync()
    {
        var connection = this._player.Connection;
        await connection.SendCashShopScriptVersionAsync(CatalogVersion.Zone, CatalogVersion.Year, CatalogVersion.Id).ConfigureAwait(false);
        await connection.SendCashShopBannerVersionAsync(BannerVersion.Zone, BannerVersion.Year, BannerVersion.Id).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowPointInfoAsync(long wCoinC, long wCoinP, long goblinPoints)
    {
        var connection = this._player.Connection;
        var totalCash = checked(wCoinC + wCoinP);
        await connection.SendCashShopPointInfoAsync(0, totalCash, wCoinC, wCoinP, 0, goblinPoints).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowOpenResultAsync(bool isAllowed)
    {
        await this._player.Connection.SendCashShopOpenResponseAsync(isAllowed ? (byte)1 : (byte)0).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowEmptyStorageAsync(uint pageIndex)
    {
        var safePageIndex = (ushort)Math.Clamp(pageIndex, 1u, ushort.MaxValue);
        await this._player.Connection.SendCashShopStorageInfoAsync(0, 0, safePageIndex, 0).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowBuyResultAsync(byte resultCode)
    {
        await this._player.Connection.SendCashShopItemBuyResponseAsync(resultCode, 0).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowGiftResultAsync(byte resultCode)
    {
        await this._player.Connection.SendCashShopItemGiftResponseAsync(resultCode, 0, 0).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowConsumeResultAsync(byte resultCode)
    {
        await this._player.Connection.SendCashShopStorageItemConsumeResponseAsync(resultCode).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowEventItemsAsync(IReadOnlyList<uint> packageIdentifiers)
    {
        var connection = this._player.Connection;
        var count = (ushort)Math.Min(packageIdentifiers.Count, ushort.MaxValue);
        await connection.SendCashShopEventItemCountAsync(count).ConfigureAwait(false);

        for (var offset = 0; offset < count; offset += 9)
        {
            var data = new byte[36];
            var blockCount = Math.Min(9, count - offset);
            for (var i = 0; i < blockCount; i++)
            {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(i * 4, 4), packageIdentifiers[offset + i]);
            }

            await connection.SendCashShopEventItemListAsync(data).ConfigureAwait(false);
        }
    }
}

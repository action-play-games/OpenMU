// <copyright file="ICashShopViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.CashShop;

/// <summary>
/// View contract for the Season 6 cash shop protocol (C1 D2).
/// </summary>
public interface ICashShopViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Sends the authoritative script and banner versions to the client.
    /// </summary>
    ValueTask InitializeAsync();

    /// <summary>
    /// Shows the account balances.
    /// </summary>
    /// <param name="wCoinC">WCoin C balance.</param>
    /// <param name="wCoinP">WCoin P balance.</param>
    /// <param name="goblinPoints">Goblin Point balance.</param>
    ValueTask ShowPointInfoAsync(long wCoinC, long wCoinP, long goblinPoints);

    /// <summary>
    /// Shows whether the cash shop may be opened.
    /// </summary>
    /// <param name="isAllowed">Whether opening is allowed.</param>
    ValueTask ShowOpenResultAsync(bool isAllowed);

    /// <summary>
    /// Shows an empty storage page during the protocol bootstrap phase.
    /// </summary>
    /// <param name="pageIndex">Requested one-based page index.</param>
    ValueTask ShowEmptyStorageAsync(uint pageIndex);

    /// <summary>
    /// Shows a purchase result code.
    /// </summary>
    /// <param name="resultCode">Legacy client result code.</param>
    ValueTask ShowBuyResultAsync(byte resultCode);

    /// <summary>
    /// Shows a gift result code.
    /// </summary>
    /// <param name="resultCode">Legacy client result code.</param>
    ValueTask ShowGiftResultAsync(byte resultCode);

    /// <summary>
    /// Shows a storage-item consumption result code.
    /// </summary>
    /// <param name="resultCode">Legacy client result code.</param>
    ValueTask ShowConsumeResultAsync(byte resultCode);

    /// <summary>
    /// Shows the event package identifiers of the selected category.
    /// </summary>
    /// <param name="packageIdentifiers">Package identifiers.</param>
    ValueTask ShowEventItemsAsync(IReadOnlyList<uint> packageIdentifiers);
}

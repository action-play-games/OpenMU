// <copyright file="CashShopLedgerOperation.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.DataModel.Entities;

/// <summary>
/// The kind of an append-only cash shop ledger entry.
/// </summary>
public enum CashShopLedgerOperation
{
    /// <summary>
    /// An administrator credited or debited the account.
    /// </summary>
    AdministrativeAdjustment,

    /// <summary>
    /// The account bought an item for itself.
    /// </summary>
    Purchase,

    /// <summary>
    /// The account bought an item for another character.
    /// </summary>
    Gift,

    /// <summary>
    /// A stored item was delivered to the inventory.
    /// </summary>
    Claim,

    /// <summary>
    /// A stored item was deleted.
    /// </summary>
    Delete,
}

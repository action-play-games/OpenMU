// <copyright file="CashShopStorageState.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.DataModel.Entities;

/// <summary>
/// The lifecycle state of a cash shop storage item.
/// </summary>
public enum CashShopStorageState
{
    /// <summary>
    /// The item can be claimed or deleted.
    /// </summary>
    Active,

    /// <summary>
    /// The item was delivered exactly once.
    /// </summary>
    Claimed,

    /// <summary>
    /// The account owner deleted the item.
    /// </summary>
    Deleted,
}

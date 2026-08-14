// <copyright file="CashShopStorageKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.DataModel.Entities;

/// <summary>
/// The kind of a cash shop storage item.
/// </summary>
public enum CashShopStorageKind
{
    /// <summary>
    /// An item bought by the account owner.
    /// </summary>
    Normal,

    /// <summary>
    /// An item which another character sent as a gift.
    /// </summary>
    Gift,
}

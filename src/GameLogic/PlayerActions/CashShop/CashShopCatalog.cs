// <copyright file="CashShopCatalog.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.CashShop;

using MUnique.OpenMU.DataModel.Entities;

/// <summary>
/// Provides the small, audited catalog which is shipped with client script 512.2012.076.
/// </summary>
public static class CashShopCatalog
{
    private static readonly IReadOnlyList<CashShopCatalogProduct> Products =
    [
        new(373, 34, 567, 7255, 0, 0, 488, 567, 673, 14, 87, 200, CashShopCurrency.GoblinPoints, true),
        new(375, 34, 569, 7254, 0, 0, 490, 569, 673, 14, 86, 300, CashShopCurrency.GoblinPoints, true),
        new(374, 34, 568, 7253, 0, 0, 489, 568, 673, 14, 85, 300, CashShopCurrency.GoblinPoints, true),
    ];

    /// <summary>
    /// Gets all active products.
    /// </summary>
    public static IReadOnlyList<CashShopCatalogProduct> ActiveProducts => Products;

    /// <summary>
    /// Resolves an exact client request to one authoritative product.
    /// </summary>
    /// <param name="packageMainIndex">The requested package sequence.</param>
    /// <param name="category">The requested display category.</param>
    /// <param name="productMainIndex">The requested price sequence.</param>
    /// <param name="itemIndex">The requested client item code.</param>
    /// <param name="coinIndex">The requested client cash type.</param>
    /// <param name="mileageFlag">The requested mileage flag.</param>
    /// <returns>The matching active product, or <see langword="null"/>.</returns>
    public static CashShopCatalogProduct? Find(
        uint packageMainIndex,
        uint category,
        uint productMainIndex,
        ushort itemIndex,
        uint coinIndex,
        byte mileageFlag)
    {
        return Products.SingleOrDefault(product =>
            product.PackageMainIndex == packageMainIndex
            && product.Category == category
            && product.ProductMainIndex == productMainIndex
            && product.ItemIndex == itemIndex
            && product.CoinIndex == coinIndex
            && product.MileageFlag == mileageFlag);
    }

    /// <summary>
    /// Resolves a stored client item code to an active product.
    /// </summary>
    /// <param name="itemCode">The stored client item code.</param>
    /// <returns>The matching active product, or <see langword="null"/>.</returns>
    public static CashShopCatalogProduct? FindByItemCode(ushort itemCode)
    {
        return Products.SingleOrDefault(product => product.ItemIndex == itemCode);
    }
}

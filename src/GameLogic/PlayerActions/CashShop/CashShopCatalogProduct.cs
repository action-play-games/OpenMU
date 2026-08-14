// <copyright file="CashShopCatalogProduct.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.CashShop;

using MUnique.OpenMU.DataModel.Entities;

/// <summary>
/// One server-authoritative product of the versioned cash shop catalog.
/// </summary>
public sealed class CashShopCatalogProduct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CashShopCatalogProduct"/> class.
    /// </summary>
    /// <param name="packageMainIndex">The package sequence.</param>
    /// <param name="category">The display category.</param>
    /// <param name="productMainIndex">The product index sent by the client; single-price packages send zero.</param>
    /// <param name="itemIndex">The client item code.</param>
    /// <param name="coinIndex">The client cash type.</param>
    /// <param name="mileageFlag">The client mileage flag.</param>
    /// <param name="productSequence">The storage product sequence.</param>
    /// <param name="priceSequence">The storage price sequence.</param>
    /// <param name="storageGroupCode">The storage group code.</param>
    /// <param name="itemGroup">The OpenMU item group.</param>
    /// <param name="itemNumber">The OpenMU item number.</param>
    /// <param name="price">The authoritative price.</param>
    /// <param name="currency">The authoritative currency.</param>
    /// <param name="isGiftAllowed">Whether the product can be gifted.</param>
    public CashShopCatalogProduct(
        uint packageMainIndex,
        uint category,
        uint productMainIndex,
        ushort itemIndex,
        uint coinIndex,
        byte mileageFlag,
        uint productSequence,
        uint priceSequence,
        uint storageGroupCode,
        byte itemGroup,
        short itemNumber,
        long price,
        CashShopCurrency currency,
        bool isGiftAllowed)
    {
        this.PackageMainIndex = packageMainIndex;
        this.Category = category;
        this.ProductMainIndex = productMainIndex;
        this.ItemIndex = itemIndex;
        this.CoinIndex = coinIndex;
        this.MileageFlag = mileageFlag;
        this.ProductSequence = productSequence;
        this.PriceSequence = priceSequence;
        this.StorageGroupCode = storageGroupCode;
        this.ItemGroup = itemGroup;
        this.ItemNumber = itemNumber;
        this.Price = price;
        this.Currency = currency;
        this.IsGiftAllowed = isGiftAllowed;
    }

    /// <summary>Gets the package sequence.</summary>
    public uint PackageMainIndex { get; }

    /// <summary>Gets the display category.</summary>
    public uint Category { get; }

    /// <summary>Gets the product index sent by the client; single-price packages use zero.</summary>
    public uint ProductMainIndex { get; }

    /// <summary>Gets the client item code.</summary>
    public ushort ItemIndex { get; }

    /// <summary>Gets the client cash type.</summary>
    public uint CoinIndex { get; }

    /// <summary>Gets the client mileage flag.</summary>
    public byte MileageFlag { get; }

    /// <summary>Gets the product sequence used by storage packets.</summary>
    public uint ProductSequence { get; }

    /// <summary>Gets the price sequence used by storage packets.</summary>
    public uint PriceSequence { get; }

    /// <summary>Gets the client storage group.</summary>
    public uint StorageGroupCode { get; }

    /// <summary>Gets the OpenMU item group.</summary>
    public byte ItemGroup { get; }

    /// <summary>Gets the OpenMU item number.</summary>
    public short ItemNumber { get; }

    /// <summary>Gets the authoritative integer price.</summary>
    public long Price { get; }

    /// <summary>Gets the authoritative currency.</summary>
    public CashShopCurrency Currency { get; }

    /// <summary>Gets a value indicating whether this product may be gifted.</summary>
    public bool IsGiftAllowed { get; }
}

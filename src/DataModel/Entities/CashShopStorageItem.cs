// <copyright file="CashShopStorageItem.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.DataModel.Entities;

/// <summary>
/// A product waiting in an account's cash shop storage.
/// </summary>
[AggregateRoot]
public class CashShopStorageItem
{
    /// <summary>
    /// Gets or sets the database-generated legacy storage index.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
    public long StorageIndex { get; set; }

    /// <summary>
    /// Gets or sets the receiving account.
    /// </summary>
    [Required]
    public virtual Account Account { get; set; } = null!;

    /// <summary>
    /// Gets or sets the storage kind.
    /// </summary>
    public CashShopStorageKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the lifecycle state.
    /// </summary>
    public CashShopStorageState State { get; set; }

    /// <summary>
    /// Gets or sets the client catalog product sequence.
    /// </summary>
    public uint ProductSequence { get; set; }

    /// <summary>
    /// Gets or sets the client catalog price sequence.
    /// </summary>
    public uint PriceSequence { get; set; }

    /// <summary>
    /// Gets or sets the client item code.
    /// </summary>
    public ushort ItemCode { get; set; }

    /// <summary>
    /// Gets or sets the storage group code expected by the client catalog.
    /// </summary>
    public uint StorageGroupCode { get; set; }

    /// <summary>
    /// Gets or sets the price paid for the item.
    /// </summary>
    public long Price { get; set; }

    /// <summary>
    /// Gets or sets the currency used to buy the item.
    /// </summary>
    public CashShopCurrency Currency { get; set; }

    /// <summary>
    /// Gets or sets the sending character name of a gift.
    /// </summary>
    [MaxLength(10)]
    public string GiftSender { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gift message.
    /// </summary>
    [MaxLength(200)]
    public string GiftMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation time in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the time at which the item left active storage.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the operation identifier which created the item.
    /// </summary>
    public Guid CreatedByOperationId { get; set; }
}

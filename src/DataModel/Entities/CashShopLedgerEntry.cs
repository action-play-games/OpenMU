// <copyright file="CashShopLedgerEntry.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.DataModel.Entities;

/// <summary>
/// An immutable audit record for one cash shop operation.
/// </summary>
[AggregateRoot]
public class CashShopLedgerEntry
{
    /// <summary>
    /// Gets or sets the globally unique idempotency identifier.
    /// </summary>
    public Guid OperationId { get; set; }

    /// <summary>
    /// Gets or sets the affected account.
    /// </summary>
    [Required]
    public virtual Account Account { get; set; } = null!;

    /// <summary>
    /// Gets or sets the operation kind.
    /// </summary>
    public CashShopLedgerOperation Operation { get; set; }

    /// <summary>
    /// Gets or sets the affected currency.
    /// </summary>
    public CashShopCurrency Currency { get; set; }

    /// <summary>
    /// Gets or sets the signed balance change.
    /// </summary>
    public long Delta { get; set; }

    /// <summary>
    /// Gets or sets the authoritative balance after the operation.
    /// </summary>
    public long BalanceAfter { get; set; }

    /// <summary>
    /// Gets or sets the related storage index, when available.
    /// </summary>
    public long? StorageIndex { get; set; }

    /// <summary>
    /// Gets or sets the request fingerprint used for duplicate suppression.
    /// </summary>
    [MaxLength(64)]
    public string RequestFingerprint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character or administrative actor.
    /// </summary>
    [MaxLength(32)]
    public string Actor { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a non-sensitive description of the operation.
    /// </summary>
    [MaxLength(256)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operation time in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

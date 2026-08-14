// <copyright file="CashShopPacketTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.Network.Packets.ServerToClient;

/// <summary>
/// Verifies the binary contract of the legacy cash shop packets.
/// </summary>
[TestFixture]
public class CashShopPacketTest
{
    /// <summary>
    /// Verifies the packet sizes expected by the Season 6 client.
    /// </summary>
    [Test]
    public void ServerPacketLengthsMatchClientContract()
    {
        Assert.Multiple(() =>
        {
            Assert.That(CashShopPointInfoRef.Length, Is.EqualTo(45));
            Assert.That(CashShopOpenResponseRef.Length, Is.EqualTo(5));
            Assert.That(CashShopItemBuyResponseRef.Length, Is.EqualTo(9));
            Assert.That(CashShopItemGiftResponseRef.Length, Is.EqualTo(17));
            Assert.That(CashShopStorageInfoRef.Length, Is.EqualTo(12));
            Assert.That(CashShopStorageItemConsumeResponseRef.Length, Is.EqualTo(5));
            Assert.That(CashShopScriptVersionRef.Length, Is.EqualTo(10));
            Assert.That(CashShopStorageItemRef.Length, Is.EqualTo(33));
            Assert.That(CashShopGiftStorageItemRef.Length, Is.EqualTo(244));
            Assert.That(CashShopEventItemCountRef.Length, Is.EqualTo(6));
            Assert.That(CashShopEventItemListRef.Length, Is.EqualTo(40));
            Assert.That(CashShopBannerVersionRef.Length, Is.EqualTo(10));
        });
    }

    /// <summary>
    /// Verifies that catalog versions survive the packet round-trip without overlapping the header.
    /// </summary>
    [Test]
    public void ScriptVersionKeepsItsValues()
    {
        var data = new byte[CashShopScriptVersionRef.Length];
        var written = new CashShopScriptVersionRef(data)
        {
            Zone = 512,
            Year = 2012,
            YearId = 76,
        };

        var read = new CashShopScriptVersionRef(data);
        Assert.That(read.Header.Type, Is.EqualTo(0xC1));
        Assert.That(read.Header.Length, Is.EqualTo(CashShopScriptVersionRef.Length));
        Assert.That(read.Header.Code, Is.EqualTo(0xD2));
        Assert.That(read.Header.SubCode, Is.EqualTo(0x0C));
        Assert.That(read.Zone, Is.EqualTo(512));
        Assert.That(read.Year, Is.EqualTo(2012));
        Assert.That(read.YearId, Is.EqualTo(76));
    }

    /// <summary>
    /// Verifies that the three displayed balances use independent little-endian fields.
    /// </summary>
    [Test]
    public void PointInfoKeepsItsValues()
    {
        var data = new byte[CashShopPointInfoRef.Length];
        var written = new CashShopPointInfoRef(data)
        {
            ViewType = 1,
            TotalCash = 30,
            CashCredit = 10,
            CashPrepaid = 20,
            TotalPoint = 40,
            TotalMileage = 50,
        };

        var read = new CashShopPointInfoRef(data);
        Assert.That(read.Header.Code, Is.EqualTo(0xD2));
        Assert.That(read.Header.SubCode, Is.EqualTo(0x01));
        Assert.That(read.ViewType, Is.EqualTo(1));
        Assert.That(read.TotalCash, Is.EqualTo(30));
        Assert.That(read.CashCredit, Is.EqualTo(10));
        Assert.That(read.CashPrepaid, Is.EqualTo(20));
        Assert.That(read.TotalPoint, Is.EqualTo(40));
        Assert.That(read.TotalMileage, Is.EqualTo(50));
    }

    /// <summary>
    /// Verifies that an item storage record survives the packet round-trip.
    /// </summary>
    [Test]
    public void StorageItemKeepsItsValues()
    {
        var data = new byte[CashShopStorageItemRef.Length];
        var written = new CashShopStorageItemRef(data)
        {
            StorageIndex = 11,
            ItemSequence = 22,
            StorageGroupCode = 33,
            ProductSequence = 44,
            PriceSequence = 55,
            CashPoint = 66,
            ItemType = (byte)'S',
        };

        var read = new CashShopStorageItemRef(data);
        Assert.That(read.Header.Code, Is.EqualTo(0xD2));
        Assert.That(read.Header.SubCode, Is.EqualTo(0x0D));
        Assert.That(read.StorageIndex, Is.EqualTo(11));
        Assert.That(read.ItemSequence, Is.EqualTo(22));
        Assert.That(read.StorageGroupCode, Is.EqualTo(33));
        Assert.That(read.ProductSequence, Is.EqualTo(44));
        Assert.That(read.PriceSequence, Is.EqualTo(55));
        Assert.That(read.CashPoint, Is.EqualTo(66));
        Assert.That(read.ItemType, Is.EqualTo((byte)'S'));
    }
}

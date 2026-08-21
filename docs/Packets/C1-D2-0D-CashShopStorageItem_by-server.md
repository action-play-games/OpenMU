# C1 D2 0D - CashShopStorageItem (by server)

## Is sent when

The server sends an item from the requested normal storage page.

## Causes the following actions on the client side

The client adds the item to its cash shop storage list.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   33   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xD2  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x0D  | Packet header - sub packet type identifier |
| 4 | 4 | IntegerLittleEndian |  | StorageIndex |
| 8 | 4 | IntegerLittleEndian |  | ItemSequence |
| 12 | 4 | IntegerLittleEndian |  | StorageGroupCode |
| 16 | 4 | IntegerLittleEndian |  | ProductSequence |
| 20 | 4 | IntegerLittleEndian |  | PriceSequence |
| 24 | 8 | Double |  | CashPoint |
| 32 | 1 | Byte |  | ItemType |
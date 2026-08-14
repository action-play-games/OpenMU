# C1 D2 14 - CashShopEventItemList (by server)

## Is sent when

The server sends a block of event package identifiers.

## Causes the following actions on the client side

The client adds the event packages to the selected category.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   40   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xD2  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x14  | Packet header - sub packet type identifier |
| 4 | 36 | Binary |  | PackageSequences |
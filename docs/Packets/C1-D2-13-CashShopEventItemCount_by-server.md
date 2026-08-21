# C1 D2 13 - CashShopEventItemCount (by server)

## Is sent when

The client requests the event items of a category.

## Causes the following actions on the client side

The client prepares to receive the event package identifiers.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   6   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xD2  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x13  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | ItemCount |
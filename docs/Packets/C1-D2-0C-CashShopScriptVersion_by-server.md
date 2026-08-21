# C1 D2 0C - CashShopScriptVersion (by server)

## Is sent when

A player enters the game or the authoritative catalog changes.

## Causes the following actions on the client side

The client loads the matching local catalog and unlocks the cash shop.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   10   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xD2  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x0C  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | Zone |
| 6 | 2 | ShortLittleEndian |  | Year |
| 8 | 2 | ShortLittleEndian |  | YearId |
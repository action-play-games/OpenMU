# C1 D2 04 - CashShopItemGiftResponse (by server)

## Is sent when

A cash shop gift request has completed.

## Causes the following actions on the client side

The client displays the gift result and refreshes balances on success.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   17   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xD2  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x04  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | ResultCode |
| 5 | 4 | IntegerLittleEndian |  | ItemLeftCount |
| 9 | 8 | Double |  | LimitedCash |
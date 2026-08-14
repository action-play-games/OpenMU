# C1 D2 03 - CashShopItemBuyResponse (by server)

## Is sent when

A cash shop purchase request has completed.

## Causes the following actions on the client side

The client displays the purchase result and refreshes balances and storage on success.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   9   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xD2  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x03  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | ResultCode |
| 5 | 4 | IntegerLittleEndian |  | ItemLeftCount |
# C1 D2 0B - CashShopStorageItemConsumeResponse (by server)

## Is sent when

A cash shop storage item consume request has completed.

## Causes the following actions on the client side

The client displays the consume result and refreshes storage on success.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   5   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xD2  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x0B  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Result |
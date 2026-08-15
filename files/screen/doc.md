# Pinout: ESP32-WROOM-32 ↔ GMT130 IPS 240×240 display

Verified on the actual board by tracing each wire from the display
connector to the ESP32 header.

## Connection table

| (GMT130 IPS 240×240)         | Wire                   | ESP32 (WROOM-32)                |
|------------------------------|------------------------|---------------------------------|
| GND                          | brown                  | GND                             |
| VCC                          | red                    | 3V3 (power)                     |
| SCK                          | orange                 | GPIO18 (D18)                    |
| SDA (MOSI)                   | yellow                 | GPIO23 (D23)                    |
| RES (RST)                    | green                  | GPIO4 (D4)                      |
| DC                           | blue                   | GPIO15 (D15)                    |
| BLK (backlight)              | purple                 | VCC (power, no GPIO)            |

## Notes

- **SPI mode 3 is required** (mode 0/2 → init succeeds in logs, but the panel
  samples data on the wrong clock edge and shows a solid black screen).
  Set `io_config.spi_mode = 3` in `main/led.cpp`.
- Panel is GMT130-V1.0, controller ST7789VW (ST7789), 240x240, no CS pin.
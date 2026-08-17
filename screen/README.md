# screen — GMT130 240×240 IPS display (ST7789)

> **Status: future project.** Hardware wiring is documented below; firmware has
> not been written yet.

Wiring the GMT130 240×240 IPS display (ST7789VW controller) to an
ESP32-WROOM-32 over SPI. The wiring was verified on the actual board: each
wire was traced from the display connector to the ESP32 headers.

## Wiring

| GMT130 IPS 240×240   | Wire        | ESP32 (WROOM-32)      |
|----------------------|-------------|-----------------------|
| GND                  | brown       | GND                   |
| VCC                  | red         | 3V3 (power)           |
| SCK                  | orange      | GPIO18 (D18)          |
| SDA (MOSI)           | yellow      | GPIO23 (D23)          |
| RES (RST)            | green       | GPIO4 (D4)            |
| DC                   | blue        | GPIO15 (D15)          |
| BLK (backlight)      | purple      | VCC (power, no GPIO)  |

## Important notes

- **SPI mode 3 is required** (mode 0/2 → init succeeds in the logs, but the
  panel samples data on the wrong clock edge and shows a solid black screen).
- The panel is GMT130-V1.0, ST7789VW (ST7789) controller, 240×240, no CS pin.

## Photos

![GMT130 display](media/display.webp)

![Display wired to the ESP32, top view](media/screen-top-view.webp)

![Wiring close-up](media/wiring-closeup.webp)
# screen — GMT130 240×240 IPS display (ST7789)

Driving the GMT130 240×240 IPS display (ST7789VW controller) from an
ESP32-WROOM-32 over SPI. The wiring was verified on the actual board: each
wire was traced from the display connector to the ESP32 headers.

## Build & flash

A PlatformIO project (Arduino framework, `TFT_eSPI`):

```bash
pio run -t upload          # build and flash
pio device monitor         # open serial monitor (115200)
```

## Firmware

`src/main.cpp` initializes the panel with `TFT_eSPI` and runs an HTTP
server (`http_service`) on a WiFi network. Any JSON body posted to the
server is rendered on the display.

### WiFi credentials

Credentials are **not** stored in the repo. Copy the template to a
gitignored file and fill in your values:

```bash
cp include/secrets.example.h include/secrets.h
# edit include/secrets.h
```

## HTTP API

The display accepts text over HTTP and draws it to the screen.

### `POST /data`

Request body (JSON):

```json
{"value":"Hello\nWorld"}
```

- `value` — text to draw. `\n` (decoded from the JSON escape) starts a new
  line; each line is drawn `newline_y_offset = 32` pixels below the
  previous one, starting at `(20, 20)`.
- Font size is `2` (`write_service(2, true)`).

Example:

```bash
curl -X POST http://192.168.0.77/data \
     -H "Content-Type: application/json" \
     -d '{"value":"Hello\nWorld"}'
```

Responses:

| Code | Meaning                                   |
|------|-------------------------------------------|
| 200  | `{"status":"ok"}` — text drawn            |
| 400  | `{"error":"no body"}` / `{"error":"invalid json"}` |

## Configuration

| Parameter          | Value                            |
|--------------------|----------------------------------|
| Platform           | `espressif32`                    |
| Board              | `esp32doit-devkit-v1` (WROOM-32) |
| Framework          | Arduino                          |
| Library            | `bodmer/TFT_eSPI@^2.5.43`        |
| Display            | GMT130 240×240, ST7789VW         |
| SPI                | mode 3                           |

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

## Demo

![Display demo](media/poster.png)

## Photos

![GMT130 display](media/display.webp)

![Display wired to the ESP32, top view](media/screen-top-view.webp)

![Wiring close-up](media/wiring-closeup.webp)

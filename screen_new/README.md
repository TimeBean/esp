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

The WiFi network name and password are **not** stored in the repo.
To set them up on a fresh clone:

1. Create `include/secrets.h` from the template:

   ```bash
   cp include/secrets.example.h include/secrets.h
   ```

2. Open `include/secrets.h` and set your values:

   ```cpp
   #define WIFI_SSID "your_wifi_ssid"
   #define WIFI_PASSWORD "your_wifi_password"
   ```

   > `include/secrets.h` is listed in `.gitignore` and will never be
   > committed. Only `include/secrets.example.h` (the template) is tracked.

3. Build and flash:

   ```bash
   pio run -t upload
   ```

The device connects to the SSID from `secrets.h` and starts the HTTP
server on `http_service`.

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

If the body contains a `metric` field, the value is drawn as a metric
(two lines at independent positions) instead of plain text:

```json
{"value":"1.4","metric":"metric","value_x":25,"value_y":25,"metric_x":25,"metric_y":80,"value_font_size":3,"metric_font_size":3}
```

- `value` — text to display (the value).
- `metric` — label. Presence of this field switches the renderer to
  `print_one_metric`.
- `value_x`, `value_y` — position of the value text.
- `metric_x`, `metric_y` — position of the metric label.
- `value_font_size`, `metric_font_size` — font sizes of the value and label.

### `POST /image`

Displays a 240×240 image. Request body is the **raw** image data, not JSON:

- `Content-Type: application/octet-stream`
- exactly `240 × 240 × 2 = 115 200` bytes
- one pixel per 2 bytes, **RGB565 little-endian**
  (`byte[2i] = rgb565 & 0xFF`, `byte[2i+1] = rgb565 >> 8`), rows from
  top-left, each row 240 pixels wide.
- `rgb565 = (r >> 3) << 11 | (g >> 2) << 5 | (b >> 3)`

Example (a `.r565` file already converted):

```bash
curl -X POST http://192.168.0.77/image \
     -H "Content-Type: application/octet-stream" \
     --data-binary @image.r565
```

Responses:

| Code | Meaning                                 |
|------|-----------------------------------------|
| 200  | `{"status":"ok"}` — image drawn         |
| 400  | `{"error":"bad image"}` — wrong size/body |

The web client in `src/client/Sender` can upload any image (JPG/PNG/…); it
resizes (center-crop) to 240×240 and converts to RGB565 on the server before
posting.

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

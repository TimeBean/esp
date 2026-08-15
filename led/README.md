# led — blinking the built-in LED

A minimal ESP32 project: the built-in LED toggles every second. A good
starting point for getting familiar with the platform.

## Build & flash

A PlatformIO project (Arduino framework):

```bash
pio run -t upload          # build and flash
pio device monitor         # open serial monitor (115200)
```

## Configuration

| Parameter          | Value                            |
|--------------------|----------------------------------|
| Platform           | `espressif32`                    |
| Board              | `esp32doit-devkit-v1` (WROOM-32) |
| Framework          | Arduino                          |
| Monitor speed      | 115200                           |
| Upload speed       | 921600                           |

## Useful links

- [ESP32 Pinout Reference](https://randomnerdtutorials.com/esp32-pinout-reference-gpios/) —
  which GPIO pins you can use on the ESP32.
#include <Arduino.h>
#include <LittleFS.h>

#include "secrets.h"
#include "infrastructure/write_service.h"
#include "infrastructure/http_service.h"
#include "infrastructure/led_service.h"

constexpr bool IS_DEBUG = false;
constexpr unsigned long LED_ON_DURATION_MS = 100;

write_service *writer;
http_service *http;
led_service *led;

void setup()
{
    Serial.begin(115200);
    LittleFS.begin();

    writer = new write_service(2, IS_DEBUG, 16);

    led = new led_service(BUILTIN_LED);

    http = new http_service(WIFI_SSID, WIFI_PASSWORD, IS_DEBUG, IS_DEBUG);
    http->init();
}

void loop()
{
    String value = http->handle();

    if (http->take_image())
    {
        writer->clear();
        writer->print_image_file("/image.r565");
        led->blink(LED_ON_DURATION_MS);
    }
    else if (value.length() > 0)
    {
        writer->clear();
        writer->print_text(value.c_str(), 30);
        led->blink(LED_ON_DURATION_MS);
    }

    led->update();
}
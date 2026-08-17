#include <Arduino.h>

#include "secrets.h"
#include "infrastructure/write_service.h"
#include "infrastructure/http_service.h"
#include "infrastructure/led_service.h"

constexpr bool IS_DEBUG = false;
constexpr unsigned long LED_ON_DURATION_MS = 200;

write_service *writer;
http_service *http;
led_service *led;

void setup()
{
    Serial.begin(115200);

    writer = new write_service(2, IS_DEBUG, 16);
    led = new led_service(BUILTIN_LED);

    http = new http_service(WIFI_SSID, WIFI_PASSWORD, IS_DEBUG, IS_DEBUG);
    http->init();

    writer->clear();
}

void loop()
{
    String value = http->handle();

    if (value.length() > 0)
    {
        writer->print_text(value.c_str(), 18);
        led->blink(LED_ON_DURATION_MS);
    }

    led->update();
}

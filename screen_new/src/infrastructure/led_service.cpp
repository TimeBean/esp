#include "led_service.h"
#include <Arduino.h>

led_service::led_service(uint8_t pin)
    : pin(pin), on_until(0)
{
    pinMode(pin, OUTPUT);
}

void led_service::set_state(bool is_on)
{
    digitalWrite(pin, is_on ? HIGH : LOW);
}

void led_service::blink(unsigned long duration_ms)
{
    set_state(true);
    on_until = millis() + duration_ms;
}

void led_service::update()
{
    if (on_until != 0 && static_cast<long>(millis() - on_until) >= 0)
    {
        set_state(false);
        on_until = 0;
    }
}

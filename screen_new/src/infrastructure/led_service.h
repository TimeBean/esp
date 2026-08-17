#pragma once
#include <Arduino.h>
#include "../core/Iled_service.h"

class led_service : public Iled_service
{
    uint8_t pin;
    unsigned long on_until;

public:
    explicit led_service(uint8_t pin);
    void set_state(bool is_on) override;
    void blink(unsigned long duration_ms);
    void update();
};

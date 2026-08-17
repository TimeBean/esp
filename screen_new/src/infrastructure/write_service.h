#pragma once
#include "../core/Iwrite_service.h"
#include <TFT_eSPI.h>

class write_service : public Iwrite_service
{
    TFT_eSPI tft;
    unsigned int base_font_size;
    bool is_debug;
    unsigned int newline_y_offset;

public:
    write_service(unsigned int font_size, bool is_debug, unsigned int newline_y_offset = 32);
    void print(const char *text) override;
};
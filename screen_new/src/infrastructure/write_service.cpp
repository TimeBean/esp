#include "write_service.h"

write_service::write_service(unsigned int font_size, bool is_debug, unsigned int newline_y_offset)
    : tft(), base_font_size(font_size), is_debug(is_debug), newline_y_offset(newline_y_offset)
{
    tft.init();
    tft.setRotation(0);
    tft.fillScreen(TFT_BLACK);
    tft.setTextColor(TFT_WHITE);
    tft.setTextFont(1);
}

void write_service::print(const char *text)
{
    tft.fillScreen(TFT_BLACK);
    tft.setTextSize(base_font_size);

    const int x = 20;
    int y = 20;
    const char *start = text;
    const char *p = text;

    while (*p != '\0')
    {
        if (*p == '\n')
        {
            tft.drawString(String(start, p - start), x, y);
            y += newline_y_offset;
            start = p + 1;
        }
        p++;
    }

    if (start != p)
    {
        tft.drawString(String(start, p - start), x, y);
    }
}
#pragma once
#include "../core/Iwrite_service.h"
#include <TFT_eSPI.h>

class write_service : public Iwrite_service
{
    TFT_eSPI tft;
    unsigned int base_font_size;
    bool is_debug;
    unsigned int newline_y_offset;
    uint8_t *frame_buffer;
    bool dma_available;

public:
    write_service(unsigned int font_size, bool is_debug, unsigned int newline_y_offset = 32);
    ~write_service();
    void print_text(const char *text, unsigned int max_length = 0) override;
    bool print_image_file(const char *path) override;
    void clear() override;
    /// @brief Not affected by base_font_size.
    void print_in_corner_of_4(const unsigned int corner_id, const float value, const char *metric) override;
    void init_font();

    static String wrap_by_width(const String &line, unsigned int max_length);
    static String wrap_by_newline(const char *text, unsigned int max_length);
    static String strip_leading_spaces(const String &line);
};

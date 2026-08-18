#pragma once
#include <WString.h>
#include <stddef.h>
#include <stdint.h>

class Iwrite_service
{
public:
    virtual ~Iwrite_service() = default;

    virtual void print_text(const char *text, unsigned int max_length = 0) = 0;
    /// @brief Draws a raw RGB565 image (width*height*2 bytes) from a LittleFS file.
    ///        Returns false if the file is missing or has a bad size.
    virtual bool print_image_file(const char *path) = 0;
    virtual void clear() = 0;

    void print(const String &text, unsigned int max_length = 0)
    {
        print_text(text.c_str(), max_length);
    }

    virtual void print_in_corner_of_4(const unsigned int corner_id, const float value, const char *metric) = 0;
};
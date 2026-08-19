#include "write_service.h"
#include <Arduino.h>
#include <LittleFS.h>
#include <esp_heap_caps.h>

namespace
{
    constexpr int kValueFontSize = 3;
    constexpr int kMetricFontSize = 3;
    constexpr int kValueMetricGap = 40;
    constexpr float kHiddenValue = -1.0f;
}

write_service::write_service(unsigned int font_size, bool is_debug, unsigned int newline_y_offset)
    : tft(), base_font_size(font_size), is_debug(is_debug), newline_y_offset(newline_y_offset),
      frame_buffer(nullptr), dma_available(false)
{
    // Banded DMA buffer. A single whole-frame (115,200-byte) DMA buffer is
    // impossible on classic ESP32: the largest contiguous DMA heap block is
    // only ~110 KB. 80 rows = 38,400 bytes fits comfortably.
    constexpr unsigned int kBandRows = 80;
    const size_t band_bytes = static_cast<size_t>(TFT_WIDTH) * 2u * kBandRows;
    frame_buffer = static_cast<uint8_t *>(
        heap_caps_malloc(band_bytes, MALLOC_CAP_DMA | MALLOC_CAP_8BIT));
    if (!frame_buffer)
    {
        frame_buffer = static_cast<uint8_t *>(malloc(band_bytes));
    }

    tft.init();
    tft.setRotation(0);
#ifdef USE_DMA
    dma_available = tft.initDMA();
#endif
    clear();

    Serial.println("--- display boot ---");
    Serial.print("SPI_FREQUENCY=");
    Serial.println(SPI_FREQUENCY);
    Serial.print("initDMA=");
    Serial.println(dma_available ? "true" : "false");
    Serial.print("frame_buffer=");
    Serial.println(frame_buffer ? "ok" : "FAILED");
    Serial.print("dma_free=");
    Serial.println(heap_caps_get_free_size(MALLOC_CAP_DMA));
    Serial.print("dma_largest_block=");
    Serial.println(heap_caps_get_largest_free_block(MALLOC_CAP_DMA));
    Serial.print("free_heap=");
    Serial.println(ESP.getFreeHeap());

    // init_font();
    tft.setTextDatum(ML_DATUM);
    tft.setTextFont(1);

    tft.setTextColor(TFT_WHITE);
}

write_service::~write_service()
{
    if (frame_buffer)
    {
        heap_caps_free(frame_buffer);
        frame_buffer = nullptr;
    }
}

void write_service::init_font()
{
    fs::File font_file = LittleFS.open("/NotoSans-Regular20.vlw");
    if (is_debug)
    {
        if (font_file)
        {
            Serial.println("Font OK");
            Serial.println(font_file.size());
        }
        else
        {
            Serial.println("Font missing");
        }
    }

    tft.loadFont("NotoSans-Regular20", LittleFS);
}

void write_service::clear()
{
    tft.fillScreen(TFT_BLACK);
}

void write_service::print_text(const char *text, unsigned int max_length)
{
    String wrapped;
    if (max_length > 0)
    {
        wrapped = wrap_by_newline(text, max_length);
        text = wrapped.c_str();
    }

    tft.setTextSize(base_font_size);

    const int x = 4;
    int y = 4;
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

void write_service::print_ready_message()
{
    clear();
    tft.setTextDatum(MC_DATUM);
    tft.setTextSize(base_font_size);
    tft.drawString("Ready", tft.width() / 2, tft.height() / 2);
    tft.setTextDatum(ML_DATUM);
}

void write_service::strip_leading_spaces(const String &line)
{
    unsigned int i = 0;
    while (i < line.length() && line[i] == ' ')
    {
        i++;
    }
    return line.substring(i);
}

String write_service::wrap_by_width(const String &line, unsigned int max_length)
{
    if (max_length == 0)
    {
        return line;
    }
    String wrapped;
    unsigned int col = 0;
    bool at_line_start = false;
    for (const char *p = line.c_str(); *p != '\0'; p++)
    {
        if (col == max_length)
        {
            wrapped += '\n';
            col = 0;
            at_line_start = true;
        }
        if (at_line_start && *p == ' ')
        {
            continue;
        }
        wrapped += *p;
        col++;
        at_line_start = false;
    }
    return wrapped;
}

String write_service::wrap_by_newline(const char *text, unsigned int max_length)
{
    String wrapped;
    const char *line_start = text;
    const char *p = text;
    bool first_line = true;
    while (true)
    {
        if (*p == '\n' || *p == '\0')
        {
            String line(line_start, p - line_start);
            if (!first_line)
            {
                // line = strip_leading_spaces(line);
            }
            wrapped += wrap_by_width(line, max_length);
            if (*p == '\0')
            {
                break;
            }
            wrapped += '\n';
            line_start = p + 1;
            first_line = false;
        }
        p++;
    }
    return wrapped;
}

void write_service::print_one_metric(const char *value, const char *metric,
                                     const int value_x, const int value_y,
                                     const int metric_x, const int metric_y,
                                     const unsigned int value_font_size,
                                     const unsigned int metric_font_size)
{
    tft.setTextDatum(MC_DATUM);
    clear();
    tft.setTextSize(value_font_size);
    tft.drawString(value, value_x, value_y);
    tft.setTextSize(metric_font_size);
    tft.drawString(metric, metric_x, metric_y);
    tft.setTextDatum(ML_DATUM);
}

bool write_service::print_image_file(const char *path)
{
    const size_t row_bytes = static_cast<size_t>(tft.width()) * 2u;
    const size_t expected = row_bytes * static_cast<size_t>(tft.height());

    fs::File file = LittleFS.open(path, "r");
    if (!file || file.size() < expected)
    {
        if (is_debug)
        {
            Serial.println("Image skipped: file missing or bad size");
        }
        if (file)
        {
            file.close();
        }
        return false;
    }

    tft.setSwapBytes(true);

    if (frame_buffer)
    {
        constexpr unsigned int kBandRows = 80;
        const unsigned int width = tft.width();
        const unsigned int height = tft.height();

        tft.startWrite();
        for (unsigned int y = 0; y < height; y += kBandRows)
        {
            const unsigned int rows = min(kBandRows, height - y);
            const size_t want = row_bytes * rows;
            size_t offset = 0;
            while (offset < want)
            {
                const int read = file.read(frame_buffer + offset, want - offset);
                if (read <= 0)
                {
                    file.close();
                    tft.endWrite();
                    return false;
                }
                offset += static_cast<size_t>(read);
            }

#ifdef USE_DMA
            tft.pushImageDMA(0, static_cast<int32_t>(y), static_cast<int32_t>(width),
                             static_cast<int32_t>(rows),
                             reinterpret_cast<uint16_t *>(frame_buffer));
            while (tft.dmaBusy())
            {
            }
#else
            tft.pushImage(0, static_cast<int32_t>(y), static_cast<int32_t>(width),
                          static_cast<int32_t>(rows),
                          reinterpret_cast<uint16_t *>(frame_buffer));
#endif
        }
        tft.endWrite();

        file.close();
        return true;
    }

    uint8_t row[480];
    for (unsigned int y = 0; y < static_cast<unsigned int>(tft.height()); y++)
    {
        if (file.read(row, row_bytes) != static_cast<int>(row_bytes))
        {
            file.close();
            return false;
        }
        tft.pushImage(0, static_cast<int32_t>(y), tft.width(), 1,
                      reinterpret_cast<const uint16_t *>(row));
    }

    file.close();
    return true;
}

void write_service::print_in_corner_of_4(const unsigned int corner_id, const float value, const char *metric)
{
    const unsigned int corner = corner_id % 4;
    const int column = corner % 2;
    const int row = corner / 2;

    const int quadrant_width = tft.width() / 2;
    const int quadrant_height = tft.height() / 2;
    const int quadrant_x = column * quadrant_width;
    const int quadrant_y = row * quadrant_height;

    tft.fillRect(quadrant_x, quadrant_y, quadrant_width, quadrant_height, TFT_BLACK);

    const String value_text = value == kHiddenValue ? "" : static_cast<String>(value);
    const String metric_text = metric;

    tft.setTextSize(kValueFontSize);
    const int value_width = tft.textWidth(value_text);
    const int value_height = tft.fontHeight(kValueFontSize);
    tft.setTextSize(kMetricFontSize);
    const int metric_width = tft.textWidth(metric_text);

    const int block_width = max(value_width, metric_width);
    const int block_height = value_height + kValueMetricGap + tft.fontHeight(kMetricFontSize);
    const int x = quadrant_x + (quadrant_width - block_width) / 2;
    const int y = quadrant_y + (quadrant_height - block_height) / 2;

    tft.setTextSize(kValueFontSize);
    tft.drawString(value_text, x, y);
    tft.setTextSize(kMetricFontSize);
    tft.drawString(metric_text, x, y + value_height + kValueMetricGap);
}

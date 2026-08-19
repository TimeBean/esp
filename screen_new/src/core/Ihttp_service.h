#pragma once
#include <WString.h>

struct metric_payload
{
    bool ready = false;
    String value;
    String metric;
    int value_x = 25;
    int value_y = 25;
    int metric_x = 25;
    int metric_y = 80;
    unsigned int value_font_size = 3;
    unsigned int metric_font_size = 3;
};

class Ihttp_service
{
public:
    virtual void init() = 0;
    /// @brief Processes one client. Returns pending text (already drained).
    virtual String handle() = 0;
    /// @brief True once if a complete image was received since the last
    ///        call. One-shot; the frame is stored in LittleFS.
    virtual bool take_image() = 0;
    /// @brief One-shot: returns the pending metric (ready=false if none).
    virtual metric_payload take_metric() = 0;
};
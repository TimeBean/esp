#pragma once
#include <WString.h>

class Ihttp_service
{
public:
    virtual void init() = 0;
    /// @brief Processes one client. Returns pending text (already drained).
    virtual String handle() = 0;
    /// @brief True once if a complete image was received since the last
    ///        call. One-shot; the frame is stored in LittleFS.
    virtual bool take_image() = 0;
};
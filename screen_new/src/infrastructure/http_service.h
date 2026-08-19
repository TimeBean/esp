#pragma once
#include "../core/Ihttp_service.h"
#include <WebServer.h>
#include <FS.h>

class http_service : public Ihttp_service
{
    const char *ssid;
    const char *password;
    bool is_connection_debug;
    bool is_debug;
    WebServer server;
    String last_value;
    metric_payload pending_metric;

    fs::File image_file;
    size_t received_bytes = 0;
    bool image_ready = false;
    bool image_receive_error = false;

public:
    http_service(const char *ssid, const char *password,
                 bool is_connection_debug, bool is_debug,
                 uint16_t port = 80);
    void init() override;
    String handle() override;
    bool take_image() override;
    metric_payload take_metric() override;
};
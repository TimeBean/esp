#include "http_service.h"
#include <WiFi.h>
#include <ArduinoJson.h>
#include <LittleFS.h>

namespace
{
    constexpr size_t kImageBytes = 240u * 240u * 2u;
    constexpr char kImagePath[] = "/image.r565";
}

http_service::http_service(const char *ssid, const char *password,
                           bool is_connection_debug, bool is_debug, uint16_t port)
    : ssid(ssid), password(password), server(port),
      is_connection_debug(is_connection_debug), is_debug(is_debug)
{
}

void http_service::init()
{
    WiFi.begin(ssid, password);

    if (is_connection_debug)
        Serial.print("Connecting");

    while (WiFi.status() != WL_CONNECTED)
    {
        delay(500);
        if (is_connection_debug)
            Serial.print(".");
    }

    if (is_connection_debug)
    {
        Serial.println();
        Serial.println("Connected");
        Serial.println(WiFi.localIP());
    }

    server.on("/data", HTTP_POST, [this]() {
        if (!server.hasArg("plain"))
        {
            server.send(400, "application/json", "{\"error\":\"no body\"}");
            return;
        }

        const String body = server.arg("plain");

        if (is_debug)
        {
            Serial.println("Received:");
            Serial.println(body);
        }

        JsonDocument doc;

        const DeserializationError error = deserializeJson(doc, body);

        if (error)
        {
            server.send(400, "application/json",
                        "{\"error\":\"invalid json\"}");
            return;
        }

        last_value = doc["value"].as<String>();

        if (is_debug)
        {
            Serial.print("Value: ");
            Serial.println(last_value);
        }

        server.send(200, "application/json", "{\"status\":\"ok\"}");
    });

    server.on(
        "/image", HTTP_POST,
        [this]() {
            if (image_receive_error || !image_ready)
            {
                if (is_debug)
                {
                    Serial.print("Image rejected: error=");
                    Serial.print(image_receive_error);
                    Serial.print(" ready=");
                    Serial.print(image_ready);
                    Serial.print(" bytes=");
                    Serial.println(received_bytes);
                }
                server.send(400, "application/json", "{\"error\":\"bad image\"}");
                return;
            }

            server.send(200, "application/json", "{\"status\":\"ok\"}");
        },
        [this]() {
            HTTPRaw &raw = server.raw();

            switch (raw.status)
            {
            case RAW_START:
                received_bytes = 0;
                image_ready = false;
                image_receive_error =
                    server.clientContentLength() != (int)kImageBytes;

                if (image_receive_error)
                {
                    if (is_debug)
                    {
                        Serial.print("Image bad content-length: ");
                        Serial.println(server.clientContentLength());
                    }
                    break;
                }

                image_file = LittleFS.open(kImagePath, "w");
                if (!image_file)
                {
                    image_receive_error = true;
                    if (is_debug)
                    {
                        Serial.println("Image file open failed");
                    }
                }
                break;

            case RAW_WRITE:
                if (image_receive_error)
                {
                    break;
                }
                if (received_bytes + raw.currentSize > kImageBytes)
                {
                    image_receive_error = true;
                    break;
                }
                if (image_file.write(raw.buf, raw.currentSize) !=
                    static_cast<int>(raw.currentSize))
                {
                    image_receive_error = true;
                    break;
                }
                received_bytes += raw.currentSize;
                break;

            case RAW_END:
                if (image_file)
                {
                    image_file.close();
                }
                if (image_receive_error)
                {
                    break;
                }
                if (received_bytes != kImageBytes)
                {
                    image_receive_error = true;
                    if (is_debug)
                    {
                        Serial.print("Image size mismatch: got ");
                        Serial.println(received_bytes);
                    }
                }
                else
                {
                    image_ready = true;
                }
                break;

            case RAW_ABORTED:
                if (image_file)
                {
                    image_file.close();
                }
                image_receive_error = true;
                break;
            }
        });

    server.begin();

    if (is_debug)
        Serial.println("HTTP server started");
}

String http_service::handle()
{
    server.handleClient();

    String result = last_value;
    last_value = "";
    return result;
}

bool http_service::take_image()
{
    bool result = image_ready;
    image_ready = false;
    image_receive_error = false;
    received_bytes = 0;
    return result;
}
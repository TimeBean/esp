#include "http_service.h"
#include <WiFi.h>
#include <ArduinoJson.h>

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

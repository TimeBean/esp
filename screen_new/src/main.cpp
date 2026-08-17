#include <Arduino.h>

#include "infrastructure/write_service.h"
#include "infrastructure/http_service.h"

write_service *writer;
http_service *http;

void setup()
{
	Serial.begin(115200);

	writer = new write_service(2, true);

	http = new http_service("PLACEHOLDER_SSID", "PLACEHOLDER_PASSWORD", true, true);
	http->init();
}

void loop()
{
	String value = http->handle();

	if (value.length() > 0)
	{
		writer->print(value.c_str());
	}
}
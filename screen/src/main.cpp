#include <Arduino.h>
#include <TFT_eSPI.h>

TFT_eSPI tft;

const unsigned int text_y_offset = 46;

unsigned long lastTime = 0;
const unsigned long interval = 1000;

const unsigned int font_size = 2;

void setup()
{
	tft.init();
	tft.setRotation(0);
	tft.fillScreen(TFT_BLACK);
	tft.setTextColor(TFT_WHITE);
	// tft.setTextSize(font_size);

	tft.setTextSize(font_size + 3);
	tft.drawString("Hello", 20, 20 + 0 * text_y_offset);
	tft.setTextSize(font_size + 3);
	tft.drawString("World", 20, 20 + 1 * text_y_offset);
	tft.setTextSize(font_size + 2);
	tft.drawString("from ESP32", 20, 20 + 2 * text_y_offset);
}

void loop() {}

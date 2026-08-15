#include <Arduino.h>

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
}

void loop() {
  digitalWrite(LED_BUILTIN, HIGH); // LED on
  delay(1000);

  digitalWrite(LED_BUILTIN, LOW);  // LED off
  delay(1000);
}
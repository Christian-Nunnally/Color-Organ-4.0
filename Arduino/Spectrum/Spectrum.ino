#include <Adafruit_NeoPixel.h>

#define NumDisplays 1
#define DisplayPins[NumDisplays] = { 6 }
#define DisplayWidth 8
#define DisplayHeight 8

Adafruit_NeoPixel display1 = Adafruit_NeoPixel(64, 6, NEO_GRB + NEO_KHZ800);
Adafruit_NeoPixel displays[NumDisplays];

int counter = 0;
int value = 0;

void setup() {

  displays[0] = display1;

  for (int i = 0; i < NumDisplays; i++)
  {
    displays[i].begin();
  }
  
  Serial.begin(115200);
}

int displayBuffer[64 *3];
int countFromLastData = 0;
void loop()
{
  if (Serial.available() > 0)
  {
    countFromLastData = 0;
    value = Serial.read();
      
    displayBuffer[counter] = value;
    counter = (counter + 1) % 192;

    if (counter % 64 == 0)
    {
      Serial.write(counter);
    }
    
    if (counter == 0)
    {
      counter = 0;
      for (int i = 0; i < 64; i++)
      {
        display1.setPixelColor(i, display1.Color(displayBuffer[i], displayBuffer[64 + i], displayBuffer[128 + i]));
      }
      display1.show();
    }
  }

  countFromLastData++;

  if (countFromLastData > 10000)
  {
      Serial.write(B0);
      countFromLastData = 0;
      counter = 0;
  }
}

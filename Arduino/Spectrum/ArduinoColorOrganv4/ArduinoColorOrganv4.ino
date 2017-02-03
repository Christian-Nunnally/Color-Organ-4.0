#include <Adafruit_NeoPixel.h>

#define NumDisplays 2
#define DisplayWidth 8
#define DisplayHeight 8
#define BytesPerFrame 97
#define OutOfSyncCounter 60000
#define PixelsPerDisplay 64
#define HeaderSize 1
#define ROffSet 0
#define GOffSet 64
#define BOffSet 128

Adafruit_NeoPixel display1 = Adafruit_NeoPixel(PixelsPerDisplay, 6, NEO_GRB + NEO_KHZ800);
Adafruit_NeoPixel display2 = Adafruit_NeoPixel(PixelsPerDisplay, 5, NEO_GRB + NEO_KHZ800);
Adafruit_NeoPixel displays[NumDisplays];

int counter = 0;
int value = 0;

void setup() {

  displays[0] = display1;
  displays[1] = display2;

  for (int i = 0; i < NumDisplays; i++)
  {
	  displays[i].begin();
  }
  
  SerialUSB.begin(115200);
  Serial.begin(115200);
  delay(1);
  SerialUSB.write(B1);
  delay(1);
}

int displayBuffer[192];
int countFromLastData = 0;
byte currentDisplay = 0;
bool header[HeaderSize];
int p = 0;
void loop()
{
  while (SerialUSB.available() > 0)
  {
  	countFromLastData = 0;
  	value = SerialUSB.read();
  
  	if (counter >= HeaderSize)
  	{
      displayBuffer[p] = map((value & B00001111), 0, 15, 0, 255);
      p = p + 1;
      displayBuffer[p] = map((value & B11110000) >> 4, 0, 15, 0, 255);
      p = p + 1;
      counter = (counter + 1) % BytesPerFrame;
      if (counter != 0) continue;
      else
      {
         for (int i = 0; i < PixelsPerDisplay; i++)
         {
          displays[currentDisplay].setPixelColor(i, displayBuffer[i], displayBuffer[GOffSet + i], displayBuffer[BOffSet + i]);
         }
         displays[currentDisplay].show();
         p = 0;
         SerialUSB.write(counter + 1);
      }
  	}
  	else
    {
      currentDisplay = value;
      counter = (counter + 1) % BytesPerFrame;
    }
  }

  countFromLastData++;

  if (countFromLastData > 30000)
  {
	  SerialUSB.write(counter);
	  countFromLastData = 0;
	  counter = 0;
    p = 0;
  }
}


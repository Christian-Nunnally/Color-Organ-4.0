using System;
using System.Windows.Media;

namespace AudioSpectrum.RackItems
{
    public class StaticLedGraphic
    {
        public string Name { get; set; }
        public byte[] Graphic { get; set; }

        public StaticLedGraphic(string name)
        {
            Graphic = new byte[64*3];
            Name = name;
        }

        public void SetPixel(int pixelNumber, Color color)
        {
            if (pixelNumber < 0 || pixelNumber >= 64) throw new ArgumentException("color");
            Graphic[pixelNumber] = color.R;
            Graphic[pixelNumber + 64] = color.G;
            Graphic[pixelNumber + 128] = color.B;
        }
    }
}

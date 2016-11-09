using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

namespace AudioSpectrum.RackItems
{
    public delegate void DisplayClickedEventHandler(int pixelNumber, EventArgs e);

    /// <summary>
    /// Interaction logic for LedDisplay.xaml
    /// </summary>
    public partial class LedDisplay : UserControl
    {
        public event DisplayClickedEventHandler PixelClicked;

        private bool _mouseDown;
        private Rectangle _lastPixelMouseMovedOver;

        public LedDisplay()
        {
            InitializeComponent();
        }

        public void Set(byte[] data)
        {
            var enhancedBrightnessData = new byte[data.Length];
            if (data.Length < 3*64) return;

            for (var i = 0; i < data.Length; i++)
            {
                enhancedBrightnessData[i] = (byte)(data[i] == 0 ? 0 : Math.Truncate(data[i]/2.0) + 128);
            }

            Pixel1.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[0], enhancedBrightnessData[64 + 0], enhancedBrightnessData[128 + 0]));
            Pixel2.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[1], enhancedBrightnessData[64 + 1], enhancedBrightnessData[128 + 1]));
            Pixel3.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[2], enhancedBrightnessData[64 + 2], enhancedBrightnessData[128 + 2]));
            Pixel4.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[3], enhancedBrightnessData[64 + 3], enhancedBrightnessData[128 + 3]));
            Pixel5.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[4], enhancedBrightnessData[64 + 4], enhancedBrightnessData[128 + 4]));
            Pixel6.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[5], enhancedBrightnessData[64 + 5], enhancedBrightnessData[128 + 5]));
            Pixel7.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[6], enhancedBrightnessData[64 + 6], enhancedBrightnessData[128 + 6]));
            Pixel8.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[7], enhancedBrightnessData[64 + 7], enhancedBrightnessData[128 + 7]));
            Pixel9.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[8], enhancedBrightnessData[64 + 8], enhancedBrightnessData[128 + 8]));
            Pixel10.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[9], enhancedBrightnessData[64 + 9], enhancedBrightnessData[128 + 9]));
            Pixel11.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[10], enhancedBrightnessData[64 + 10], enhancedBrightnessData[128 + 10]));
            Pixel12.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[11], enhancedBrightnessData[64 + 11], enhancedBrightnessData[128 + 11]));
            Pixel13.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[12], enhancedBrightnessData[64 + 12], enhancedBrightnessData[128 + 12]));
            Pixel14.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[13], enhancedBrightnessData[64 + 13], enhancedBrightnessData[128 + 13]));
            Pixel15.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[14], enhancedBrightnessData[64 + 14], enhancedBrightnessData[128 + 14]));
            Pixel16.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[15], enhancedBrightnessData[64 + 15], enhancedBrightnessData[128 + 15]));
            Pixel17.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[16], enhancedBrightnessData[64 + 16], enhancedBrightnessData[128 + 16]));
            Pixel18.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[17], enhancedBrightnessData[64 + 17], enhancedBrightnessData[128 + 17]));
            Pixel19.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[18], enhancedBrightnessData[64 + 18], enhancedBrightnessData[128 + 18]));
            Pixel20.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[19], enhancedBrightnessData[64 + 19], enhancedBrightnessData[128 + 19]));
            Pixel21.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[20], enhancedBrightnessData[64 + 20], enhancedBrightnessData[128 + 20]));
            Pixel22.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[21], enhancedBrightnessData[64 + 21], enhancedBrightnessData[128 + 21]));
            Pixel23.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[22], enhancedBrightnessData[64 + 22], enhancedBrightnessData[128 + 22]));
            Pixel24.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[23], enhancedBrightnessData[64 + 23], enhancedBrightnessData[128 + 23]));
            Pixel25.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[24], enhancedBrightnessData[64 + 24], enhancedBrightnessData[128 + 24]));
            Pixel26.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[25], enhancedBrightnessData[64 + 25], enhancedBrightnessData[128 + 25]));
            Pixel27.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[26], enhancedBrightnessData[64 + 26], enhancedBrightnessData[128 + 26]));
            Pixel28.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[27], enhancedBrightnessData[64 + 27], enhancedBrightnessData[128 + 27]));
            Pixel29.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[28], enhancedBrightnessData[64 + 28], enhancedBrightnessData[128 + 28]));
            Pixel30.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[29], enhancedBrightnessData[64 + 29], enhancedBrightnessData[128 + 29]));
            Pixel31.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[30], enhancedBrightnessData[64 + 30], enhancedBrightnessData[128 + 30]));
            Pixel32.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[31], enhancedBrightnessData[64 + 31], enhancedBrightnessData[128 + 31]));
            Pixel33.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[32], enhancedBrightnessData[64 + 32], enhancedBrightnessData[128 + 32]));
            Pixel34.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[33], enhancedBrightnessData[64 + 33], enhancedBrightnessData[128 + 33]));
            Pixel35.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[34], enhancedBrightnessData[64 + 34], enhancedBrightnessData[128 + 34]));
            Pixel36.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[35], enhancedBrightnessData[64 + 35], enhancedBrightnessData[128 + 35]));
            Pixel37.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[36], enhancedBrightnessData[64 + 36], enhancedBrightnessData[128 + 36]));
            Pixel38.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[37], enhancedBrightnessData[64 + 37], enhancedBrightnessData[128 + 37]));
            Pixel39.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[38], enhancedBrightnessData[64 + 38], enhancedBrightnessData[128 + 38]));
            Pixel40.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[39], enhancedBrightnessData[64 + 39], enhancedBrightnessData[128 + 39]));
            Pixel41.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[40], enhancedBrightnessData[64 + 40], enhancedBrightnessData[128 + 40]));
            Pixel42.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[41], enhancedBrightnessData[64 + 41], enhancedBrightnessData[128 + 41]));
            Pixel43.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[42], enhancedBrightnessData[64 + 42], enhancedBrightnessData[128 + 42]));
            Pixel44.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[43], enhancedBrightnessData[64 + 43], enhancedBrightnessData[128 + 43]));
            Pixel45.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[44], enhancedBrightnessData[64 + 44], enhancedBrightnessData[128 + 44]));
            Pixel46.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[45], enhancedBrightnessData[64 + 45], enhancedBrightnessData[128 + 45]));
            Pixel47.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[46], enhancedBrightnessData[64 + 46], enhancedBrightnessData[128 + 46]));
            Pixel48.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[47], enhancedBrightnessData[64 + 47], enhancedBrightnessData[128 + 47]));
            Pixel49.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[48], enhancedBrightnessData[64 + 48], enhancedBrightnessData[128 + 48]));
            Pixel50.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[49], enhancedBrightnessData[64 + 49], enhancedBrightnessData[128 + 49]));
            Pixel51.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[50], enhancedBrightnessData[64 + 50], enhancedBrightnessData[128 + 50]));
            Pixel52.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[51], enhancedBrightnessData[64 + 51], enhancedBrightnessData[128 + 51]));
            Pixel53.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[52], enhancedBrightnessData[64 + 52], enhancedBrightnessData[128 + 52]));
            Pixel54.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[53], enhancedBrightnessData[64 + 53], enhancedBrightnessData[128 + 53]));
            Pixel55.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[54], enhancedBrightnessData[64 + 54], enhancedBrightnessData[128 + 54]));
            Pixel56.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[55], enhancedBrightnessData[64 + 55], enhancedBrightnessData[128 + 55]));
            Pixel57.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[56], enhancedBrightnessData[64 + 56], enhancedBrightnessData[128 + 56]));
            Pixel58.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[57], enhancedBrightnessData[64 + 57], enhancedBrightnessData[128 + 57]));
            Pixel59.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[58], enhancedBrightnessData[64 + 58], enhancedBrightnessData[128 + 58]));
            Pixel60.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[59], enhancedBrightnessData[64 + 59], enhancedBrightnessData[128 + 59]));
            Pixel61.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[60], enhancedBrightnessData[64 + 60], enhancedBrightnessData[128 + 60]));
            Pixel62.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[61], enhancedBrightnessData[64 + 61], enhancedBrightnessData[128 + 61]));
            Pixel63.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[62], enhancedBrightnessData[64 + 62], enhancedBrightnessData[128 + 62]));
            Pixel64.Fill = new SolidColorBrush(Color.FromArgb(255, enhancedBrightnessData[63], enhancedBrightnessData[64 + 63], enhancedBrightnessData[128 + 63]));
        }

        private void PixelMouseDown(object sender, MouseButtonEventArgs e)
        {
            var clickedPixel = (Rectangle)sender;

            var name = clickedPixel.Name;
            if (name.Length <= 5) return;
            int pixelNumber;
            if (!int.TryParse(name.Substring(5), out pixelNumber)) return;
            pixelNumber--;
            PixelClicked?.Invoke(pixelNumber, e);
            _lastPixelMouseMovedOver = clickedPixel;
        }

        private void LedDisplay_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pixel = Mouse.DirectlyOver as Rectangle;
                if (pixel != null && pixel.Name.Length > 4 && pixel.Name.Substring(0, 5) == "Pixel" && _lastPixelMouseMovedOver != pixel)
                {
                    _lastPixelMouseMovedOver = pixel;
                    int pixelNumber;
                    if (!int.TryParse(pixel.Name.Substring(5), out pixelNumber)) return;
                    pixelNumber--;
                    PixelClicked?.Invoke(pixelNumber, EventArgs.Empty);
                }
            }
        }
    }
}

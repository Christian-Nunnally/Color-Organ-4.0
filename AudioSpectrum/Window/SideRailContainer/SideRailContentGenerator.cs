using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AudioSpectrum.SideRailContainers
{
    public static class SideRailContentGenerator
    {
        public static Rectangle GenerateSeperator(int seperatorThinkness)
        {
            var seperator = new Rectangle
            {
                Height = seperatorThinkness,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Fill = Brushes.Black
            };
            return seperator;
        }
    }
}
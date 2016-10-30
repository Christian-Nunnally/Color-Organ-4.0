using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AudioSpectrum.RackItems
{
    public partial class SpectrumItem : UserControl, IRackItem
    {
        private RackItemContainer _parentRack;
        private readonly Style _barStyle;

        public string ItemName => "Spectrum";

        private IEnumerable<ProgressBar> Bars => SpectrumStackPanel.Children.OfType<ProgressBar>();

        public SpectrumItem()
        {
            _barStyle = new Style(typeof(ProgressBar));
            _barStyle.Setters.Add(new Setter(ProgressBar.OrientationProperty, Orientation.Vertical));
            _barStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.RangeBase.MaximumProperty, 255.0));
            _barStyle.Setters.Add(new Setter(MarginProperty, new Thickness(2, 0, 2, 0)));
            InitializeComponent();
        }

        public IRackItem CreateRackItem()
        {
            return new SpectrumItem();
        }

        public Dictionary<string, Pipe> GetInputs()
        {
            var inputs = new Dictionary<string, Pipe> {{"Spectrum In", SpectrumIn}};
            return inputs;
        }

        public List<string> GetOutputs()
        {
            var outputs = new List<string> {"Spectrum Out"};
            return outputs;
        }

        public void SetRack(RackItemContainer rack)
        {
            _parentRack = rack;
        }

        private void SpectrumIn(List<byte> data, int iteration)
        {
            var newData = new List<byte>();
            if (SpectrumStackPanel.Children.Count != data.Count)
            {
                SpectrumStackPanel.Children.Clear();

                for (var i = 0; i < data.Count; i++)
                {
                    var bar = new ProgressBar
                    {
                        Style = _barStyle,
                    };
                    SpectrumStackPanel.Children.Add(bar);
                }

                var barWidth = (SpectrumStackPanelGrid.ActualWidth - data.Count*4) / data.Count;
                foreach (var progressBar in Bars)
                {
                    progressBar.Width = barWidth;
                }
            }

            var bars = Bars.ToList();
            for (var i = 0; i < data.Count; i++)
            {
                bars[i].Value = data[i];
                newData.Add(data[i]);
            }

            _parentRack.OutputPipe("Spectrum Out", newData, iteration);
        }

        public void CleanUp()
        {

        }

        public bool CanDelete()
        {
            return true;
        }

        public void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            sideRailSetter.Invoke(ItemName, new List<Control>());
        }

        public void HeartBeat()
        {
            
        }
    }
}

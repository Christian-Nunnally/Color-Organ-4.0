using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    public partial class SpectrumItem : RackItemBase
    {
        private readonly Style _barStyle;

        private IEnumerable<ProgressBar> Bars => SpectrumStackPanel.Children.OfType<ProgressBar>();

        public SpectrumItem(XmlNode xml)
        {
            _barStyle = new Style(typeof(ProgressBar));
            _barStyle.Setters.Add(new Setter(ProgressBar.OrientationProperty, Orientation.Vertical));
            _barStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.RangeBase.MaximumProperty, 255.0));
            _barStyle.Setters.Add(new Setter(MarginProperty, new Thickness(2, 0, 2, 0)));
            InitializeComponent();
            ItemName = "Spectrum";

            if (xml == null)
            {
                AddInput(new RackItemInput("Spectrum In", SpectrumIn));
                AddOutput(new RackItemOutput("Spectrum Out"));
            }
            else
            {
                Load(xml);
            }
        }

        public override IRackItem CreateRackItem(XmlElement xml)
        {
            return new SpectrumItem(xml);
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

            if (RackItemOutputs.Count > 0)
            {
                RackContainer.OutputPipe(RackItemOutputs.First(), newData, iteration);
            }
        }

        public override void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement(RackItemName + "-" + ItemName));
            SaveOutputs(xml, node);
            SaveInputs(xml, node);
        }

        public sealed override void Load(XmlNode xml)
        {
            LoadInputsAndOutputs(xml);
            AttachPipeToInput(1, SpectrumIn);
        }
    }
}

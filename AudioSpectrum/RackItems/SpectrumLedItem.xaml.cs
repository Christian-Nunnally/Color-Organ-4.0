using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    public partial class SpectrumLedItem : RackItemBase
    {

        public SpectrumLedItem()
        {
            InitializeComponent();
            ItemName = "Spectrum Led Display";
        }

        public override IRackItem CreateRackItem()
        {
            return new SpectrumLedItem();
        }

        public override Dictionary<string, Pipe> GetInputs()
        {
            var inputs = new Dictionary<string, Pipe> {{"Specturm Led Panel", SpectrumIn}};
            return inputs;
        }

        public override List<string> GetOutputs()
        {
            var outputs = new List<string> {"Graphics Out"};
            return outputs;
        }

        private void SpectrumIn(List<byte> data, int iteration)
        {
            if (data.Count == 0) return;

            while (data.Count < 8)
            {
                for (var i = 0; i < data.Count; i += 2)
                {
                    data.Insert(i + 1, data[i]);
                }
            }

            while (data.Count >= 16)
            {
                for (var i = 0; i < data.Count; i += 1)
                {
                    data.RemoveAt(i);
                }
            }

            var graphicsData = new byte[64 * 3];

            var colStart = 0;
            for(var i = 0; i < 8; i += 1)
            {
                var barValue = (data[i] + 1) / 32;
                for (var j = 0; j < barValue; j++)
                {
                    graphicsData[colStart + j] =  (byte)(10 + (5 * j));
                    graphicsData[64 + colStart + j] = (byte)(50 - (5 * j));
                    graphicsData[128 + colStart + j] = 10;
                }
                colStart += 8;
            }

            LedSimulator.Set(graphicsData);
            RackContainer.OutputPipe("Graphics Out", graphicsData.ToList(), iteration);
        }

        public override void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            sideRailSetter.Invoke(ItemName, new List<Control>());
        }

        public override void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement("SpectrumLedItem"));
        }

        public override void Load(XmlElement xml)
        {
            throw new System.NotImplementedException();
        }
    }
}

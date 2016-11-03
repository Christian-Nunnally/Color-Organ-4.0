using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    [Serializable]
    public partial class AudioProcessorItem : RackItemBase
    {
        private readonly List<List<byte>> _history = new List<List<byte>>();
        private int _current;

        public AudioProcessorItem()
        {
            InitializeComponent();
            ItemName = "AudioProcessor";
        }

        public override void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            sideRailSetter.Invoke(ItemName, new List<Control>());
        }

        public override IRackItem CreateRackItem()
        {
            return new AudioProcessorItem();
        }

        public override Dictionary<string, Pipe> GetInputs()
        {
            var inputs = new Dictionary<string, Pipe> {{"Spectrum Input", SpectrumInput}};
            return inputs;
        }

        public override List<string> GetOutputs()
        {
            var outputs = new List<string> { "Processed Spectrum" };
            return outputs;
        }

        private void SpectrumInput(List<byte> data, int iteration)
        {
            if (NumberOfSamplesUpDown.Value == null) return;
            var samples = NumberOfSamplesUpDown.Value.Value;
            while (_history.Count < samples) _history.Add(new List<byte>());
            while (_history.Count > samples) _history.RemoveAt(_history.Count - 1);

            _history[++_current % _history.Count] = data;

            var processedDataInt = new List<int>();
            for (var i = 0; i < data.Count; i++) processedDataInt.Add(0);
            foreach (var t in _history)
            {
                for (var j = 0; j < t.Count; j++)
                {
                    processedDataInt[j] += t[j];
                }
            }

            var processedData = processedDataInt.Select(t => (byte) (t/samples)).ToList();
            RackContainer.OutputPipe("Processed Spectrum", processedData, iteration);
        }

        public override void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement(RackItemName + "-" + ItemName));
            SaveInputs(xml, parent);
            SaveOutputs(xml, parent);
            node.AppendChild(xml.CreateElement("NumberOfSamples")).InnerText = NumberOfSamplesUpDown.Value.ToString();
        }

        public override void Load(XmlNode xml)
        {
            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
            {
                switch (node.Name)
                {
                    case "NumberOfSamples":
                        int numberOfSamples;
                        if (int.TryParse(node.InnerText, out numberOfSamples))
                        {
                            NumberOfSamplesUpDown.Value = numberOfSamples;
                        }
                        break;
                    case "Outputs":
                        LoadOutputs(node);
                        break;
                    case "Inputs":
                        LoadInputs(node);
                        break;
                }
            }
        }
    }
}

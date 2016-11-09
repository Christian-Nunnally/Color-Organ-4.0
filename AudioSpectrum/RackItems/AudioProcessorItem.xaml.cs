using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml;
using AudioSpectrum.SideRailContainers;
using Xceed.Wpf.Toolkit;

namespace AudioSpectrum.RackItems
{
    [Serializable]
    public partial class AudioProcessorItem : RackItemBase
    {
        private readonly List<List<byte>> _history = new List<List<byte>>();
        private int _current;

        private readonly IntegerUpDown _numberOfSamplesUpDown = new IntegerUpDown();

        public AudioProcessorItem(XmlNode xml)
        {
            _numberOfSamplesUpDown.Minimum = 1;
            _numberOfSamplesUpDown.Maximum = 16;
            _numberOfSamplesUpDown.Increment = 1;

            InitializeComponent();
            ItemName = "AudioProcessor";

            if (xml == null)
            {
                AddInput(new RackItemInput("Spectrum Input", SpectrumInput));
                AddOutput(new RackItemOutput("Processed Spectrum"));
            }
            else
            {
                Load(xml);
            }
        }

        private List<Control> _sideRailControls;

        public override void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            if (_sideRailControls == null)
            {
                _sideRailControls = new List<Control>
                {
                    new LabeledControlSideRailContainer("Number of samples to average", _numberOfSamplesUpDown,
                        Orientation.Horizontal, 80)
                };

            }
            sideRailSetter.Invoke(ItemName, _sideRailControls);
        }

        public override IRackItem CreateRackItem(XmlElement xml)
        {
            return new AudioProcessorItem(xml);
        }

        private void SpectrumInput(List<byte> data, int iteration)
        {
            if (_numberOfSamplesUpDown.Value == null) return;
            var samples = _numberOfSamplesUpDown.Value.Value;
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
            if (RackItemOutputs.Count > 0)
            {
                RackContainer.OutputPipe(RackItemOutputs.First(), processedData, iteration);
            }
        }

        public override void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement(RackItemName + "-" + ItemName));
            SaveOutputs(xml, node);
            SaveInputs(xml, node);
            node.AppendChild(xml.CreateElement("NumberOfSamples")).InnerText = _numberOfSamplesUpDown.Value.ToString();
        }

        public sealed override void Load(XmlNode xml)
        {
            LoadInputsAndOutputs(xml);
            AttachPipeToInput(1, SpectrumInput);

            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
            {
                switch (node.Name)
                {
                    case "NumberOfSamples":
                        int numberOfSamples;
                        if (int.TryParse(node.InnerText, out numberOfSamples))
                        {
                            _numberOfSamplesUpDown.Value = numberOfSamples;
                        }
                        break;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    public partial class FilterSpectrumItem : RackItemBase
    {
        private int _maxBars;
        private int _numBars;

        private readonly List<double> _spectrumMax = new List<double>();
        private readonly List<double> _spectrumMin = new List<double>();

        public FilterSpectrumItem()
        {
            InitializeComponent();
            ItemName = "FilterSpectrum";
        }

        public override void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            sideRailSetter.Invoke(ItemName, new List<Control>());
        }

        private void SpectrumPipeIn(List<byte> spectrum, int iteration)
        {
            var spectrumCopy = spectrum.ToList();
            _maxBars = spectrumCopy.Count;
            ChannelCountSlider.Maximum = _maxBars;

            while (_spectrumMax.Count < spectrumCopy.Count)
            {
                _spectrumMax.Add(0);
            }

            while (_spectrumMin.Count < spectrumCopy.Count)
            {
                _spectrumMin.Add(0);
            }

            for (var i = 0; i < spectrumCopy.Count; i++)
            {
                _spectrumMax[i] -= DecayUpDown.Value.Value;
                _spectrumMin[i] += DecayUpDown.Value.Value;
                _spectrumMax[i] = Math.Max(_spectrumMax[i], spectrumCopy[i]);
                _spectrumMin[i] = Math.Min(_spectrumMin[i], spectrumCopy[i]);
                if (_spectrumMax[i] < _spectrumMin[i]) _spectrumMax[i] = _spectrumMin[i];
            }

            if (_numBars <= 1) return;

            var increment = spectrum.Count / (_numBars - 1.0);
            var mostActiveBands = new List<int>();
            for (var d = 0.0; d < spectrumCopy.Count - (increment / 2); d += increment)
            {
                var lowerBound = (int)Math.Max(0, Math.Ceiling(d - (increment / 2)));
                var upperBound = (int)Math.Min(spectrumCopy.Count - 1, Math.Floor(d + (increment / 2)));

                var bandRanges = new List<int>();
                for (var i = lowerBound; i <= upperBound; i++)
                {
                    bandRanges.Add((int)(_spectrumMax[i] - _spectrumMin[i]));
                }
                if (bandRanges.Count > 0)
                {
                    mostActiveBands.Add(lowerBound + bandRanges.IndexOf(bandRanges.Max()));
                }
            }

            var filteredSpectrum = mostActiveBands.Select(t => spectrumCopy[t]).ToList();
            RackContainer.OutputPipe("Filtered Spectrum", filteredSpectrum, iteration);
        }

        private void ChannelCountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            if (slider == null) return;
            _numBars = (int)slider.Value;
        }

        public override IRackItem CreateRackItem()
        {
            return new FilterSpectrumItem();
        }

        public override Dictionary<string, Pipe> GetInputs()
        {
            var inputs = new Dictionary<string, Pipe> {{"Unfiltered Spectrum", SpectrumPipeIn}};
            return inputs;
        }

        public override List<string> GetOutputs()
        {
            var outputs = new List<string> {"Filtered Spectrum"};
            return outputs;
        }

        public override void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement(RackItemName + "-" + ItemName));
            node.AppendChild(xml.CreateElement("DecayValue")).InnerText = DecayUpDown.Value.ToString();
            node.AppendChild(xml.CreateElement("NumberOfBars")).InnerText = ChannelCountSlider.Value.ToString(CultureInfo.InvariantCulture);
            SaveInputs(xml, node);
            SaveOutputs(xml, node);
        }

        public override void Load(XmlNode xml)
        {
            for (var i = 0; i < xml.ChildNodes.Count; i++)
            {
                var node = xml.ChildNodes.Item(i);
                if (node == null) continue;

                switch (node.Name)
                {
                    case "DecayValue":
                        int decay;
                        if (int.TryParse(node.InnerText, out decay))
                        {
                            DecayUpDown.Value = decay;
                        }
                        break;
                    case "NumberOfBars":
                        double numberOfBars;
                        if (double.TryParse(node.InnerText, out numberOfBars))
                        {
                            ChannelCountSlider.Value = numberOfBars;
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

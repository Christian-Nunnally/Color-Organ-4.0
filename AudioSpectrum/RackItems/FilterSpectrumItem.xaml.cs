using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using AudioSpectrum.SideRailContainers;
using Xceed.Wpf.Toolkit;

namespace AudioSpectrum.RackItems
{
    public partial class FilterSpectrumItem : RackItemBase
    {
        private int _maxBars;
        private int _numBars;

        private readonly List<double> _spectrumMax = new List<double>();
        private readonly List<double> _spectrumMin = new List<double>();

        private readonly Slider _channelCountSlider = new ColorSpectrumSlider();
        private readonly DoubleUpDown _decayUpDown = new DoubleUpDown();

        public FilterSpectrumItem(XmlNode xml)
        {
            InitializeComponent();
            ItemName = "FilterSpectrum";

            _channelCountSlider.ValueChanged += ChannelCountSlider_ValueChanged;
            _channelCountSlider.Orientation = Orientation.Horizontal;
            _channelCountSlider.IsSnapToTickEnabled = true;
            _channelCountSlider.Background = Brushes.White;
            _channelCountSlider.Height = 20;
            _decayUpDown.Minimum = 0.05;
            _decayUpDown.Maximum = 5.00;
            _decayUpDown.Increment = 0.05;
            _decayUpDown.Value = 1.00;

            if (xml == null)
            {
                AddInput(new RackItemInput("Unfiltered Spectrum", SpectrumPipeIn));
                AddOutput(new RackItemOutput("Filtered Spectrum"));
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
                _sideRailControls = new List<Control>();
                _sideRailControls.Add(new LabeledControlSideRailContainer("Filtered Channel Count", _channelCountSlider, Orientation.Vertical, _channelCountSlider.Height + 4));
                _sideRailControls.Add(new LabeledControlSideRailContainer("Normalization Decay", _decayUpDown, Orientation.Horizontal, 70));
            }
            sideRailSetter.Invoke(ItemName, _sideRailControls);
        }

        private void SpectrumPipeIn(List<byte> spectrum, int iteration)
        {
            var spectrumCopy = spectrum.ToList();
            _maxBars = spectrumCopy.Count;
            _channelCountSlider.Maximum = _maxBars;

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
                _spectrumMax[i] -= _decayUpDown.Value.Value;
                _spectrumMin[i] += _decayUpDown.Value.Value;
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

            if (RackItemOutputs.Count > 0)
            {
                RackContainer.OutputPipe(RackItemOutputs.First(), filteredSpectrum, iteration);
            }
        }

        private void ChannelCountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            if (slider == null) return;
            _numBars = (int)slider.Value;
        }

        public override IRackItem CreateRackItem(XmlElement xml)
        {
            return new FilterSpectrumItem(xml);
        }

        public override void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement(RackItemName + "-" + ItemName));
            node.AppendChild(xml.CreateElement("DecayValue")).InnerText = _decayUpDown.Value.ToString();
            node.AppendChild(xml.CreateElement("NumberOfBars")).InnerText = _channelCountSlider.Value.ToString(CultureInfo.InvariantCulture);
            SaveInputs(xml, node);
            SaveOutputs(xml, node);
        }

        public sealed override void Load(XmlNode xml)
        {
            LoadInputsAndOutputs(xml);
            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
            {
                switch (node.Name)
                {
                    case "DecayValue":
                        int decay;
                        if (int.TryParse(node.InnerText, out decay))
                        {
                            _decayUpDown.Value = decay;
                        }
                        break;
                    case "NumberOfBars":
                        double numberOfBars;
                        if (double.TryParse(node.InnerText, out numberOfBars))
                        {
                            _channelCountSlider.Value = numberOfBars;
                        }
                        break;
                }
            }
        }
    }
}

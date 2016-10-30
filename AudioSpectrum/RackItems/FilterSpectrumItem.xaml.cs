using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AudioSpectrum.RackItems
{
    public partial class FilterSpectrumItem : UserControl, IRackItem
    {
        private int _maxBars;
        private int _numBars;

        private RackItemContainer _rack;

        private const int Decay = 2;

        private readonly List<byte> _spectrumMax = new List<byte>();
        private readonly List<byte> _spectrumMin = new List<byte>();

        public string ItemName => "Filter Spectrum";

        public FilterSpectrumItem()
        {
            InitializeComponent();
        }

        public void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            sideRailSetter.Invoke(ItemName, new List<Control>());
        }

        private void SpectrumPipeIn(List<byte> spectrum, int iteration)
        {
            var spectrumCopy = spectrum.ToList();
            _maxBars = spectrumCopy.Count;

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
                _spectrumMax[i] -= Decay;
                _spectrumMin[i] += Decay;
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
                    bandRanges.Add(_spectrumMax[i] - _spectrumMin[i]);
                }
                if (bandRanges.Count > 0)
                {
                    mostActiveBands.Add(lowerBound + bandRanges.IndexOf(bandRanges.Max()));
                }
            }

            var filteredSpectrum = mostActiveBands.Select(t => spectrumCopy[t]).ToList();
            _rack.OutputPipe("Filtered Spectrum", filteredSpectrum, iteration);
        }

        private void ChannelCountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            if (slider == null) return;
            slider.Maximum = _maxBars;
            _numBars = (int)slider.Value;
        }

        public IRackItem CreateRackItem()
        {
            return new FilterSpectrumItem();
        }

        public Dictionary<string, Pipe> GetInputs()
        {
            var inputs = new Dictionary<string, Pipe> {{"Unfiltered Spectrum", SpectrumPipeIn}};
            return inputs;
        }

        public List<string> GetOutputs()
        {
            var outputs = new List<string> {"Filtered Spectrum"};
            return outputs;
        }

        public void SetRack(RackItemContainer rack)
        {
            _rack = rack;
        }

        public void CleanUp()
        {
        }

        public bool CanDelete()
        {
            return true;
        }

        public void HeartBeat()
        {
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AudioSpectrum.RackItems
{
    public partial class SpectrumToBinaryDataItem : UserControl, IRackItem
    {
        private RackItemContainer _rack;
        private bool _normalize;
        private readonly List<double> _maxs = new List<double>();
        private readonly List<double> _mins = new List<double>();

        public string ItemName => "Spectrum to Binary";

        public SpectrumToBinaryDataItem()
        {
            InitializeComponent();
        }

        public bool CanDelete()
        {
            return true;
        }

        public void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            sideRailSetter?.Invoke(ItemName, new List<Control>());
        }

        public void HeartBeat()
        {
        }

        public void CleanUp()
        {
        }

        public IRackItem CreateRackItem()
        {
            return new SpectrumToBinaryDataItem();
        }

        public Dictionary<string, Pipe> GetInputs()
        {
            var inputs = new Dictionary<string, Pipe> {{"Spectrum Input", SpectrumInput}};
            return inputs;
        }

        private void SpectrumInput(List<byte> data, int iteration)
        {
            var binaryData = new List<byte>();

            if (_normalize)
            {
                while (_maxs.Count < data.Count) _maxs.Add(0);
                while (_mins.Count < data.Count) _mins.Add(255);

                for (var i = 0; i < data.Count; i++)
                {
                    _maxs[i] = Math.Max(data[i], _maxs[i]);
                    _mins[i] = Math.Min(data[i], _mins[i]);
                    if (NormalizationDecayDoubleUpDown.Value == null) continue;
                    _maxs[i] -= (double)NormalizationDecayDoubleUpDown.Value.Value;
                    _mins[i] += (double)NormalizationDecayDoubleUpDown.Value.Value;

                    binaryData.Add((byte)(Math.Abs(_maxs[i] - _mins[i]) * ActivationPrecentDoubleUpDown.Value) + _mins[i] > data[i] ? (byte)0 : (byte)1);
                }
            }
            else
            {
                binaryData.AddRange(data.Select(@byte => (byte) (255*ActivationPrecentDoubleUpDown.Value) > @byte ? (byte) 0 : (byte) 1));
            }

            _rack.OutputPipe("Binary Data Out", binaryData, iteration);
        }

        public List<string> GetOutputs()
        {
            var outputs = new List<string> {"Binary Data Out"};
            return outputs;
        }

        public void SetRack(RackItemContainer rack)
        {
            _rack = rack;
        }

        private void NormalizeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (NormalizeCheckbox.IsChecked != null) _normalize = NormalizeCheckbox.IsChecked.Value;
        }
    }
}

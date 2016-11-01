using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    public partial class SpectrumToBinaryDataItem : RackItemBase
    {
        private bool _normalize;
        private readonly List<double> _maxs = new List<double>();
        private readonly List<double> _mins = new List<double>();

        private readonly List<byte> _binaryData = new List<byte>();

        public SpectrumToBinaryDataItem()
        {
            InitializeComponent();
            ItemName = "Spectrum to Binary";
        }

        public override void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            sideRailSetter?.Invoke(ItemName, new List<Control>());
        }

        public override IRackItem CreateRackItem()
        {
            return new SpectrumToBinaryDataItem();
        }

        public override Dictionary<string, Pipe> GetInputs()
        {
            var inputs = new Dictionary<string, Pipe> {{"Spectrum Input", SpectrumInput}};
            return inputs;
        }

        private void SpectrumInput(List<byte> data, int iteration)
        {
            while (_binaryData.Count < data.Count) _binaryData.Add(0);
            while (_binaryData.Count > data.Count) _binaryData.RemoveAt(_binaryData.Count - 1);

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

                    if ((Math.Abs(_maxs[i] - _mins[i]) * ActivationPrecentDoubleUpDown.Value) + _mins[i] < data[i])
                    {
                        _binaryData[i] = 1;
                    }
                    else if ((Math.Abs(_maxs[i] - _mins[i]) * DeactivationPrecentDoubleUpDown.Value) + _mins[i] > data[i])
                    {
                        _binaryData[i] = 0;
                    }
                }
            }
            else
            {
                for (var i = 0; i < data.Count; i++)
                {
                    if (255 * ActivationPrecentDoubleUpDown.Value < data[i])
                    {
                        _binaryData[i] = 1;
                    }
                    else if (255 * DeactivationPrecentDoubleUpDown.Value > data[i])
                    {
                        _binaryData[i] = 0;
                    }
                }
            }

            RackContainer.OutputPipe("Binary Data Out", _binaryData, iteration);
        }

        public override List<string> GetOutputs()
        {
            var outputs = new List<string> {"Binary Data Out"};
            return outputs;
        }

        private void NormalizeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (NormalizeCheckbox.IsChecked != null) _normalize = NormalizeCheckbox.IsChecked.Value;
        }

        private void DeactivationPrecentDoubleUpDown_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ActivationPrecentDoubleUpDown == null) return;

            if (ActivationPrecentDoubleUpDown.Value < DeactivationPrecentDoubleUpDown.Value)
            {
                ActivationPrecentDoubleUpDown.Value = DeactivationPrecentDoubleUpDown.Value;
            }
        }

        private void ActivationPrecentDoubleUpDown_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ActivationPrecentDoubleUpDown.Value < DeactivationPrecentDoubleUpDown?.Value)
            {
                DeactivationPrecentDoubleUpDown.Value =  ActivationPrecentDoubleUpDown.Value;
            }
        }

        public override void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement("SpectrumToBinaryDataItem"));
        }

        public override void Load(XmlElement xml)
        {
            throw new NotImplementedException();
        }
    }
}

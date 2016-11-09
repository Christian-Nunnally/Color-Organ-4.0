using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    public partial class SpectrumToBinaryDataItem : RackItemBase
    {
        private bool _normalize;
        private readonly List<double> _maxs = new List<double>();
        private readonly List<double> _mins = new List<double>();

        private readonly List<byte> _binaryData = new List<byte>();

        public SpectrumToBinaryDataItem(XmlNode xml)
        {
            InitializeComponent();
            ItemName = "SpectrumToBinary";

            if (xml == null)
            {
                AddInput(new RackItemInput("Spectrum Input", SpectrumInput));
                AddOutput(new RackItemOutput("Binary Data Out"));
            }
            else
            {
                Load(xml);
            }
        }

        public override IRackItem CreateRackItem(XmlElement xml)
        {
            return new SpectrumToBinaryDataItem(xml);
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

            if (RackItemOutputs.Count > 0)
            {
                RackContainer.OutputPipe(RackItemOutputs.First(), _binaryData, iteration);
            }
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
            var node = parent.AppendChild(xml.CreateElement(RackItemName + "-" + ItemName));
            node.AppendChild(xml.CreateElement("ActivationPrecent")).InnerText = ActivationPrecentDoubleUpDown.Value.ToString();
            node.AppendChild(xml.CreateElement("DeactivationPrecent")).InnerText = DeactivationPrecentDoubleUpDown.Value.ToString();
            node.AppendChild(xml.CreateElement("Normalize")).InnerText = (NormalizeCheckbox.IsChecked != null && NormalizeCheckbox.IsChecked.Value).ToString();
            SaveOutputs(xml, node);
            SaveInputs(xml, node);
        }

        public sealed override void Load(XmlNode xml)
        {
            LoadInputsAndOutputs(xml);
            AttachPipeToInput(1, SpectrumInput);

            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
            {
                switch (node.Name)
                {
                    case "ActivationPrecent":
                        ActivationPrecentDoubleUpDown.Value = double.Parse(node.InnerText);
                        break;
                    case "DeactivationPrecent":
                        DeactivationPrecentDoubleUpDown.Value = double.Parse(node.InnerText);
                        break;
                    case "Normalize":
                        NormalizeCheckbox.IsChecked = bool.Parse(node.InnerText);
                        break;
                }
            }
        }
    }
}

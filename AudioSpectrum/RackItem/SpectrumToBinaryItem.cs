using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using AudioSpectrum.Window.SideRailContainer;
using Xceed.Wpf.Toolkit;

namespace AudioSpectrum.RackItem
{
    public class SpectrumToBinaryItem : RackItemBase
    {
        private readonly DoubleUpDown _activationPercentDoubleUpDown = new DoubleUpDown();

        private readonly List<byte> _binaryData = new List<byte>();
        private readonly DoubleUpDown _deactivationPercentDoubleUpDown = new DoubleUpDown();
        private readonly List<double> _maxs = new List<double>();
        private readonly List<double> _mins = new List<double>();
        private readonly CheckBox _normalizationCheckBox = new CheckBox();
        private readonly DoubleUpDown _normalizationDecayDoubleUpDown = new DoubleUpDown();
        private bool _normalize = true;

        public SpectrumToBinaryItem(XmlNode xml)
        {
            ItemName = "SpectrumToBinary";

            _activationPercentDoubleUpDown.Minimum = 0;
            _activationPercentDoubleUpDown.Maximum = 1;
            _activationPercentDoubleUpDown.Increment = 0.01;
            _activationPercentDoubleUpDown.Value = 0.90;
            _activationPercentDoubleUpDown.ValueChanged += ActivationPercentDoubleUpDownOnValueChanged;

            _deactivationPercentDoubleUpDown.Minimum = 0;
            _deactivationPercentDoubleUpDown.Maximum = 1;
            _deactivationPercentDoubleUpDown.Increment = 0.01;
            _deactivationPercentDoubleUpDown.Value = 0.50;
            _deactivationPercentDoubleUpDown.ValueChanged += DeactivationPercentDoubleUpDownOnValueChanged;

            _normalizationDecayDoubleUpDown.Minimum = 0.0;
            _normalizationDecayDoubleUpDown.Maximum = 5.0;
            _normalizationDecayDoubleUpDown.Value = 0.5;
            _normalizationDecayDoubleUpDown.Increment = 0.05;

            _normalizationCheckBox.Checked += NormalizationCheckBox_Checked;
            _normalizationCheckBox.IsChecked = true;

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
            return new SpectrumToBinaryItem(xml);
        }

        private void SpectrumInput(List<byte> data)
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
                    if (_normalizationDecayDoubleUpDown.Value == null) continue;
                    _maxs[i] -= _normalizationDecayDoubleUpDown.Value.Value;
                    _mins[i] += _normalizationDecayDoubleUpDown.Value.Value;

                    if (Math.Abs(_maxs[i] - _mins[i]) * _activationPercentDoubleUpDown.Value + _mins[i] < data[i])
                        _binaryData[i] = 1;
                    else if (Math.Abs(_maxs[i] - _mins[i]) * _deactivationPercentDoubleUpDown.Value + _mins[i] > data[i])
                        _binaryData[i] = 0;
                }
            }
            else
            {
                for (var i = 0; i < data.Count; i++)
                    if (255 * _activationPercentDoubleUpDown.Value < data[i])
                        _binaryData[i] = 1;
                    else if (255 * _deactivationPercentDoubleUpDown.Value > data[i])
                        _binaryData[i] = 0;
            }

            if (RackItemOutputs.Count > 0)
                RackContainer.OutputPipe(RackItemOutputs.First(), _binaryData);
        }

        private void NormalizationCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_normalizationCheckBox.IsChecked != null) _normalize = _normalizationCheckBox.IsChecked.Value;
        }

        private void DeactivationPercentDoubleUpDownOnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_activationPercentDoubleUpDown?.Value < _deactivationPercentDoubleUpDown.Value)
                _activationPercentDoubleUpDown.Value = _deactivationPercentDoubleUpDown.Value;
        }

        private void ActivationPercentDoubleUpDownOnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_activationPercentDoubleUpDown.Value < _deactivationPercentDoubleUpDown?.Value)
                _deactivationPercentDoubleUpDown.Value = _activationPercentDoubleUpDown.Value;
        }

        public override void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement(RackItemName + "-" + ItemName));
            node.AppendChild(xml.CreateElement("ActivationPercent")).InnerText = _activationPercentDoubleUpDown.Value.ToString();
            node.AppendChild(xml.CreateElement("DeactivationPercent")).InnerText = _deactivationPercentDoubleUpDown.Value.ToString();
            node.AppendChild(xml.CreateElement("NormalizationDecay")).InnerText = _normalizationDecayDoubleUpDown.Value.ToString();
            node.AppendChild(xml.CreateElement("Normalize")).InnerText = ((_normalizationCheckBox.IsChecked != null) && _normalizationCheckBox.IsChecked.Value).ToString();
            SaveOutputs(xml, node);
            SaveInputs(xml, node);
        }

        public override void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            var controls = new List<UIElement>();
            controls.Add(new LabeledControlSideRailContainer("Activation Percent", _activationPercentDoubleUpDown, Orientation.Horizontal, 60));
            controls.Add(new LabeledControlSideRailContainer("Deactivation Percent", _deactivationPercentDoubleUpDown, Orientation.Horizontal, 60));
            controls.Add(new LabeledControlSideRailContainer("Normalizaton Enabled", _normalizationCheckBox, Orientation.Horizontal, 60));
            controls.Add(new LabeledControlSideRailContainer("Normalizaton Decay", _normalizationDecayDoubleUpDown, Orientation.Horizontal, 60));
            sideRailSetter.Invoke(ItemName, controls);
        }

        public sealed override void Load(XmlNode xml)
        {
            LoadInputsAndOutputs(xml);
            AttachPipeToInput(1, SpectrumInput);

            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
                switch (node.Name)
                {
                    case "ActivationPercent":
                        _activationPercentDoubleUpDown.Value = double.Parse(node.InnerText);
                        break;
                    case "DeactivationPercent":
                        _deactivationPercentDoubleUpDown.Value = double.Parse(node.InnerText);
                        break;
                    case "NormalizationDecay":
                        _normalizationDecayDoubleUpDown.Value = double.Parse(node.InnerText);
                        break;
                    case "Normalize":
                        _normalizationCheckBox.IsChecked = bool.Parse(node.InnerText);
                        break;
                }
        }
    }
}
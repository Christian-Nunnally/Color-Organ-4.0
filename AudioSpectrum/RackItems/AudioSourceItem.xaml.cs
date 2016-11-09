using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using AudioSpectrum.SideRailContainers;
using Xceed.Wpf.Toolkit;

namespace AudioSpectrum.RackItems
{
    public partial class AudioSourceItem : RackItemBase
    {
        private readonly ComboBox _deviceSelectionBox = new ComboBox();
        private readonly IntegerUpDown _numberOfLinesUpDown = new IntegerUpDown();

        private static Analyzer Analyzer { get; set; }

        private bool _enabled;

        public AudioSourceItem(XmlNode xml)
        {
            if (Analyzer == null)
            {
                Analyzer = new Analyzer();
            }

            InitializeComponent();
            ItemName = "AudioSource";

            _numberOfLinesUpDown.Minimum = 1;
            _numberOfLinesUpDown.Maximum = 64;
            _numberOfLinesUpDown.Increment = 1;
            _numberOfLinesUpDown.Value = 16;
            _numberOfLinesUpDown.ValueChanged += LinesUpDownValueChanged;

            if (xml == null)
            {
                AddOutput(new RackItemOutput("Audio Source"));
            }
            else
            {
                Load(xml);
            }

            Analyzer.AnalyerDataReady += SendAudioData;
            Analyzer.InitDeviceComboBox(_deviceSelectionBox);
        }

        private List<Control> _sideRailControls;

        public override void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            if (_sideRailControls == null)
            {
                _sideRailControls = new List<Control>
                {
                    new LabeledControlSideRailContainer("Device", _deviceSelectionBox, Orientation.Horizontal, 180),
                    new LabeledControlSideRailContainer("Number of lines", _numberOfLinesUpDown, Orientation.Horizontal,
                        70)
                };
            }

            sideRailSetter.Invoke(ItemName, _sideRailControls);
        }

        private void EnableButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string) EnableButton.Content == "Disable Source")
            {
                EnableButton.Content = "Enable Source";
                Analyzer.Disable(EnableButton, _deviceSelectionBox);
                EnabledIndicator.Fill = new SolidColorBrush(Color.FromRgb(255, 85, 85));
                _enabled = false;
            }
            else
            {
                EnableButton.Content = "Disable Source";
                Analyzer.Enable(EnableButton, _deviceSelectionBox);
                EnabledIndicator.Fill = new SolidColorBrush(Color.FromRgb(85, 255, 85));
                _enabled = true;
            }
        }

        private void SendAudioData(List<byte> data)
        {
            if (!_enabled) return;
            if (RackItemOutputs.Count > 0)
            {
                RackContainer?.OutputPipe(RackItemOutputs.First(), data, 0);
            }
        }

        public override IRackItem CreateRackItem(XmlElement xml)
        {
            return new AudioSourceItem(xml);
        }

        private void LinesUpDownValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_numberOfLinesUpDown.Value != null && Analyzer != null) Analyzer.Lines = _numberOfLinesUpDown.Value.Value;
        }

        public override void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement(RackItemName + "-" + ItemName));
            SaveOutputs(xml, node);
            SaveInputs(xml, node);
        }

        public sealed override void Load(XmlNode xml)
        {
            LoadInputsAndOutputs(xml);
        }


    }
}

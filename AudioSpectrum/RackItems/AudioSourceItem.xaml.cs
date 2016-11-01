using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    public partial class AudioSourceItem : RackItemBase
    {
        private static Analyzer Analyzer { get; set; }

        public AudioSourceItem()
        {
            InitializeComponent();
            ItemName = "Audio Source";
        }

        public override void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            sideRailSetter.Invoke(ItemName, new List<Control>());
        }

        private void EnableButton_Click(object sender, RoutedEventArgs e)
        {
            if (Analyzer == null)
            {
                Analyzer = new Analyzer(SendAudioData, DeviceSelectionBox);
            }
            if (EnableButton.IsChecked == true)
            {
                EnableButton.Content = "Disable";
                Analyzer.Enable = true;
            }
            else
            {
                Analyzer.Enable = false;
                EnableButton.Content = "Enable";
            }
        }

        private void SendAudioData(List<byte> data)
        {
            RackContainer?.OutputPipe("Audio Source", data, 0);
        }

        public override List<string> GetOutputs()
        {
            var outputs = new List<string> {"Audio Source"};
            return outputs;
        }

        public override IRackItem CreateRackItem()
        {
            return new AudioSourceItem();
        }

        public override Dictionary<string, Pipe> GetInputs()
        {
            return new Dictionary<string, Pipe>();
        }

        private void LinesUpDownValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (LinesUpDown.Value != null && Analyzer != null) Analyzer.Lines = LinesUpDown.Value.Value;
        }

        public override void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement("AudioSourceItem"));
        }

        public override void Load(XmlElement xml)
        {
            throw new System.NotImplementedException();
        }
    }
}

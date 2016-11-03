using System.Collections.Generic;
using System.Linq;
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
            ItemName = "AudioSource";
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
            var node = parent.AppendChild(xml.CreateElement(RackItemName + "-" + ItemName));
            SaveOutputs(xml, node);
            SaveInputs(xml, node);
        }

        public override void Load(XmlNode xml)
        {
            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
            {
                switch (node.Name)
                {
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

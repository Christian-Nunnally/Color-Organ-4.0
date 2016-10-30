using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AudioSpectrum.RackItems
{
    public partial class AudioSourceItem : UserControl, IRackItem
    {
        private static Analyzer Analyzer { get; set; }

        private RackItemContainer _parentRack;

        public string ItemName => "Audio Source";

        public AudioSourceItem()
        {
            InitializeComponent();
        }

        public void SetSideRail(SetSideRailDelegate sideRailSetter)
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
            _parentRack?.OutputPipe("Audio Source", data, 0);
        }

        public List<string> GetOutputs()
        {
            var outputs = new List<string> {"Audio Source"};
            return outputs;
        }

        public IRackItem CreateRackItem()
        {
            return new AudioSourceItem();
        }

        Dictionary<string, Pipe> IRackItem.GetInputs()
        {
            return new Dictionary<string, Pipe>();
        }

        public void SetRack(RackItemContainer rack)
        {
            _parentRack = rack;
        }

        public void CleanUp()
        {
            //Analyzer.Free();
        }

        public bool CanDelete()
        {
            return true;
        }

        public void HeartBeat()
        {
            
        }

        private void LinesUpDownValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (LinesUpDown.Value != null && Analyzer != null) Analyzer.Lines = LinesUpDown.Value.Value;
        }
    }
}

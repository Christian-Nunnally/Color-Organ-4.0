using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace AudioSpectrum.RackItems
{
    public partial class AudioProcessorItem : UserControl, IRackItem
    {
        private RackItemContainer _rack;

        private readonly List<List<byte>> _history = new List<List<byte>>();
        private int _current;

        public string ItemName => "Audio Processor";

        public AudioProcessorItem()
        {
            InitializeComponent();
        }

        public void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            sideRailSetter.Invoke(ItemName, new List<Control>());
        }

        public bool CanDelete()
        {
            return true;
        }

        public void HeartBeat()
        {
            
        }

        public void CleanUp()
        {
        }

        public IRackItem CreateRackItem()
        {
            return new AudioProcessorItem();
        }

        public Dictionary<string, Pipe> GetInputs()
        {
            var inputs = new Dictionary<string, Pipe> {{"Spectrum Input", SpectrumInput}};
            return inputs;
        }

        public List<string> GetOutputs()
        {
            var outputs = new List<string> { "Processed Spectrum" };
            return outputs;
        }

        public void SetRack(RackItemContainer rack)
        {
            _rack = rack;
        }

        private void SpectrumInput(List<byte> data, int iteration)
        {
            if (NumberOfSamplesUpDown.Value == null) return;
            var samples = NumberOfSamplesUpDown.Value.Value;
            while (_history.Count < samples) _history.Add(new List<byte>());
            while (_history.Count > samples) _history.RemoveAt(_history.Count - 1);

            _history[++_current % _history.Count] = data;

            var processedDataInt = new List<int>();
            for (var i = 0; i < data.Count; i++) processedDataInt.Add(0);
            foreach (var t in _history)
            {
                for (var j = 0; j < t.Count; j++)
                {
                    processedDataInt[j] += t[j];
                }
            }

            var processedData = processedDataInt.Select(t => (byte) (t/samples)).ToList();
            _rack.OutputPipe("Processed Spectrum", processedData, iteration);
        }
    }
}

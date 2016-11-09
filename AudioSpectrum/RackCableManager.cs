 using System;
using System.Collections.Generic;
 using System.Collections.ObjectModel;
 using System.Threading;
using System.Windows.Controls;
using AudioSpectrum.RackItems;

namespace AudioSpectrum
{
    public delegate void MoreOutputsLoaded();

    public class RackCableManager
    {

        private readonly List<RackItemContainer> _racks = new List<RackItemContainer>();
        private readonly Dictionary<RackItemOutput, List<RackItemInput>> _outputToInputs = new Dictionary<RackItemOutput, List<RackItemInput>>();

        public readonly ObservableCollection<RackItemOutput> AllRackItemOutputs = new ObservableCollection<RackItemOutput>();
        public readonly ObservableCollection<RackItemInput> AllRackItemInputs = new ObservableCollection<RackItemInput>();

        public event MoreOutputsLoaded MoreOutputsLoadedDelegate;

        public RackCableManager()
        {
            ThreadStart heartbeatStart = Heartbeat;
            var heartbeatThread = new Thread(heartbeatStart);
            heartbeatThread.Start();
        }

        public void AddRack(RackItemContainer rack)
        {
            _racks.Add(rack);
        }

        public void RackContentSet(RackItemContainer rack)
        {
            foreach (var output in rack.GetOutputs())
            {
                AllRackItemOutputs.Add(output);
            }
            MoreOutputsLoadedDelegate?.Invoke();

            foreach (var input in rack.GetInputs())
            {
                AllRackItemInputs.Add(input);
            }
        }

        public void RemoveRack(RackItemContainer rack)
        {
            foreach (var output in rack.GetOutputs())
            {
                AllRackItemOutputs.Remove(output);
            }

            foreach (var input in rack.GetInputs())
            {
                AllRackItemInputs.Remove(input);
            }

            _racks.Remove(rack);
        }

        public void OutputPipe(RackItemOutput output, List<byte> data, int iteration)
        {
            if (iteration > 1000)
            {
                return;
            }

            if (_outputToInputs.ContainsKey(output))
            {
                foreach (var input in _outputToInputs[output])
                {
                    input.Pipe.Invoke(data, iteration++);
                }
            }
        }

        public void InputSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var inputComboBox = (ComboBox) sender;
            var input = (RackItemInput) inputComboBox.Tag;
            var output = (RackItemOutput) inputComboBox.SelectedValue;

            if (e.RemovedItems.Count > 0)
            {
                var unselectedItem = (RackItemOutput)e.RemovedItems[0];
                if (_outputToInputs.ContainsKey(unselectedItem))
                {
                    _outputToInputs[unselectedItem].Remove(input);
                }
            }

            input.ConnectedOutput = "";
            if (output == null) return;

            if (!_outputToInputs.ContainsKey(output))
            {
                _outputToInputs.Add(output, new List<RackItemInput>());
            }

            input.ConnectedOutput = output.VisibleName;
            _outputToInputs[output].Add(input);

            foreach (var rackItemContainer in _racks)
            {
                rackItemContainer.InputSelectorItemsChanged();
            }
        }

        public void OutputTextBoxTextChanged(object sender, EventArgs e)
        {
            var outputTextBox = (TextBox)sender;
            var rackItemOutput = (RackItemOutput) outputTextBox.Tag;

            if (_outputToInputs.ContainsKey(rackItemOutput))
            {
                _outputToInputs[rackItemOutput].Clear();
            }
            rackItemOutput.VisibleName = outputTextBox.Text;
            
        }

        private void Heartbeat()
        {
            while (true)
            {
                for (var i = _racks.Count - 1; i >= 0; i--)
                {
                    if (i >= _racks.Count) continue;
                    var rackItem = _racks[i].RackItem;
                    rackItem?.HeartBeat();
                }
                Thread.Sleep(50);
            }
        }
    }
}

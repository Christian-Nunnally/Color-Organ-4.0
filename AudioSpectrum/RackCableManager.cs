using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using AudioSpectrum.RackItems;

namespace AudioSpectrum
{
    public class RackCableManager
    {
        private readonly List<RackItemContainer> _racks = new List<RackItemContainer>();
        private readonly Dictionary<string, List<Pipe>> _outputToInputs = new Dictionary<string, List<Pipe>>();
        private readonly Dictionary<string, Pipe> _inputNameToInputPipe = new Dictionary<string, Pipe>();

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
            foreach (var kv in rack.GetInputs())
            {
                var i = 0;
                while (_inputNameToInputPipe.ContainsKey(kv.Key + (i == 0 ? "" : i.ToString()))) { i++; }

                _inputNameToInputPipe.Add(kv.Key + (i == 0 ? "" : i.ToString()), kv.Value);
            }

            foreach (var outputName in rack.GetExternalOutputNames())
            {
                var i = 0;
                while (_outputToInputs.ContainsKey(outputName + (i == 0 ? "" : i.ToString()))) { i++; }

                _outputToInputs.Add(outputName + (i == 0 ? "" : i.ToString()), new List<Pipe>());
            }

            UpdateInputLists();
        }

        public void RemoveRack(RackItemContainer rack)
        {
            _racks.Remove(rack);
            UpdateInputLists();
        }

        public void OutputPipe(string outputName, List<byte> data, int iteration)
        {
            if (iteration > 1000)
            {
                return;
            }

            foreach (var inputPipe in _outputToInputs[outputName])
            {
                inputPipe.Invoke(data, iteration++);
            }
        }

        public List<string> GetOutputNameList()
        {
            var outputNames = new List<string>();

            foreach (var rack in _racks)
            {
                outputNames.AddRange(rack.GetExternalOutputNames());
            }

            return outputNames;
        }

        private void UpdateInputLists()
        {
            var outputNames = GetOutputNameList();

            foreach (var rack in _racks)
            {
                rack.UpdateInputSelectors(outputNames);
            }
        }

        public void InputSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var inputComboBox = sender as ComboBox;
            if (inputComboBox == null) return;
            var inputName = (string)inputComboBox.Tag;

            if (e.RemovedItems.Count > 0)
            {
                var unselectedItem = (string)e.RemovedItems[0];
                if (_outputToInputs.ContainsKey(unselectedItem))
                {
                    _outputToInputs[unselectedItem].Remove(_inputNameToInputPipe[inputName]);
                }
            }

            if (e.AddedItems.Count <= 0) return;
            var selectedItem = (string)e.AddedItems[0];
            _outputToInputs[selectedItem].Add(_inputNameToInputPipe[inputName]);
        }

        public void OutputTextBoxTextChanged(object sender, EventArgs e)
        {
            var outputTextBox = (TextBox)sender;

            if (_outputToInputs.ContainsKey(outputTextBox.Text))
            {
                outputTextBox.Text = (string)outputTextBox.Tag;
                return;
            }

            var oldName = (string)outputTextBox.Tag;
            outputTextBox.Tag = outputTextBox.Text;

            var oldInputPipes = new List<Pipe>();
            if (oldName != null && _outputToInputs.ContainsKey(oldName))
            {
                oldInputPipes = _outputToInputs[oldName];
                _outputToInputs.Remove(oldName);
            }

            _outputToInputs.Add(outputTextBox.Text, oldInputPipes);
            UpdateInputLists();
        }

        private void Heartbeat()
        {
            while (true)
            {
                foreach (var rackItemContainer in _racks)
                {
                    var rackItem = rackItemContainer.RackItem;
                    rackItem?.HeartBeat();
                }
                Thread.Sleep(50);
            }
        }
    }
}

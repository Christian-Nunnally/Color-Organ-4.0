using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using AudioSpectrum.RackItems;

namespace AudioSpectrum
{
    public delegate void MoreOutputsLoaded();

    public class RackCableManager
    {
        private readonly Dictionary<RackItemOutput, List<RackItemInput>> _outputToInputs = new Dictionary<RackItemOutput, List<RackItemInput>>();
        private readonly List<RackItemContainer> _racks = new List<RackItemContainer>();
        public readonly ObservableCollection<RackItemOutput> AllRackItemOutputs = new ObservableCollection<RackItemOutput>();

        public event MoreOutputsLoaded MoreOutputsLoadedDelegate;

        public void AddRack(RackItemContainer rack)
        {
            _racks.Add(rack);
        }

        public void RackContentSet(RackItemContainer rack)
        {
            foreach (var output in rack.GetOutputs())
                AllRackItemOutputs.Add(output);
            MoreOutputsLoadedDelegate?.Invoke();
        }

        public void RemoveRack(RackItemContainer rack)
        {
            foreach (var output in rack.GetOutputs())
                AllRackItemOutputs.Remove(output);

            _racks.Remove(rack);
        }

        public void OutputPipe(RackItemOutput output, List<byte> data)
        {
            if (!_outputToInputs.ContainsKey(output)) return;
            foreach (var input in _outputToInputs[output])
                input.Pipe.Invoke(data);
        }

        public void InputSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var inputComboBox = (ComboBox)sender;
            var input = (RackItemInput)inputComboBox.Tag;
            var output = (RackItemOutput)inputComboBox.SelectedValue;

            if (e.RemovedItems.Count > 0)
            {
                var unselectedItem = (RackItemOutput)e.RemovedItems[0];
                if (_outputToInputs.ContainsKey(unselectedItem))
                    _outputToInputs[unselectedItem].Remove(input);
            }

            input.ConnectedOutput = string.Empty;
            if (output == null) return;

            if (!_outputToInputs.ContainsKey(output))
                _outputToInputs.Add(output, new List<RackItemInput>());

            input.ConnectedOutput = output.VisibleName;
            _outputToInputs[output].Add(input);

            foreach (var rackItemContainer in _racks)
                rackItemContainer.InputSelectorItemsChanged();
        }

        public void OutputTextBoxTextChanged(object sender, EventArgs e)
        {
            var outputTextBox = (TextBox)sender;
            var rackItemOutput = (RackItemOutput)outputTextBox.Tag;

            if (_outputToInputs.ContainsKey(rackItemOutput))
                _outputToInputs[rackItemOutput].Clear();
            rackItemOutput.VisibleName = outputTextBox.Text;
        }
    }
}
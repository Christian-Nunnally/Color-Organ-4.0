using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace AudioSpectrum.RackItems
{
    [ContentProperty("AdditionalContent")]
    public partial class RackItemContainer : UserControl
    {
        private static int _rackId;
        private readonly int _id;

        public StackPanel ContainingPanel;
        public IRackItem RackItem { get; }

        private readonly Dictionary<string, ComboBox> _inputSelectors = new Dictionary<string, ComboBox>();
        private readonly Dictionary<string, TextBox> _outputTextboxs = new Dictionary<string, TextBox>();

        private List<string> OutputNames { get; set; }
        private Dictionary<string, Pipe> Inputs { get; set; }
        private RackCableManager RackCableManager { get; }

        private readonly MouseEventHandler _dragItemEventHandler;
        private readonly RackArrayControl.SelectRackItemDelegate _selectRackItemDelegate;

        /// <summary>
        /// Gets or sets additional content for the UserControl
        /// </summary>
        public object AdditionalContent
        {
            get { return GetValue(AdditionalContentProperty); }
            set { SetValue(AdditionalContentProperty, value); }
        }

        public static readonly DependencyProperty AdditionalContentProperty = DependencyProperty.Register("AdditionalContent", typeof(object), typeof(RackItemContainer), new PropertyMetadata(null));

        public RackItemContainer(RackCableManager rackCableManager, RackArrayControl rackArray, IRackItem rackItem, MouseEventHandler mouseMovePreviewEventHandler, RackArrayControl.SelectRackItemDelegate mouseClickEventHandler)
        {
            InitializeComponent();

            _id = _rackId++;

            var desc = DependencyPropertyDescriptor.FromProperty(AdditionalContentProperty, typeof(UserControl));
            desc.AddValueChanged(this, ContentPropertyChanged);

            OutputNames = new List<string>();
            Inputs = new Dictionary<string, Pipe>();

            RackCableManager = rackCableManager;
            RackCableManager.AddRack(this);

            ContainingPanel = rackArray.GetRackStackPanel();
            RackItem = rackItem;
            ContainingPanel.Children.Add(this);
            AdditionalContent = rackItem;

            _dragItemEventHandler = mouseMovePreviewEventHandler;
            InputsPanel.PreviewMouseMove += RackItemMouseMoveForDragging;
            OutputsPanel.PreviewMouseMove += RackItemMouseMoveForDragging;
            InputsGrid.PreviewMouseMove += RackItemMouseMoveForDragging;
            OutputsGrid.PreviewMouseMove += RackItemMouseMoveForDragging;

            _selectRackItemDelegate = mouseClickEventHandler;
            BackgroundGrid.MouseDown += BackgroundGridOnMouseDown;
        }

        private void BackgroundGridOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            _selectRackItemDelegate.Invoke(RackItem);
        }

        private void RackItemMouseMoveForDragging(object sender, MouseEventArgs e)
        {
            var alteredMouseArgs = new MouseEventArgs(e.MouseDevice, e.Timestamp)
            {
                RoutedEvent = PreviewMouseMoveEvent,
                Source = this,
            };
            _dragItemEventHandler.Invoke(ContainingPanel, alteredMouseArgs);
        }

        private void ContentPropertyChanged(object sender, EventArgs e)
        {
            var rack = sender as RackItemContainer;
            if (sender == null) return;

            var rackPanel = rack?.AdditionalContent as IRackItem;
            if (rackPanel == null) return;

            var userControl = rack.AdditionalContent as UserControl;
            if (userControl == null) return;

            ContentRow.Height = new GridLength(userControl.Height);

            Inputs = rackPanel.GetInputs();
            OutputNames = rackPanel.GetOutputs();

            _inputSelectors.Clear();
            foreach (var input in Inputs)
            {
                var inputSelector = new ComboBox();
                inputSelector.SelectionChanged += RackCableManager.InputSelectorSelectionChanged;
                inputSelector.Tag = input.Key + _id.ToString();
                inputSelector.Margin = new Thickness(1, 1, 5, 1);
                inputSelector.Width = 100;
                inputSelector.FontSize = 10;
                inputSelector.HorizontalContentAlignment = HorizontalAlignment.Center;
                inputSelector.VerticalContentAlignment = VerticalAlignment.Center;
                InputsPanel.Children.Add(inputSelector);
                _inputSelectors.Add(input.Key, inputSelector);
            }

            _outputTextboxs.Clear();
            foreach (var outputName in OutputNames)
            {
                var outputTextBox = new TextBox
                {
                    Text = outputName,
                    FontSize = 10,
                    Width = 100,
                    Margin = new Thickness(1, 1, 5, 1),
                    Background = Brushes.Transparent,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                outputTextBox.TextChanged += RackCableManager.OutputTextBoxTextChanged;
                OutputsPanel.Children.Add(outputTextBox);
                _outputTextboxs.Add(outputName, outputTextBox);
            }

            CloseButton.Visibility = rackPanel.CanDelete() ? Visibility.Visible : Visibility.Hidden;

            rackPanel.SetRack(this);
            UpdateInputSelectors();
            RackCableManager.RackContentSet(this);
        }

        public void UpdateInputSelectors(List<string> outputs)
        {
            foreach (var inputSelector in _inputSelectors)
            {
                var oldValue = inputSelector.Value.SelectedItem as string;
                inputSelector.Value.Items.Clear();
                foreach (var outputName in outputs)
                {
                    inputSelector.Value.Items.Add(outputName);
                }

                if (oldValue != null && outputs.Contains(oldValue)) 
                {
                    inputSelector.Value.SelectedItem = oldValue;
                }
            }
        }

        private void UpdateInputSelectors()
        {
            var outputNameList = RackCableManager.GetOutputNameList();
            foreach (var inputSelector in _inputSelectors)
            {
                inputSelector.Value.Items.Clear();
                foreach (var outputName in outputNameList)
                {
                    inputSelector.Value.Items.Add(outputName);
                }
            }
        }

        public Dictionary<string, Pipe> GetInputs()
        {
            return Inputs.ToDictionary(kv => kv.Key + _id.ToString(), kv => kv.Value);
        }

        public IEnumerable<string> GetExternalOutputNames()
        {
            return _outputTextboxs.Select(textBox => textBox.Value.Text).ToList();
        }

        public IDictionary<string, string> GetInternalToExternalOutputNames()
        {
            return _outputTextboxs.ToDictionary(output => output.Key, output => output.Value.Text);
        }

        public IDictionary<string, string> GetInputToConnectedOutputNames()
        {
            return _inputSelectors.ToDictionary(input => input.Key, input => input.Value.SelectedValue?.ToString());
        }

        public void OutputPipe(string outputName, List<byte> data, int iteration)
        {
            RackCableManager.OutputPipe(_outputTextboxs[outputName].Text, data, iteration);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            RackCableManager.RemoveRack(this);
            ContainingPanel.Children.Remove(this);

            RackItem?.CleanUp();
        }

        public void RenameOutputs(IDictionary<string, string> renameMap)
        {
            foreach (var rename in renameMap)
            {
                if (_outputTextboxs.ContainsKey(rename.Key))
                {
                    _outputTextboxs[rename.Key].Text = rename.Value;
                }
            }
        }

        public void SetInputs(IDictionary<string, string> setMap)
        {
            foreach (var set in setMap)
            {
                //if (_inputSelectors.ContainsKey(set.Key))
                //{
                    _inputSelectors[set.Key].SelectedValue = set.Value;
                //}
            }
        }
    }
}

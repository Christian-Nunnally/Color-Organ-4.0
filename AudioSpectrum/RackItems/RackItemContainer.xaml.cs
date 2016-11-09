﻿using System;
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
        public StackPanel ContainingPanel;
        public IRackItem RackItem { get; }

        private RackCableManager RackCableManager { get; }

        private readonly MouseEventHandler _dragItemEventHandler;
        private readonly RackArrayControl.SelectRackItemDelegate _selectRackItemDelegate;
        private readonly List<ComboBox> _inputComboBoxs = new List<ComboBox>();

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

            var desc = DependencyPropertyDescriptor.FromProperty(AdditionalContentProperty, typeof(UserControl));
            desc.AddValueChanged(this, ContentPropertyChanged);

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

            var rackItem = rack?.AdditionalContent as IRackItem;
            if (rackItem == null) return;

            var userControl = rack.AdditionalContent as UserControl;
            if (userControl == null) return;

            ItemNameLabel.Content = rackItem.ItemName;
            ContentRow.Height = new GridLength(double.IsNaN(userControl.Height) ? 0.0 : userControl.Height);

            foreach (var input in rackItem.GetInputs())
            {
                var inputSelector = new ComboBox();
                inputSelector.SelectionChanged += RackCableManager.InputSelectorSelectionChanged;
                inputSelector.Tag = input;
                inputSelector.Margin = new Thickness(1, 0, 5, 1);
                inputSelector.Width = 100;
                inputSelector.FontSize = 10;
                inputSelector.HorizontalContentAlignment = HorizontalAlignment.Center;
                inputSelector.VerticalContentAlignment = VerticalAlignment.Center;
                inputSelector.ItemsSource = RackCableManager.AllRackItemOutputs;
                _inputComboBoxs.Add(inputSelector);

                InputsPanel.Children.Add(inputSelector);
            }

            foreach (var output in rackItem.GetOutputs())
            {
                var outputTextBox = new TextBox
                {
                    Text = output.VisibleName,
                    FontSize = 10,
                    Width = 100,
                    Margin = new Thickness(1, 1, 5, 1),
                    Background = Brushes.Transparent,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Tag = output
                };
                outputTextBox.TextChanged += RackCableManager.OutputTextBoxTextChanged;
                OutputsPanel.Children.Add(outputTextBox);
            }

            CloseButton.Visibility = rackItem.CanDelete() ? Visibility.Visible : Visibility.Hidden;

            rackItem.SetRack(this);
            RackCableManager.RackContentSet(this);
        }

        public void InputSelectorItemsChanged()
        {
            foreach (var inputCb in _inputComboBoxs)
            {
                var input = (RackItemInput)inputCb.Tag;
                if (input.ConnectedOutput == null) return;

                int i = 0;
                foreach (var output in inputCb.Items.OfType<RackItemOutput>())
                {
                    if (output.VisibleName.Equals(input.ConnectedOutput))
                    {
                        inputCb.SelectedIndex = i;
                    }
                    i++;
                }
            }
        }

        public IEnumerable<RackItemInput> GetInputs()
        {
            return RackItem.GetInputs();
        }

        public List<RackItemOutput> GetOutputs()
        {
            return RackItem.GetOutputs();
        }

        public void OutputPipe(RackItemOutput output, List<byte> data, int iteration)
        {
            RackCableManager.OutputPipe(output, data, iteration);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            RackCableManager.RemoveRack(this);
            ContainingPanel.Children.Remove(this);

            RackItem?.CleanUp();
        }
    }
}

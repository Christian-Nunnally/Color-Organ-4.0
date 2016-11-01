using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    public partial class GraphicEditorItem : RackItemBase
    {
        private readonly ObservableCollection<StaticLedGraphic> _graphics = new ObservableCollection<StaticLedGraphic>();
        private int _selectedIndex;
        private bool _editMode;

        public GraphicEditorItem()
        {
            InitializeComponent();
            ItemName = "Graphic Editor";
            ChannelsListBox.ItemsSource = _graphics;
            LedSimulator.PixelClicked += PixelClicked;
        }

        public override IRackItem CreateRackItem()
        {
            return new GraphicEditorItem();
        }

        public override Dictionary<string, Pipe> GetInputs()
        {
            var inputs = new Dictionary<string, Pipe> {{"Switch Input", SwitchInput}};
            return inputs;
        }

        public override List<string> GetOutputs()
        {
            var outputs = new List<string> {"Image Out"};
            return outputs;
        }

        private void PixelClicked(int pixelNumber, MouseButtonEventArgs e)
        {
            if (ChannelsListBox.SelectedIndex < 0) return;
            if (ChannelsListBox.SelectedIndex >= _graphics.Count) return;

            if (e.ChangedButton == MouseButton.Left)
            {
                _graphics[ChannelsListBox.SelectedIndex].SetPixel(pixelNumber, ColorPicker.SelectedColor ?? Colors.Black);
            }
            else
            {
                _graphics[ChannelsListBox.SelectedIndex].SetPixel(pixelNumber, Colors.Black);
            }

            if (!_editMode) return;

            if (_selectedIndex < 0) return;
            if (_selectedIndex >= _graphics.Count) return;

            LedSimulator.Set(_graphics[_selectedIndex].Graphic);
        }

        private void AddChannelButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChannelNameTextBox.Text == string.Empty) return;
            if (_graphics.Any(graphic => graphic.Name == ChannelNameTextBox.Text))
            {
                return;
            }

            _graphics.Add(new StaticLedGraphic(ChannelNameTextBox.Text));
        }

        private void SwitchInput(List<byte> binaryData, int iteration)
        {
            if (_editMode)
            {
                if (ChannelsListBox.SelectedIndex < 0) return;
                if (ChannelsListBox.SelectedIndex >= _graphics.Count) return;

                RackContainer.OutputPipe("Image Out", _graphics[ChannelsListBox.SelectedIndex].Graphic.ToList(), iteration);
                return;
            }

            var compositeGraphic = new byte[192];
            for (var i = 0; i < Math.Min(_graphics.Count, binaryData.Count); i++)
            {
                if (binaryData[i] != 1) continue;
                for (var p = 0; p < 64; p++)
                {
                    var r = _graphics[i].Graphic[p];
                    var g = _graphics[i].Graphic[p + 64];
                    var b = _graphics[i].Graphic[p + 128];
                    if (r == 0 && g == 0 && b == 0)
                    {
                        compositeGraphic[p] = r;
                        compositeGraphic[p + 64] = g;
                        compositeGraphic[p + 128] = g;
                    }
                    else
                    {
                        compositeGraphic[p] = (byte)((r + compositeGraphic[p]) / 2);
                        compositeGraphic[p + 64] = (byte)((g + compositeGraphic[p + 64]) / 2);
                        compositeGraphic[p + 128] = (byte)((b + compositeGraphic[p + 128]) / 2);
                    }
                }
            }

            LedSimulator.Set(compositeGraphic);
            RackContainer.OutputPipe("Image Out", compositeGraphic.ToList(), iteration);
        }

        private void ModeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _editMode = !_editMode;
            ModeToggleButton.Content = _editMode ? "Display Mode" : "Edit Mode";
        }

        private void ChannelsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null) _selectedIndex = listBox.SelectedIndex;
        }

        public override void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement("GraphicEditorItem"));
        }

        public override void Load(XmlElement xml)
        {
            throw new System.NotImplementedException();
        }
    }
}

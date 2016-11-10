using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using AudioSpectrum.SideRailContainers;
using Xceed.Wpf.Toolkit;

namespace AudioSpectrum.RackItems
{
    public partial class StaticGraphicEditorItem : RackItemBase
    {
        private readonly Button _addChannelButton = new Button();
        private readonly TextBox _channelNameTextBox = new TextBox();
        private readonly ListBox _channelsListBox = new ListBox();
        private readonly ColorPicker _colorPicker = new ColorPicker();

        private readonly ObservableCollection<StaticLedGraphic> _graphics = new ObservableCollection<StaticLedGraphic>();

        private readonly LedDisplay _ledDisplay = new LedDisplay();
        private int _selectedIndex;
        private List<UIElement> _sideRailControls;

        public StaticGraphicEditorItem(XmlNode xml)
        {
            InitializeComponent();
            ItemName = "GraphicEditor";
            _ledDisplay.PixelClicked += PixelClicked;
            _channelsListBox.ItemsSource = _graphics;
            _channelsListBox.DisplayMemberPath = "Name";
            _channelsListBox.SelectionChanged += ChannelsListBox_OnSelectionChanged;
            _addChannelButton.Content = "Add New Graphic";
            _addChannelButton.Width = 255;
            _addChannelButton.HorizontalContentAlignment = HorizontalAlignment.Center;
            _addChannelButton.Click += AddChannelButton_Click;
            _channelNameTextBox.Text = "Graphic 1";

            if (xml == null)
            {
                AddInput(new RackItemInput("Switch Input", SwitchInput));
                AddOutput(new RackItemOutput("Image Out"));

                AddChannel("Graphic 1");
            }
            else
            {
                Load(xml);
            }
        }

        public override IRackItem CreateRackItem(XmlElement xml)
        {
            return new StaticGraphicEditorItem(xml);
        }

        private void PixelClicked(int pixelNumber, EventArgs e)
        {
            if (_channelsListBox.SelectedIndex < 0) return;
            if (_channelsListBox.SelectedIndex >= _graphics.Count) return;

            if (_graphics[_channelsListBox.SelectedIndex].IsPixelOff(pixelNumber))
                _graphics[_channelsListBox.SelectedIndex].SetPixel(pixelNumber, _colorPicker.SelectedColor ?? Colors.Black);
            else
                _graphics[_channelsListBox.SelectedIndex].SetPixel(pixelNumber, Colors.Black);

            if (_selectedIndex < 0) return;
            if (_selectedIndex >= _graphics.Count) return;

            _ledDisplay.Set(_graphics[_selectedIndex].Graphic);
        }

        private void AddChannelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_channelNameTextBox.Text == string.Empty) return;
            AddChannel(_channelNameTextBox.Text);
        }

        public override void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            if (_sideRailControls == null)
            {
                _sideRailControls = new List<UIElement>();
                _sideRailControls.Add(new LabeledControlSideRailContainer("New graphic name", _channelNameTextBox, Orientation.Horizontal, 150));
                _sideRailControls.Add(new LabeledControlSideRailContainer(string.Empty, _addChannelButton, Orientation.Horizontal, 0));
                _sideRailControls.Add(SideRailContentGenerator.GenerateSeperator(1));
                _sideRailControls.Add(new LabeledControlSideRailContainer("Graphics list", _channelsListBox, Orientation.Vertical, 300));
                _sideRailControls.Add(SideRailContentGenerator.GenerateSeperator(1));
                _sideRailControls.Add(new LabeledControlSideRailContainer("Color", _colorPicker, Orientation.Horizontal, 200));
                _sideRailControls.Add(new LabeledControlSideRailContainer("Graphic editor", _ledDisplay, Orientation.Vertical, 152));
            }
            sideRailSetter.Invoke(ItemName, _sideRailControls);
        }

        private void AddChannel(XmlNode xml)
        {
            AddChannel(string.Empty, xml);
        }

        private void AddChannel(string channelName, XmlNode xml = null)
        {
            if (_graphics.Any(graphic => graphic.Name == _channelNameTextBox.Text))
                return;

            var staticGraphic = (channelName == string.Empty) && (xml != null) ? new StaticLedGraphic(xml) : new StaticLedGraphic(channelName);
            _graphics.Add(staticGraphic);
            if (_channelsListBox.Items.Count > 0) _channelsListBox.SelectedIndex = _channelsListBox.Items.Count - 1;
            _channelNameTextBox.Text = "Graphic " + (_graphics.Count + 1);
        }

        private void SwitchInput(List<byte> binaryData)
        {
            var compositeGraphic = new byte[192];
            for (var i = 0; i < Math.Min(_graphics.Count, binaryData.Count); i++)
            {
                if (binaryData[i] != 1) continue;

                for (var p = 0; p < 64; p++)
                {
                    var r = _graphics[i].Graphic[p];
                    var g = _graphics[i].Graphic[p + 64];
                    var b = _graphics[i].Graphic[p + 128];
                    if ((compositeGraphic[p] == 0) && (compositeGraphic[p + 64] == 0) && (compositeGraphic[p + 128] == 0))
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

            if (RackItemOutputs.Count > 0)
                RackContainer.OutputPipe(RackItemOutputs.First(), compositeGraphic.ToList());
        }

        private void ChannelsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null) _selectedIndex = listBox.SelectedIndex;

            if (_channelsListBox.SelectedIndex < 0) return;
            if (_channelsListBox.SelectedIndex >= _graphics.Count) return;

            _ledDisplay.Set(_graphics[_selectedIndex].Graphic);
        }

        public override void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement(RackItemName + "-" + ItemName));
            foreach (var staticLedGraphic in _graphics)
                staticLedGraphic.Save(xml, node);
            SaveOutputs(xml, node);
            SaveInputs(xml, node);
        }

        public sealed override void Load(XmlNode xml)
        {
            LoadInputsAndOutputs(xml);
            AttachPipeToInput(1, SwitchInput);

            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
                switch (node.Name)
                {
                    case "StaticGraphic":
                        AddChannel(node);
                        break;
                }
        }
    }
}
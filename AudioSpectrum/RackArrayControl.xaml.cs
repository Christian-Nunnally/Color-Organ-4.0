using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using AudioSpectrum.RackItems;

namespace AudioSpectrum
{
    public partial class RackArrayControl : UserControl, ISaveable
    {
        private static readonly Dictionary<string, RackItemFactory> RegisteredRackItems = new Dictionary<string, RackItemFactory>();
        private static readonly List<RackArrayControl> AllExistingRackArrays = new List<RackArrayControl>();

        public delegate void SelectRackItemDelegate(IRackItem selectedRackItem);

        private readonly RackCableManager _rackCableManager = new RackCableManager();
        private readonly List<StackPanel> _rackStackPanels = new List<StackPanel>();
        private readonly List<Button> _rackItemFactoryButtons = new List<Button>();
        private const double RackWidth = 400;

        private bool _isDown;
        private bool _isDragging;
        private Point _startPoint;
        private UIElement _realDragSource;
        private readonly UIElement _dummyDragSource = new UIElement();
        private IRackItem _selectedRackItem;

        public RackArrayControl()
        {
            InitializeComponent();
            AddRack();
            PopulateTopRailWithRegisteredRackItems();
            AllExistingRackArrays.Add(this);
        }

        public void AddRack(XmlNode xml = null)
        {
            var rackPanel = new StackPanel {Width = RackWidth};
            rackPanel.PreviewMouseLeftButtonUp += StackPanelPreviewMouseLeftButtonUp;
            rackPanel.PreviewMouseLeftButtonDown += StackPanelPreviewMouseLeftButtonDown;
            rackPanel.DragEnter += StackPanelDragEnter;
            rackPanel.Drop += StackPanelDrop;
            rackPanel.AllowDrop = true;
            rackPanel.Background = Brushes.White;
            _rackStackPanels.Add(rackPanel);
            RackPanel.Items.Add(rackPanel);

            if (xml == null) return;

            for (var i = 0; i < xml.ChildNodes.Count; i++)
            {
                var node = xml.ChildNodes.Item(i);
                if (node == null) continue;
                if (node.Name == "RackItem")
                {
                    
                }
            }
        }

        public StackPanel GetRackStackPanel()
        { 
            if (RackPanel.Items.Count < 1) AddRack();
            var stackPanel = RackPanel.Items[0] as StackPanel;
            if (stackPanel == null) throw new InvalidCastException("The rack panel should only contain stack panels");
            return stackPanel;
        }

#region StackPanel Drag Drop

        private void StackPanelPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var stackPanel = sender as StackPanel;
            if (Equals(e.Source, stackPanel))
            {
            }
            else
            {
                _isDown = true;
                _startPoint = e.GetPosition(stackPanel);
            }
        }

        private void StackPanelPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDown = false;
            _isDragging = false;
            _realDragSource?.ReleaseMouseCapture();
        }

        private void StackPanelPreviewMouseMove(object sender, MouseEventArgs e)
        {
            var stackPanel = sender as StackPanel;
            if (!_isDown) return;

            if (_isDragging || (!(Math.Abs(e.GetPosition(stackPanel).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) &&
                !(Math.Abs(e.GetPosition(stackPanel).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance))) return;

            _isDragging = true;
            _realDragSource = e.Source as UIElement;
            _realDragSource?.CaptureMouse();
            DragDrop.DoDragDrop(_dummyDragSource, new DataObject("UIElement", e.Source, true), DragDropEffects.Move);
        }

        private static void StackPanelDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("UIElement"))
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void StackPanelDrop(object sender, DragEventArgs e)
        {
            var stackPanel = (StackPanel)sender;
            if (!e.Data.GetDataPresent("UIElement")) return;
            var droptarget = e.Source as UIElement;
            int droptargetIndex = -1, i = 0;
            foreach (UIElement element in stackPanel.Children)
            {
                if (element.Equals(droptarget))
                {
                    droptargetIndex = i;
                    break;
                }
                i++;
            }
            foreach (var rackStackPanel in _rackStackPanels)
            {
                rackStackPanel.Children.Remove(_realDragSource);
            }

            stackPanel.Children.Insert(droptargetIndex != -1 ? droptargetIndex : stackPanel.Children.Count,
                _realDragSource);

            _isDown = false;
            _isDragging = false;
            _realDragSource.ReleaseMouseCapture();
        }

#endregion

        public static void RegisterRackItem(string name, RackItemFactory rackFactory)
        {
            RegisteredRackItems.Add(name, rackFactory);

            foreach (var rackArray in AllExistingRackArrays)
            {
                rackArray.PopulateTopRailWithRegisteredRackItems();
            }
        }

        private void PopulateTopRailWithRegisteredRackItems()
        {
            _rackItemFactoryButtons.Clear();
            TopRail.Children.Clear();
            var i = 0;
            foreach (var rackItem in RegisteredRackItems)
            {
                var grayness = (byte) (220 - i%2*10);
                var factoryButton = new Button
                {
                    Content = new TextBlock
                    {
                        Text = rackItem.Key,
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center
                    },
                    Width = TopRail.Height,
                    MaxWidth = 48,
                    MinWidth = 48,
                    FontSize = 10,
                    Margin = new Thickness(1, 1, 1, 1),
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Background = new SolidColorBrush(Color.FromRgb(grayness, grayness, grayness))
                };

                factoryButton.Click += (sender, args) =>
                {
                    var rackContainer = new RackItemContainer(_rackCableManager, this, rackItem.Value(),
                        StackPanelPreviewMouseMove, SelectRackItem);
                };

                _rackItemFactoryButtons.Add(factoryButton);
                TopRail.Children.Add(factoryButton);
                i++;
            }
        }

        private void SetSideRail(string title, IEnumerable<Control> controls)
        {
            SelectedItemNameLabel.Content = title;

            SideRail.Children.Clear();
            foreach (var control in controls)
            {
                SideRail.Children.Add(control);
            }
        }

        private void SelectRackItem(IRackItem selectedRackItem)
        {
            selectedRackItem.SetSideRail(SetSideRail);
            _selectedRackItem = selectedRackItem;
        }

        public void Save(XmlDocument xml, XmlNode parent)
        {
            foreach (var rackStackPanel in _rackStackPanels)
            {
                var stackPanelElement = parent.AppendChild(xml.CreateElement("StackPanel"));
                foreach (var rackItemContainer in rackStackPanel.Children.OfType<RackItemContainer>())
                {
                    rackItemContainer.RackItem.Save(xml, stackPanelElement);
                }
            }
        }

        public void Load(XmlElement xml)
        {

        }
    }
}

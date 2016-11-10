using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using AudioSpectrum.RackItems;

namespace AudioSpectrum
{
    public partial class RackArrayControl : UserControl, ISaveable
    {

        public delegate void SelectRackItemDelegate(IRackItem selectedRackItem);

        private const double RackWidth = 400;
        private static readonly Dictionary<string, RackItemFactory> RegisteredRackItems = new Dictionary<string, RackItemFactory>();
        private static readonly List<RackArrayControl> AllExistingRackArrays = new List<RackArrayControl>();
        private readonly UIElement _dummyDragSource = new UIElement();

        private readonly RackCableManager _rackCableManager = new RackCableManager();
        private readonly List<StackPanel> _rackStackPanels = new List<StackPanel>();

        private bool _isDown;
        private bool _isDragging;

        private StackPanel _mostRecentlyAddedStackPanelForLoading;
        private UIElement _realDragSource;
        private Point _startPoint;

        public RackArrayControl()
        {
            InitializeComponent();
            PopulateTopRailWithRegisteredRackItems();
            _rackCableManager.MoreOutputsLoadedDelegate += InputSelectorItemsChanged;
            AllExistingRackArrays.Add(this);
        }

        public void Save(XmlDocument xml, XmlNode parent)
        {
            foreach (var rackStackPanel in _rackStackPanels)
            {
                var stackPanelElement = parent.AppendChild(xml.CreateElement("StackPanel"));
                foreach (var rackItemContainer in rackStackPanel.Children.OfType<RackItemContainer>())
                    rackItemContainer.RackItem.Save(xml, stackPanelElement);
            }
        }

        public void Load(XmlNode xml)
        {
            foreach (var node in xml.ChildNodes.OfType<XmlElement>())
            {
                var splitName = node?.Name.Split('-');
                if (!(splitName?.Length > 1)) continue;
                if (splitName[0] == "RackItem")
                    CreateRackItem(splitName[1], node);
            }
        }

        public void AddRack(XmlNode xml = null)
        {
            var rackPanel = new StackPanel { Width = RackWidth };
            rackPanel.PreviewMouseLeftButtonUp += StackPanelPreviewMouseLeftButtonUp;
            rackPanel.PreviewMouseLeftButtonDown += StackPanelPreviewMouseLeftButtonDown;
            rackPanel.DragEnter += StackPanelDragEnter;
            rackPanel.Drop += StackPanelDrop;
            rackPanel.AllowDrop = true;
            rackPanel.Background = Brushes.White;
            _mostRecentlyAddedStackPanelForLoading = rackPanel;
            _rackStackPanels.Add(rackPanel);
            RackPanel.Items.Add(rackPanel);

            if (xml == null) return;
            Load(xml);
        }

        public StackPanel GetRackStackPanel()
        {
            if (_mostRecentlyAddedStackPanelForLoading == null) AddRack();
            return _mostRecentlyAddedStackPanelForLoading;
        }

        public static void RegisterRackItem(string name, RackItemFactory rackFactory)
        {
            RegisteredRackItems.Add(name, rackFactory);

            foreach (var rackArray in AllExistingRackArrays)
                rackArray.PopulateTopRailWithRegisteredRackItems();
        }

        private void InputSelectorItemsChanged()
        {
            foreach (var stackPanel in _rackStackPanels)
                foreach (var rackItemContainer in stackPanel.Children.OfType<RackItemContainer>())
                    rackItemContainer.InputSelectorItemsChanged();
        }

        private void PopulateTopRailWithRegisteredRackItems()
        {
            TopRail.Children.Clear();
            var i = 0;
            foreach (var rackItem in RegisteredRackItems)
            {
                var grayness = (byte)(220 - i % 2 * 10);
                var factoryButton = new Button
                {
                    Content = new TextBlock
                    {
                        Text = SplitCamelCase(rackItem.Key),
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center
                    },
                    Width = TopRail.Height,
                    MaxWidth = 75,
                    MinWidth = 75,
                    FontSize = 12,
                    Margin = new Thickness(1, 1, 1, 1),
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Background = new SolidColorBrush(Color.FromRgb(grayness, grayness, grayness))
                };

                factoryButton.Click += (sender, args) => { CreateRackItem(rackItem.Key); };
                TopRail.Children.Add(factoryButton);
                i++;
            }
        }

        private static string SplitCamelCase(string input)
        {
            return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
        }

        private void CreateRackItem(string rackItemName, XmlElement xml = null)
        {
            if (!RegisteredRackItems.ContainsKey(rackItemName)) throw new UnregisteredRackItemException();
            new RackItemContainer(_rackCableManager, this, RegisteredRackItems[rackItemName](xml),
                StackPanelPreviewMouseMove, SelectRackItem);
        }

        private void SetSideRail(string title, IEnumerable<UIElement> controls)
        {
            SelectedItemNameLabel.Content = title;

            SideRail.Children.Clear();

            var propertiesLabel = new TextBlock
            {
                Text = "Properties",
                FontSize = 10,
                Height = 20,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 2, 0, 0)
            };
            SideRail.Children.Add(propertiesLabel);
            SideRail.Children.Add(GetSideRailSeperator());

            foreach (var control in controls)
                SideRail.Children.Add(control);

            SideRail.Children.Add(GetSideRailSeperator());
        }

        private static Rectangle GetSideRailSeperator()
        {
            var seperator = new Rectangle
            {
                Height = 1,
                Fill = Brushes.Black
            };
            return seperator;
        }

        private void SelectRackItem(IRackItem selectedRackItem)
        {
            selectedRackItem.SetSideRail(SetSideRail);
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
                e.Effects = DragDropEffects.Move;
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
                rackStackPanel.Children.Remove(_realDragSource);

            var rackItemContainer = _realDragSource as RackItemContainer;
            if (rackItemContainer != null) rackItemContainer.ContainingPanel = stackPanel;

            stackPanel.Children.Insert(droptargetIndex != -1 ? droptargetIndex : stackPanel.Children.Count,
                _realDragSource);

            _isDown = false;
            _isDragging = false;
            _realDragSource.ReleaseMouseCapture();
        }

        #endregion
    }

    internal class UnregisteredRackItemException : Exception
    {
    }
}
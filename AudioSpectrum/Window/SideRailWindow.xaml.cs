using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AudioSpectrum.SideRailContainers;

namespace AudioSpectrum
{
    /// <summary>
    ///     Interaction logic for SideRailWindow.xaml
    /// </summary>
    public partial class SideRailWindow : UserControl
    {
        private ProjectManager _projectManager;

        public SideRailWindow()
        {
            InitializeComponent();
            SelectionManager.SelectionMgr.SetSideRail = SetSideRail;
        }

        public void AttachProjectManager(ProjectManager projectManager)
        {
            _projectManager = projectManager;
            
        }

        private void SetSideRail(string title, IEnumerable<UIElement> controls)
        {
            SelectedItemButton.Content = title;

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
            SideRail.Children.Add(SideRailContentGenerator.GenerateSeperator(1));

            foreach (var control in controls)
                SideRail.Children.Add(control);

            SideRail.Children.Add(SideRailContentGenerator.GenerateSeperator(1));
        }
    }
}
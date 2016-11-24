using System;
using System.Windows;
using System.Windows.Controls;
using AudioSpectrum.Project;

namespace AudioSpectrum.Window
{
    /// <summary>
    ///     Interaction logic for LeftSideWindow.xaml
    /// </summary>
    public partial class LeftSideWindow : UserControl
    {
        private ProjectManager _projectManager;

        public LeftSideWindow()
        {
            InitializeComponent();
        }

        public void AttachProjectManager(ProjectManager projectManager)
        {
            _projectManager = projectManager;
            _projectManager.CurrentProjectChanged += ProjectChangedEventHandler;
            FileWindow.Visibility = Visibility.Collapsed;
            FileWindow.AttachProjectManager(projectManager);
            RackSetupsWindow.AttachProjectManager(projectManager);
        }

        private void ProjectNameLabel_Click(object sender, RoutedEventArgs e)
        {
            FileWindow.Visibility = FileWindow.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ProjectChangedEventHandler(object sender, EventArgs e)
        {
            ProjectNameLabel.Content = _projectManager.CurrentProject?.ProjectName ?? "";
        }
    }
}
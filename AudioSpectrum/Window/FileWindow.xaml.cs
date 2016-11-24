using System.Windows;
using System.Windows.Controls;
using AudioSpectrum.Project;

namespace AudioSpectrum.Window
{
    /// <summary>
    ///     Interaction logic for FileWindow.xaml
    /// </summary>
    public partial class FileWindow : UserControl
    {
        private ProjectManager _projectManager;

        public FileWindow()
        {
            InitializeComponent();
        }

        public void AttachProjectManager(ProjectManager projectManager)
        {
            _projectManager = projectManager;
        }

        private void NewProjectButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            _projectManager?.OpenNewProject();
        }

        private void OpenProjectButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            _projectManager?.LoadProject();
        }

        private void CloseProjectButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            _projectManager?.CloseProject();
        }
    }
}
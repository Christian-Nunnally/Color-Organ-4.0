using System;
using System.Windows;
using System.Windows.Controls;
using AudioSpectrum.Project;

namespace AudioSpectrum.Window
{
    /// <summary>
    ///     Interaction logic for RackSetupsWindow.xaml
    /// </summary>
    public partial class RackSetupsWindow : UserControl
    {
        private ProjectManager _projectManager;

        public RackSetupsWindow()
        {
            InitializeComponent();
            RackSetupListBox.DisplayMemberPath = "Name";
            ControlRow.Height = new GridLength(0);
        }

        public void AttachProjectManager(ProjectManager projectManager)
        {
            _projectManager = projectManager;
            _projectManager.CurrentProjectChanged += CurrentProjectChanged;
        }

        private void CurrentProjectChanged(object sender, EventArgs eventArgs)
        {
            if (_projectManager.CurrentProject == null)
            {
                RackSetupListBox.ItemsSource = null;
                ControlRow.Height = new GridLength(0);
                return;
            }
            RackSetupListBox.ItemsSource = _projectManager.CurrentProject.RackSetups;
            ControlRow.Height = new GridLength(33);
        }

        private void RackSetupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RackSetupListBox.SelectedItems.Count != 1) return;
            var selectedItem = RackSetupListBox.SelectedItems[0] as RackSetup;
            if (selectedItem == null) return;
            if (_projectManager == null) return;
            _projectManager.CurrentProject.SelectedRackSetup = selectedItem;
            _projectManager.RackSetupChanged();
        }

        private void NewSetupNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AddSetupButton == null) return;

            AddSetupButton.IsEnabled = NewSetupNameTextBox.Text != string.Empty;
        }

        private void AddSetupButton_Click(object sender, RoutedEventArgs e)
        {
            if (_projectManager == null) return;
            _projectManager?.CurrentProject?.AddSetup(NewSetupNameTextBox.Text);
        }
    }
}
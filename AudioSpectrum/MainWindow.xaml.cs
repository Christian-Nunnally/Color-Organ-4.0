using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using AudioSpectrum.RackItems;

namespace AudioSpectrum
{
    public partial class MainWindow : Window
    {
        private readonly ProjectManager _projectManager;

        public MainWindow()
        {
            InitializeComponent();
            RegisterRackItems();

            _projectManager = new ProjectManager(this);
            _projectManager.CurrentProjectChanged += ProjectChangedEventHandler;
            WindowState = WindowState.Maximized;
            SetupRow.Height = new GridLength(0);

            Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            _projectManager.SaveCurrentProject();
        }

        private static void RegisterRackItems()
        {
            RackArrayControl.RegisterRackItem(new AudioSourceItem().ItemName, new AudioSourceItem().CreateRackItem);
            RackArrayControl.RegisterRackItem(new SpectrumItem().ItemName, new SpectrumItem().CreateRackItem);
            RackArrayControl.RegisterRackItem(new SpectrumLedItem().ItemName, new SpectrumLedItem().CreateRackItem);
            RackArrayControl.RegisterRackItem(new SerialInterfaceItem().ItemName, new SerialInterfaceItem().CreateRackItem);
            RackArrayControl.RegisterRackItem(new FilterSpectrumItem().ItemName, new FilterSpectrumItem().CreateRackItem);
            RackArrayControl.RegisterRackItem(new GraphicEditorItem().ItemName, new GraphicEditorItem().CreateRackItem);
            RackArrayControl.RegisterRackItem(new SpectrumToBinaryDataItem().ItemName, new SpectrumToBinaryDataItem().CreateRackItem);
            RackArrayControl.RegisterRackItem(new AudioProcessorItem().ItemName, new AudioProcessorItem().CreateRackItem);
        }

        private void NewProject()
        {
            _projectManager?.OpenNewProject();
        }

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            NewProject();
        }

        private void ProjectChangedEventHandler(object sender, EventArgs e)
        {
            RackSetupChanged();
            ProjectNameLabel.Content = _projectManager.CurrentProject.ProjectName;
            RackSetupListBox.ItemsSource = _projectManager.CurrentProject.RackSetups;
            RackSetupListBox.DisplayMemberPath = "Name";

            if (_projectManager.CurrentProject != null)
            {
                SetupRow.Height = new GridLength(33);
            }
            else
            {
                SetupRow.Height = new GridLength(0);

            }
        }

        private void RackSetupListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (RackSetupListBox.SelectedItems.Count != 1) return;
            var selectedItem = RackSetupListBox.SelectedItems[0] as RackSetup;
            if (selectedItem == null) return;
            _projectManager.CurrentProject.SelectedRackSetup = selectedItem;
            RackSetupChanged();
        }

        private void RackSetupChanged()
        {
            RackArrayContentControl.Content = _projectManager.CurrentProject.RackArrayControl;
        }

        private void AddRackCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _projectManager?.CurrentProject?.RackArrayControl?.AddRack();
        }

        private void NewSetupNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (AddSetupButton == null) return;

            AddSetupButton.IsEnabled = NewSetupNameTextBox.Text != string.Empty;
        }

        private void AddSetupButton_Click(object sender, RoutedEventArgs e)
        {
            _projectManager?.CurrentProject?.AddSetup(NewSetupNameTextBox.Text);
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            OpenProject();
        }

        private void OpenProject()
        {
            _projectManager.LoadProject();
        }
    }

    public static class Commands
    {
        public static readonly RoutedUICommand AddRackCommand = new RoutedUICommand("Add RackContainer", "AddRack", typeof(MainWindow));
    }
}

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
            _projectManager?.OpenUntitledProject();
            WindowState = WindowState.Maximized;

            Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            var result = MessageBox.Show(this, "Do you want to save before closing?", "Save",
                MessageBoxButton.YesNoCancel);
            switch (result)
            {
                case MessageBoxResult.Cancel:
                    cancelEventArgs.Cancel = true;
                    return;
                case MessageBoxResult.Yes:
                    _projectManager.SaveCurrentProject();
                    break;
            }
        }

        private static void RegisterRackItems()
        {
            RackArrayControl.RegisterRackItem(new AudioSourceItem(null).ItemName, new AudioSourceItem(null).CreateRackItem);
            RackArrayControl.RegisterRackItem(new SpectrumItem(null).ItemName, new SpectrumItem(null).CreateRackItem);
            RackArrayControl.RegisterRackItem(new LedItem(null).ItemName, new LedItem(null).CreateRackItem);
            RackArrayControl.RegisterRackItem(new SerialInterfaceItem(null).ItemName, new SerialInterfaceItem(null).CreateRackItem);
            RackArrayControl.RegisterRackItem(new FilterSpectrumItem(null).ItemName, new FilterSpectrumItem(null).CreateRackItem);
            RackArrayControl.RegisterRackItem(new StaticGraphicEditorItem(null).ItemName, new StaticGraphicEditorItem(null).CreateRackItem);
            RackArrayControl.RegisterRackItem(new SpectrumToBinaryDataItem(null).ItemName, new SpectrumToBinaryDataItem(null).CreateRackItem);
            RackArrayControl.RegisterRackItem(new AudioProcessorItem(null).ItemName, new AudioProcessorItem(null).CreateRackItem);
            RackArrayControl.RegisterRackItem(new SpectrumGraphicGenerator(null).ItemName, new SpectrumGraphicGenerator(null).CreateRackItem);
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

            SetupRow.Height = _projectManager.CurrentProject != null ? new GridLength(33) : new GridLength(0);
        }

        private void RackSetupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void NewSetupNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
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
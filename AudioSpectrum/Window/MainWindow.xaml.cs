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

            _projectManager = new ProjectManager(this, RackArrayContentControl);
            LeftSideWindow.AttachProjectManager(_projectManager);
            _projectManager?.OpenUntitledProject();
            _projectManager.CurrentProjectChanged += CurrentProjectChanged;
            WindowState = WindowState.Maximized;

            Closing += OnClosing;
        }

        private void CurrentProjectChanged(object sender, EventArgs eventArgs)
        {
            if (_projectManager.CurrentProject == null)
            {
                RackArrayContentControl.Content = null;
            }
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
            RackArrayWindow.RegisterRackItem(new AudioSourceItem(null).ItemName, new AudioSourceItem(null).CreateRackItem);
            RackArrayWindow.RegisterRackItem(new SpectrumItem(null).ItemName, new SpectrumItem(null).CreateRackItem);
            RackArrayWindow.RegisterRackItem(new LedItem(null).ItemName, new LedItem(null).CreateRackItem);
            RackArrayWindow.RegisterRackItem(new SerialInterfaceItem(null).ItemName, new SerialInterfaceItem(null).CreateRackItem);
            RackArrayWindow.RegisterRackItem(new FilterSpectrumItem(null).ItemName, new FilterSpectrumItem(null).CreateRackItem);
            RackArrayWindow.RegisterRackItem(new StaticGraphicEditorItem(null).ItemName, new StaticGraphicEditorItem(null).CreateRackItem);
            RackArrayWindow.RegisterRackItem(new SpectrumToBinaryDataItem(null).ItemName, new SpectrumToBinaryDataItem(null).CreateRackItem);
            RackArrayWindow.RegisterRackItem(new AudioProcessorItem(null).ItemName, new AudioProcessorItem(null).CreateRackItem);
            RackArrayWindow.RegisterRackItem(new SpectrumGraphicGenerator(null).ItemName, new SpectrumGraphicGenerator(null).CreateRackItem);
        }

        private void AddRackCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _projectManager?.CurrentProject?.RackArrayWindow?.AddRack();
        }
    }

    public static class Commands
    {
        public static readonly RoutedUICommand AddRackCommand = new RoutedUICommand("Add RackContainer", "AddRack", typeof(MainWindow));
    }
}
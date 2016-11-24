using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using AudioSpectrum.Project;
using AudioSpectrum.RackItem;

namespace AudioSpectrum.Window
{
    public partial class MainWindow : System.Windows.Window
    {
        private readonly ProjectManager _projectManager;

        public MainWindow()
        {
            InitializeComponent();
            RegisterRackItems();

            _projectManager = new ProjectManager(this, RackArrayContentControl);
            LeftSideWindow.AttachProjectManager(_projectManager);
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

            _projectManager.CloseProject();
        }

        private static void RegisterRackItems()
        {
            Window.RackArrayWindow.RegisterRackItem(new AudioSourceItem(null).ItemName, new AudioSourceItem(null).CreateRackItem);
            Window.RackArrayWindow.RegisterRackItem(new SpectrumItem(null).ItemName, new SpectrumItem(null).CreateRackItem);
            Window.RackArrayWindow.RegisterRackItem(new LedItem(null).ItemName, new LedItem(null).CreateRackItem);
            Window.RackArrayWindow.RegisterRackItem(new SerialInterfaceItem(null).ItemName, new SerialInterfaceItem(null).CreateRackItem);
            Window.RackArrayWindow.RegisterRackItem(new FilterSpectrumItem(null).ItemName, new FilterSpectrumItem(null).CreateRackItem);
            Window.RackArrayWindow.RegisterRackItem(new StaticGraphicEditorItem(null).ItemName, new StaticGraphicEditorItem(null).CreateRackItem);
            Window.RackArrayWindow.RegisterRackItem(new SpectrumToBinaryItem(null).ItemName, new SpectrumToBinaryItem(null).CreateRackItem);
            Window.RackArrayWindow.RegisterRackItem(new AudioProcessorItem(null).ItemName, new AudioProcessorItem(null).CreateRackItem);
            Window.RackArrayWindow.RegisterRackItem(new SpectrumGraphicGenerator(null).ItemName, new SpectrumGraphicGenerator(null).CreateRackItem);
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
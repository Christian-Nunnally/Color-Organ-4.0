using System;
using System.Configuration;
using System.Windows;
using AudioSpectrum.RackItems;

namespace AudioSpectrum
{
    public partial class MainWindow : Window
    {
        private ProjectManager _projectManager;

        public MainWindow()
        {
            InitializeComponent();
            RegisterRackItems();

            _projectManager = new ProjectManager(this);
            _projectManager.CurrentProjectChanged += ProjectChangedEventHandler;
            WindowState = WindowState.Maximized;
        }

        private void RegisterRackItems()
        {
            RackArrayControl.RegisterRackItem("Audio Source", new AudioSourceItem().CreateRackItem);
            RackArrayControl.RegisterRackItem("Spectrum", new SpectrumItem().CreateRackItem);
            RackArrayControl.RegisterRackItem("Led Panel", new SpectrumLedItem().CreateRackItem);
            RackArrayControl.RegisterRackItem("Serial Interface", new SerialInterfaceItem().CreateRackItem);
            RackArrayControl.RegisterRackItem("Spectrum Filter", new FilterSpectrumItem().CreateRackItem);
            RackArrayControl.RegisterRackItem("Graphic Editor", new GraphicEditorItem().CreateRackItem);
            RackArrayControl.RegisterRackItem("Binary Converter", new SpectrumToBinaryDataItem().CreateRackItem);
            RackArrayControl.RegisterRackItem("Processor", new AudioProcessorItem().CreateRackItem);
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
            RackArrayContentControl.Content = _projectManager.CurrentProject.RackArrayControl;
            ProjectNameLabel.Content = _projectManager.CurrentProject.ProjectName;
        }
    }
}

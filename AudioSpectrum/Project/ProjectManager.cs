using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using AudioSpectrum.RackItem;
using Microsoft.Win32;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace AudioSpectrum.Project
{
    public class ProjectManager
    {
        private readonly ContentControl _rackArrayContentControl;
        private readonly System.Windows.Window _window;

        private AudioSpectrum.Project.Project _project;
        public EventHandler CurrentProjectChanged;
        public SetSideRailDelegate SetSideRail;

        public ProjectManager(System.Windows.Window window, ContentControl rackArrayContentControl)
        {
            _window = window;
            _rackArrayContentControl = rackArrayContentControl;
        }

        public AudioSpectrum.Project.Project CurrentProject
        {
            get { return _project; }
            private set
            {
                _project = value;
                CurrentProjectChanged.Invoke(null, EventArgs.Empty);
            }
        }

        public void RackSetupChanged()
        {
            _rackArrayContentControl.Content = CurrentProject.RackArrayWindow;
        }

        public void OpenNewProject()
        {
            if (CurrentProject != null)
            {
                var result = MessageBox.Show(_window, "Do you want to save your current project before creating a new one?",
                    "Save current project", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                    SaveCurrentProject();

                CurrentProject.Close();
            }

            string fileName;
            if (GetFileNameDialog(out fileName, _window))
                CurrentProject = new AudioSpectrum.Project.Project(fileName, _window);
        }

        public static bool GetFileNameDialog(out string fileName, System.Windows.Window window)
        {
            fileName = "";
            var ofd = new OpenFileDialog
            {
                Multiselect = false,
                FileName = "project.cos",
                AddExtension = true,
                DefaultExt = "cos",
                CheckFileExists = false
            };

            ofd.ShowDialog(window);

            if (ofd.FileName == string.Empty)
            {
                MessageBox.Show(window, "File path can not be empty.");
                return false;
            }

            fileName = ofd.FileName;
            return true;
        }

        public void SaveCurrentProject()
        {
            if (CurrentProject == null) return;
            if (CurrentProject.ProjectPath == string.Empty) return;
            var doc = CurrentProject.SaveProject();
            doc.Save(CurrentProject.ProjectPath);
        }

        public void LoadProject()
        {
            string fileName;
            if (!GetFileNameDialog(out fileName, _window)) return;
            if (!fileName.EndsWith(".cos"))
            {
                MessageBox.Show(_window, $"{fileName} must end in '.cos'");
                return;
            }
            var doc = new XmlDocument();
            if (!File.Exists(fileName))
            {
                MessageBox.Show(_window, $"{fileName} does not exist");
                return;
            }
            doc.Load(fileName);
            CurrentProject = new AudioSpectrum.Project.Project(doc);
        }

        public void OpenUntitledProject()
        {
            if (CurrentProject != null)
            {
                MessageBox.Show(_window, "Can not create untitled project with a project currently open");
                return;
            }

            CurrentProject = new AudioSpectrum.Project.Project("Project", _window);
        }

        public void CloseProject()
        {
            CurrentProject?.Close();
            CurrentProject = null;
        }
    }
}
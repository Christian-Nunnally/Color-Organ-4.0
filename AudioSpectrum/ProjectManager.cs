using System;
using System.Windows;
using System.Xml;
using Microsoft.Win32;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace AudioSpectrum
{
    public class ProjectManager
    {
        public EventHandler CurrentProjectChanged;

        private Project _project;

        public Project CurrentProject
        {
            get
            {
                return _project;
            }
            private set
            {
                _project = value;
                CurrentProjectChanged.Invoke(null, EventArgs.Empty);
            }
        }
        private readonly Window _window;

        public ProjectManager(Window window)
        {
            _window = window;
        }

        public void OpenNewProject()
        {
            if (CurrentProject != null)
            {
                var result = MessageBox.Show(_window, "Do you want to save your current project before creating a new one?",
                    "Save current project", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    SaveCurrentProject();
                }

                CurrentProject.Close();
            }

            string fileName;
            if (GetFileNameDialog(out fileName))
            {
                CurrentProject = new Project(fileName);
            }
        }

        private bool GetFileNameDialog(out string fileName)
        {
            fileName = "";
            var ofd = new OpenFileDialog
            {
                Multiselect = false,
                FileName = "project.cos",
                AddExtension = true,
                DefaultExt = "cos",
                CheckFileExists = false,
            };

            ofd.ShowDialog(_window);



            if (ofd.FileName == string.Empty)
            {
                MessageBox.Show(_window, "File path can not be empty.");
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
            if (!GetFileNameDialog(out fileName)) return;
            if (!fileName.EndsWith(".cos"))
            {
                MessageBox.Show(_window, "Must be a .cos file");
            }
            var doc = new XmlDocument();
            doc.Load(fileName);
            CurrentProject = new Project(doc);
        }
    }
}

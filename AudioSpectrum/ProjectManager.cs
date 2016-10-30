

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
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
            set
            {
                _project = value;
                CurrentProjectChanged.Invoke(null, EventArgs.Empty);
            }
        }
        private Window _window;

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
                    // TODO: Save project here.    
                }

                CurrentProject.Close();
            }

            var ofd = new OpenFileDialog
            {
                Multiselect = false,
                FileName = "project1.cos",
                AddExtension = true,
                DefaultExt = "cos",
                CheckFileExists = false,
            };

            ofd.ShowDialog(_window);

            if (ofd.FileName == "")
            {
                MessageBox.Show(_window, "File path can not be empty.");
                return;
            }

            CurrentProject = new Project(ofd.FileName);
        }
    }
}

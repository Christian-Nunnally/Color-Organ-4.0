using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace AudioSpectrum.Project
{
    public class Project
    {

        private readonly System.Windows.Window _window;

        public readonly ObservableCollection<RackSetup> RackSetups = new ObservableCollection<RackSetup>();

        private RackSetup _selectedRackSetup;

        public Project(string projectPath, System.Windows.Window window)
        {
            _window = window;

            if (projectPath != "Project")
            {
                ProjectPath = projectPath;
                var sp = projectPath.Split('\\');
                ProjectName = sp.Length > 0 ? sp[sp.Length - 1].Split('.')[0] : "Invalid Project Name".Split('.')[0];
            }
            else
            {
                ProjectName = projectPath;
            }

            RackSetups.Add(new RackSetup("Default Setup"));
        }

        public Project(XmlDocument xml)
        {
            if (xml.DocumentElement == null) throw new ProjectLoadException();
            for (var i = 0; i < xml.DocumentElement.ChildNodes.Count; i++)
            {
                var node = xml.DocumentElement.ChildNodes.Item(i);
                if (node == null) continue;
                switch (node.Name)
                {
                    case "ProjectPath":
                        ProjectPath = node.InnerText;
                        var sp = ProjectPath?.Split('\\');
                        ProjectName = sp.Length > 0 ? sp[sp.Length - 1].Split('.')[0] : "Invalid Project Name".Split('.')[0];
                        break;
                    case "RackSetup":
                        AddSetup(node);
                        break;
                }
            }
        }

        public string ProjectName { get; private set; }
        public string ProjectPath { get; private set; }

        public RackSetup SelectedRackSetup
        {
            private get { return _selectedRackSetup; }
            set
            {
                if (RackSetups.Contains(value))
                    _selectedRackSetup = value;
            }
        }

        public Window.RackArrayWindow RackArrayWindow => SelectedRackSetup?.RackArrayWindow;

        public void AddSetup(string setupName)
        {
            if (RackSetups.Any(rackSetup => rackSetup.Name == setupName))
                return;
            
            RackSetups.Add(new RackSetup(setupName));
        }

        private void AddSetup(XmlNode xml)
        {
            RackSetups.Add(new RackSetup(xml));
        }

        public XmlDocument SaveProject()
        {
            if (ProjectName == "Project")
            {
                string fileName;
                if (ProjectManager.GetFileNameDialog(out fileName, _window))
                {
                    ProjectPath = fileName;
                    var sp = fileName.Split('\\');
                    ProjectName = sp.Length > 0 ? sp[sp.Length - 1].Split('.')[0] : "Invalid Project Name".Split('.')[0];
                }
            }

            var xml = new XmlDocument();
            xml.AppendChild(xml.CreateElement("Project"));
            var projectNode = xml.DocumentElement;

            // ReSharper disable once PossibleNullReferenceException
            projectNode.AppendChild(xml.CreateElement("ProjectPath")).InnerText = ProjectPath;

            foreach (var rackSetup in RackSetups)
                rackSetup.Save(xml, projectNode);

            return xml;
        }

        public void Close()
        {
            foreach (var rackSetup in RackSetups)
            {
                rackSetup.Close();
            }
        }
    }

    public class ProjectLoadException : Exception // TODO: Add exception messeges
    {
    }
}
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;

namespace AudioSpectrum
{
    public class Project
    {
        public string ProjectName { get; }
        public string ProjectPath{ get; private set; }

        public readonly ObservableCollection<RackSetup> RackSetups = new ObservableCollection<RackSetup>();

        private RackSetup _selectedRackSetup;
        public RackSetup SelectedRackSetup
        {
            private get { return _selectedRackSetup; }
            set
            {
                if (RackSetups.Contains(value))
                {
                    _selectedRackSetup = value;
                }
            }
        }

        public RackArrayControl RackArrayControl => SelectedRackSetup?.RackArrayControl;

        public Project(string projectPath)
        {
            ProjectPath = projectPath;
            var sp = projectPath.Split('\\');
            ProjectName = sp.Length > 0 ? sp[sp.Length - 1].Split('.')[0] : "Invalid Project Name".Split('.')[0];

            RackSetups.Add(new RackSetup("Default Setup"));
        }

        public Project(XmlDocument xml)
        {
            if (xml.DocumentElement == null) throw new ProjectLoadException();
            ProjectPath = xml.DocumentElement?.InnerText;
            var sp = ProjectPath?.Split('\\');
            ProjectName = sp.Length > 0 ? sp[sp.Length - 1].Split('.')[0] : "Invalid Project Name".Split('.')[0];

            for (var i = 0; i < xml.DocumentElement.ChildNodes.Count; i++)
            {
                var node = xml.DocumentElement.ChildNodes.Item(i);
                if (node != null && node.Name == "RackSetups")
                {
                    AddSetup(node.InnerText, node);
                }
            }
        }

        public void AddSetup(string setupName, XmlNode xml = null)
        {
            if (RackSetups.Any(rackSetup => rackSetup.Name == setupName))
            {
                return;
            }

            var setup = new RackSetup(setupName);
            RackSetups.Add(setup);

            if (xml == null) return;
            for (var i = 0; i < xml.ChildNodes.Count; i++)
            {
                var node = xml.ChildNodes.Item(i);
                if (node == null) continue;
                if (node.Name == "StackPanel")
                {
                    setup.RackArrayControl.AddRack();
                }
            }
        }

        public XmlDocument SaveProject()
        {
            var xml = new XmlDocument();
            var projectElement = xml.AppendChild(xml.CreateElement("Project"));
            if (projectElement != null) projectElement.InnerText = ProjectPath;

            var rackSetupElement = xml.DocumentElement?.AppendChild(xml.CreateElement("RackSetups"));

            if (rackSetupElement == null) return xml;
            foreach (var rackSetup in RackSetups)
            {
                rackSetup.Save(xml, rackSetupElement);
            }

            return xml;
        }

        public void Close()
        {
            
        }
    }

    public class ProjectLoadException : Exception
    {
    }
}

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;

namespace AudioSpectrum
{
    public class Project
    {
        public string ProjectName { get; }
        public string ProjectPath{ get; }

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
                    case "RackSetups":
                        LoadRackSetups(node);
                        break;
                }
            }
        }

        private void LoadRackSetups(XmlNode node)
        {
            foreach (var rackSetupNode in node.ChildNodes.OfType<XmlNode>())
            {
                if (rackSetupNode.Name != "RackSetup") continue;
                AddSetup("", rackSetupNode);
            }
        }

        public void AddSetup(string setupName, XmlNode xml = null)
        {
            if (RackSetups.Any(rackSetup => rackSetup.Name == setupName))
            {
                return;
            }

            if (xml != null)
            {
                foreach (var node in xml.ChildNodes.OfType<XmlNode>())
                {
                    if (node.Name == "SetupName") setupName = node.InnerText;
                }
            }
            else if (setupName == string.Empty)
            {
                throw new InvalidOperationException("Must supply an xml node to retrive the setup name from if the setup name is left blank.");
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
                    setup.RackArrayControl.AddRack(node);
                }
            }
        }

        public XmlDocument SaveProject()
        {
            var xml = new XmlDocument();
            xml.AppendChild(xml.CreateElement("Project"));
            if (xml.DocumentElement == null) throw new ProjectLoadException();
            xml.DocumentElement.AppendChild(xml.CreateElement("ProjectPath")).InnerText = ProjectPath;

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

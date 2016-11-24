using System;
using System.Linq;
using System.Xml;
using AudioSpectrum.Interfaces;

namespace AudioSpectrum.Project
{
    [Serializable]
    public class RackSetup : ISaveable
    {
        public RackSetup(string name)
        {
            RackArrayWindow = new Window.RackArrayWindow();
            Name = name;
        }

        public RackSetup(XmlNode xml)
        {
            RackArrayWindow = new Window.RackArrayWindow();
            Load(xml);
        }

        public Window.RackArrayWindow RackArrayWindow { get; }

        public string Name { get; private set; }

        public void Save(XmlDocument xml, XmlNode parent)
        {
            var rackElement = parent.AppendChild(xml.CreateElement("RackSetup"));
            rackElement.AppendChild(xml.CreateElement("SetupName")).InnerText = Name;
            RackArrayWindow.Save(xml, rackElement);
        }

        public void Load(XmlNode xml)
        {
            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
            {
                if (node.Name == "SetupName") Name = node.InnerText;
                if (node.Name == "StackPanel") RackArrayWindow.AddRack(node);
            }
        }

        public void Close()
        {
            RackArrayWindow.Close();
        }
    }
}
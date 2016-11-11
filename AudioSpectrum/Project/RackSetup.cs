using System;
using System.Linq;
using System.Xml;

namespace AudioSpectrum
{
    [Serializable]
    public class RackSetup : ISaveable
    {
        public RackSetup(string name)
        {
            RackArrayWindow = new RackArrayWindow();
            Name = name;
        }

        public RackSetup(XmlNode xml) // TODO: Enable loading through the constructor
        {
            RackArrayWindow = new RackArrayWindow();
            Load(xml);
        }

        public RackArrayWindow RackArrayWindow { get; }

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
                if (node.Name == "SetupName") Name = node.InnerText;
        }

        public void Close()
        {
            RackArrayWindow.Close();
        }
    }
}
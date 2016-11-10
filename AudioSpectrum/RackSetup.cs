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
            RackArrayControl = new RackArrayControl();
            Name = name;
        }

        public RackSetup(XmlNode xml) // TODO: Enable loading through the constructor
        {
            RackArrayControl = new RackArrayControl();
            Load(xml);
        }

        public RackArrayControl RackArrayControl { get; }
        public string Name { get; private set; }

        public void Save(XmlDocument xml, XmlNode parent)
        {
            var rackElement = parent.AppendChild(xml.CreateElement("RackSetup"));
            rackElement.AppendChild(xml.CreateElement("SetupName")).InnerText = Name;
            RackArrayControl.Save(xml, rackElement);
        }

        public void Load(XmlNode xml)
        {
            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
                if (node.Name == "SetupName") Name = node.InnerText;
        }
    }
}
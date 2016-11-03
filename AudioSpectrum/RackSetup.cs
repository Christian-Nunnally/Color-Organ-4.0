using System;
using System.Linq;
using System.Xml;

namespace AudioSpectrum
{
    [Serializable]
    public class RackSetup : ISaveable
    {
        public RackArrayControl RackArrayControl { get; set; }
        public string Name { get; set; }

        public RackSetup(string name)
        {
            RackArrayControl = new RackArrayControl();
            Name = name;
        }

        public RackSetup(XmlElement xml)
        {
            RackArrayControl = new RackArrayControl();
            Load(xml);
        }

        public void Save(XmlDocument xml, XmlNode parent)
        {
            var rackElement = parent.AppendChild(xml.CreateElement("RackSetup"));
            rackElement.AppendChild(xml.CreateElement("SetupName")).InnerText = Name;
            RackArrayControl.Save(xml, rackElement);
        }

        public void Load(XmlNode xml)
        {
            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
            {
                if (node.Name == "Name") Name = node.InnerText;
            }
        }
    }
}

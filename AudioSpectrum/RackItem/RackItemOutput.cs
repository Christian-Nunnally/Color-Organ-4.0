using System;
using System.Linq;
using System.Xml;
using AudioSpectrum.Interfaces;

namespace AudioSpectrum.RackItem
{
    public class RackItemOutput : ISaveable
    {
        private static readonly Random Rnd = new Random();

        public RackItemOutput(string visibleName)
        {
            VisibleName = visibleName;
            Key = (long)(Rnd.NextDouble() * long.MaxValue);
        }

        public RackItemOutput(XmlNode xml)
        {
            Load(xml);
        }

        public string VisibleName { get; set; }

        public long Key { get; private set; }

        public int OutputNumber { private get; set; }

        public void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement("Output"));
            node.AppendChild(xml.CreateElement("VisibleName")).InnerText = VisibleName;
            node.AppendChild(xml.CreateElement("Key")).InnerText = Key.ToString();
            node.AppendChild(xml.CreateElement("OutputNumber")).InnerText = OutputNumber.ToString();
        }

        public void Load(XmlNode xml)
        {
            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
                switch (node.Name)
                {
                    case "VisibleName":
                        VisibleName = node.InnerText;
                        break;
                    case "Key":
                        Key = long.Parse(node.InnerText);
                        break;
                    case "OutputNumber":
                        OutputNumber = int.Parse(node.InnerText);
                        break;
                }
        }

        public override string ToString()
        {
            return VisibleName;
        }
    }
}
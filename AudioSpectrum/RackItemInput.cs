using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AudioSpectrum.RackItems;

namespace AudioSpectrum
{
    public class RackItemInput : ISaveable
    {
        public string VisibleName { get; private set; }

        public long Key { get; private set; }

        public Pipe Pipe { get; private set; }

        public RackItemInput(string visibleName, Pipe pipeIn)
        {
            VisibleName = visibleName;
            Key = (long) (new Random().NextDouble() * long.MaxValue);
            Pipe = pipeIn;
        }

        public RackItemInput(XmlNode xml)
        {
            Load(xml);
        }

        public void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement("Input"));
            node.AppendChild(xml.CreateElement("VisibleName")).InnerText = VisibleName;
            node.AppendChild(xml.CreateElement("Key")).InnerText = Key.ToString();
        }

        public void Load(XmlNode xml)
        {
            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
            {
                switch (node.Name)
                {
                    case "VisibleName":
                        VisibleName = node.InnerText;
                        break;
                    case "Key":
                        Key = long.Parse(node.InnerText);
                        break;
                }
            }
        }
    }
}

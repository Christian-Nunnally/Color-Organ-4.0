using System;
using System.Linq;
using System.Xml;
using AudioSpectrum.Interfaces;
using AudioSpectrum.RackItem;

namespace AudioSpectrum.RackItem
{
    public class RackItemInput : ISaveable
    {
        private static readonly Random Rnd = new Random();

        public RackItemInput(string visibleName, Pipe pipeIn)
        {
            VisibleName = visibleName;
            Key = (long)(Rnd.NextDouble() * long.MaxValue);
            Pipe = pipeIn;
        }

        public RackItemInput(XmlNode xml)
        {
            Load(xml);
        }

        private string VisibleName { get; set; }

        private long Key { get; set; }

        public Pipe Pipe { get; set; }

        public int InputNumber { get; set; }

        public string ConnectedOutput { get; set; }

        public void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement("Input"));
            node.AppendChild(xml.CreateElement("VisibleName")).InnerText = VisibleName;
            node.AppendChild(xml.CreateElement("Key")).InnerText = Key.ToString();
            node.AppendChild(xml.CreateElement("InputNumber")).InnerText = InputNumber.ToString();
            node.AppendChild(xml.CreateElement("ConnectedOutput")).InnerText = ConnectedOutput;
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
                    case "InputNumber":
                        InputNumber = int.Parse(node.InnerText);
                        break;
                    case "ConnectedOutput":
                        ConnectedOutput = node.InnerText;
                        break;
                }
        }
    }
}
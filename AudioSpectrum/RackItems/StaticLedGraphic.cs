using System;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    public class StaticLedGraphic : ISaveable
    {
        public string Name { get; private set; }
        public byte[] Graphic { get; }

        public StaticLedGraphic(XmlNode xml)
        {
            Graphic = new byte[64 * 3];
            Load(xml);
        }

        public StaticLedGraphic(string name)
        {
            Graphic = new byte[64*3];
            Name = name;
        }

        public void SetPixel(int pixelNumber, Color color)
        {
            if (pixelNumber < 0 || pixelNumber >= 64) throw new ArgumentException("color");
            Graphic[pixelNumber] = color.R;
            Graphic[pixelNumber + 64] = color.G;
            Graphic[pixelNumber + 128] = color.B;
        }

        public void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement("StaticGraphic"));
            node.AppendChild(xml.CreateElement("GraphicName")).InnerText = Name;
            node.AppendChild(xml.CreateElement("Graphic")).InnerText = ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var b in Graphic)
            {
                sb.Append(b);
                sb.Append("-");
            }
            return sb.ToString();
        }

        public void Load(XmlNode xml)
        {
            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
            {
                switch (node.Name)
                {
                    case "GraphicName":
                        Name = node.InnerText;
                        break;
                    case "Graphic":
                        var splitBytes = node.InnerText.Split(new []{ '-' }, Graphic.Length);
                        if (Graphic.Length == splitBytes.Length)
                        {
                            var i = 0;
                            foreach (var splitByte in splitBytes)
                            {
                                byte @byte;
                                if (byte.TryParse(splitByte, out @byte))
                                {
                                    Graphic[i] = @byte;
                                }
                                i++;
                            }
                        }
                        else
                        {
                            throw new ProjectLoadException();
                        }
                        break;
                }
            }
        }
    }
}

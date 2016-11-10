using System.Xml;

namespace AudioSpectrum
{
    public interface ISaveable
    {
        void Save(XmlDocument xml, XmlNode parent);

        void Load(XmlNode xml);
    }
}
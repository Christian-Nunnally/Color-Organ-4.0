using System.Xml;

namespace AudioSpectrum.Interfaces
{
    public interface ISaveable
    {
        void Save(XmlDocument xml, XmlNode parent);

        void Load(XmlNode xml);
    }
}
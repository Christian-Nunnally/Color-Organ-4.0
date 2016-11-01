using System.Collections.Generic;
using System.Windows.Controls;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    public abstract class RackItemBase : UserControl, IRackItem
    {
        protected RackItemContainer RackContainer;

        public abstract IRackItem CreateRackItem();

        public abstract Dictionary<string, Pipe> GetInputs();

        public abstract List<string> GetOutputs();

        public void SetRack(RackItemContainer rack)
        {
            RackContainer = rack;
        }
        public abstract void Save(XmlDocument xml, XmlNode parent);

        public abstract void Load(XmlElement xml);

        public string ItemName { get; set; }

        public virtual void CleanUp()
        {
        }

        public virtual bool CanDelete()
        {
            return true;
        }

        public virtual void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
        }

        public virtual void HeartBeat()
        {
        }
    }
}

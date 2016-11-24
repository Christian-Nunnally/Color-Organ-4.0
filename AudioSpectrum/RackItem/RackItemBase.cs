using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using AudioSpectrum.RackItem;

namespace AudioSpectrum.RackItem
{
    public abstract class RackItemBase : UserControl, IRackItem
    {

        protected const string RackItemName = "RackItem";
        private readonly List<RackItemInput> _rackItemInputs = new List<RackItemInput>();
        protected readonly List<RackItemOutput> RackItemOutputs = new List<RackItemOutput>();

        protected RackItemContainer RackContainer;

        public abstract IRackItem CreateRackItem(XmlElement xml);

        public IEnumerable<RackItemInput> GetInputs()
        {
            return _rackItemInputs;
        }

        public IEnumerable<RackItemOutput> GetOutputs()
        {
            return RackItemOutputs;
        }

        public void SetRack(RackItemContainer rack)
        {
            RackContainer = rack;
        }

        public abstract void Save(XmlDocument xml, XmlNode parent);

        public abstract void Load(XmlNode xml);

        public string ItemName { get; protected set; }

        public List<UIElement> SideRailItems { get; set; }

        public virtual void CleanUp()
        {
        }

        public virtual bool CanDelete()
        {
            return true;
        }

        public virtual void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            sideRailSetter.Invoke(ItemName, new List<UIElement>());
        }

        public void AddOutput(RackItemOutput output)
        {
            RackItemOutputs.Add(output);
            output.OutputNumber = RackItemOutputs.Count;
        }

        public void AddInput(RackItemInput input)
        {
            _rackItemInputs.Add(input);
            input.InputNumber = _rackItemInputs.Count;
        }

        public void RenameOutput(long key, string newName)
        {
            var rackItemOutput = RackItemOutputs.FirstOrDefault(x => x.Key == key);
            if (rackItemOutput != null)
                rackItemOutput.VisibleName = newName;
        }

        protected void SaveInputs(XmlDocument xml, XmlNode node)
        {
            foreach (var rackItemInput in _rackItemInputs)
                rackItemInput.Save(xml, node);
        }

        protected void SaveOutputs(XmlDocument xml, XmlNode node)
        {
            foreach (var rackItemOutput in RackItemOutputs)
                rackItemOutput.Save(xml, node);
        }

        protected void AttachPipeToInput(int inputNumber, Pipe pipe)
        {
            var input = _rackItemInputs.FirstOrDefault(x => x.InputNumber == inputNumber);
            if (input != null)
                input.Pipe = pipe;
        }

        protected void LoadInputsAndOutputs(XmlNode xml)
        {
            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
                switch (node.Name)
                {
                    case "Input":
                        AddInput(new RackItemInput(node));
                        break;
                    case "Output":
                        AddOutput(new RackItemOutput(node));
                        break;
                }
        }

        public bool IsSelected { get; set; }
    }
}
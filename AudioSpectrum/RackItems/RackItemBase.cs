using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    public abstract class RackItemBase : UserControl, IRackItem
    {
        public readonly List<RackItemInput> RackItemInputs = new List<RackItemInput>();
        public readonly List<RackItemOutput> RackItemOutputs = new List<RackItemOutput>();

        protected const string RackItemName = "RackItem";

        protected RackItemContainer RackContainer;

        public abstract IRackItem CreateRackItem(XmlElement xml);
        public List<RackItemInput> GetInputs()
        {
            return RackItemInputs;
        }

        public List<RackItemOutput> GetOutputs()
        {
            return RackItemOutputs;
        }

        public void SetRack(RackItemContainer rack)
        {
            RackContainer = rack;
        }
        public abstract void Save(XmlDocument xml, XmlNode parent);

        public abstract void Load(XmlNode xml);

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
            sideRailSetter.Invoke(ItemName, new List<Control>());
        }

        public void AddOutput(RackItemOutput output)
        {
            RackItemOutputs.Add(output);
            output.OutputNumber = RackItemOutputs.Count;
        }

        public void AddInput(RackItemInput input)
        {
            RackItemInputs.Add(input);
            input.InputNumber = RackItemInputs.Count;
        }

        public void RenameOutput(long key, string newName)
        {
            var rackItemOutput = RackItemOutputs.FirstOrDefault(x => x.Key == key);
            if (rackItemOutput != null)
            {
                rackItemOutput.VisibleName = newName;
            }
        }

        public virtual void HeartBeat()
        {
        }

        protected void SaveInputs(XmlDocument xml, XmlNode node)
        {
            foreach (var rackItemInput in RackItemInputs)
            {
                rackItemInput.Save(xml, node);
            }
        }

        protected void SaveOutputs(XmlDocument xml, XmlNode node)
        {
            foreach (var rackItemOutput in RackItemOutputs)
            {
                rackItemOutput.Save(xml, node);
            }
        }

        protected void AttachPipeToInput(int inputNumber, Pipe pipe)
        {
            var input = RackItemInputs.FirstOrDefault(x => x.InputNumber == inputNumber);
            if (input != null)
            {
                input.Pipe = pipe;
            }
        }

        protected void LoadInputsAndOutputs(XmlNode xml)
        {
            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
            {
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
        }
    }
}

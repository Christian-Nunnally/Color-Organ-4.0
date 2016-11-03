using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    public abstract class RackItemBase : UserControl, IRackItem
    {
        protected const string RackItemName = "RackItem";

        public RackItemContainer RackContainer;

        public abstract IRackItem CreateRackItem();

        public abstract Dictionary<string, Pipe> GetInputs();

        public abstract List<string> GetOutputs();

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
        }

        public virtual void HeartBeat()
        {
        }

        protected void SaveInputs(XmlDocument xml, XmlNode node)
        {
            if (RackContainer == null) return;
            var outputsNode = node.AppendChild(xml.CreateElement("Inputs"));

            foreach (var mapItem in RackContainer.GetInputToConnectedOutputNames())
            {
                var outputNode = outputsNode.AppendChild(xml.CreateElement("Input"));
                outputNode.AppendChild(xml.CreateElement("InputName")).InnerText = mapItem.Key;
                outputNode.AppendChild(xml.CreateElement("ConnectedOutputName")).InnerText = mapItem.Value;
            }
        }

        protected void SaveOutputs(XmlDocument xml, XmlNode node)
        {
            if (RackContainer == null) return;
            var outputsNode = node.AppendChild(xml.CreateElement("Outputs"));
            
            foreach (var mapItem in RackContainer.GetInternalToExternalOutputNames())
            {
                var outputNode = outputsNode.AppendChild(xml.CreateElement("Output"));
                outputNode.AppendChild(xml.CreateElement("InternalName")).InnerText = mapItem.Key;
                outputNode.AppendChild(xml.CreateElement("ExternalName")).InnerText = mapItem.Value;
            }
        }

        protected void LoadInputs(XmlNode node)
        {
            if (RackContainer == null) return;
            var inputConnectionMap = new Dictionary<string, string>();
            if (node.Name != "Inputs") throw new ProjectLoadException();
            foreach (var nodeChildNode in node.ChildNodes.OfType<XmlNode>())
            {
                if (nodeChildNode.Name != "Input") throw new ProjectLoadException();
                var inputName = "";
                var connectedOutputName = "";
                foreach (var nameNode in nodeChildNode.ChildNodes.OfType<XmlNode>())
                {
                    if (nameNode.Name == "InputName") inputName = nameNode.InnerText;
                    if (nameNode.Name == "ConnectedOutputName") connectedOutputName = nameNode.InnerText;
                }
                if (inputName != string.Empty && connectedOutputName != string.Empty) inputConnectionMap.Add(inputName, connectedOutputName);
            }
            RackContainer.SetInputs(inputConnectionMap);
        }

        protected void LoadOutputs(XmlNode node)
        {
            if (RackContainer == null) return;
            var outputRenameMap = new Dictionary<string, string>();
            if (node.Name != "Outputs") throw new ProjectLoadException();
            foreach (var nodeChildNode in node.ChildNodes.OfType<XmlNode>())
            {
                if (nodeChildNode.Name != "Output") throw new ProjectLoadException();
                var internalName = "";
                var externalName = "";
                foreach (var nameNode in nodeChildNode.ChildNodes.OfType<XmlNode>())
                {
                    if (nameNode.Name == "InternalName") internalName = nameNode.InnerText;
                    if (nameNode.Name == "ExternalName") externalName = nameNode.InnerText;
                }
                if (internalName != string.Empty && externalName != string.Empty) outputRenameMap.Add(internalName, externalName);
            }
            RackContainer.RenameOutputs(outputRenameMap);
        }
    }
}

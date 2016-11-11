using System.Collections.Generic;
using System.Windows;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    public delegate void Pipe(List<byte> data);

    public delegate void SetSideRailDelegate(string name, List<UIElement> controls);

    public delegate IRackItem RackItemFactory(XmlElement xml);

    public interface IRackItem : ISaveable, ISelectable
    {
        string ItemName { get; }

        List<UIElement> SideRailItems { get; set; }

        IRackItem CreateRackItem(XmlElement xml);

        IEnumerable<RackItemInput> GetInputs();

        IEnumerable<RackItemOutput> GetOutputs();

        void SetRack(RackItemContainer rack);

        void CleanUp();

        bool CanDelete();

        void SetSideRail(SetSideRailDelegate sideRailSetter);

        void AddOutput(RackItemOutput output);

        void AddInput(RackItemInput input);

        void RenameOutput(long key, string newName);
    }
}
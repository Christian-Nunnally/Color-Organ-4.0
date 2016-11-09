using System.Collections.Generic;
using System.Windows.Controls;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    public delegate void Pipe(List<byte> data, int depth);
    public delegate void SetSideRailDelegate(string name, List<Control> controls);

    public delegate IRackItem RackItemFactory(XmlElement xml);

    public interface IRackItem : ISaveable
    {
        string ItemName { get; set; }

        IRackItem CreateRackItem(XmlElement xml);

        List<RackItemInput> GetInputs();

        List<RackItemOutput> GetOutputs();

        void SetRack(RackItemContainer rack);

        void CleanUp();

        bool CanDelete();

        void SetSideRail(SetSideRailDelegate sideRailSetter);

        void AddOutput(RackItemOutput output);

        void AddInput(RackItemInput input);

        void RenameOutput(long key, string newName);

        /// <summary>
        /// This function gets run on a the RackCableManagers Heartbeat thread.
        /// </summary>
        void HeartBeat();
    }
}
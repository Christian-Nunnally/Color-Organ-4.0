using System.Collections.Generic;
using System.Windows.Controls;

namespace AudioSpectrum.RackItems
{
    public delegate void Pipe(List<byte> data, int depth);
    public delegate void SetSideRailDelegate(string name, List<Control> controls);

    public delegate IRackItem RackItemFactory();

    public interface IRackItem : ISaveable
    {
        string ItemName { get; set; }

        IRackItem CreateRackItem();

        Dictionary<string, Pipe> GetInputs();

        List<string> GetOutputs();

        void SetRack(RackItemContainer rack);

        void CleanUp();

        bool CanDelete();

        void SetSideRail(SetSideRailDelegate sideRailSetter);

        /// <summary>
        /// This function gets run on a the RackCableManagers Heartbeat thread.
        /// </summary>
        void HeartBeat();
    }
}
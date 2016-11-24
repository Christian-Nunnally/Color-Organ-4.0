using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml;
using AudioSpectrum.RackItem;

namespace AudioSpectrum.RackItem
{
    public partial class LedItem : RackItemBase
    {

        private readonly List<byte[]> _graphicsIns = new List<byte[]>();

        public LedItem(XmlNode xml)
        {
            InitializeComponent();
            ItemName = "LedDisplay";
            _graphicsIns.Add(new byte[64 * 3]);
            _graphicsIns.Add(new byte[64 * 3]);

            if (xml == null)
            {
                AddInput(new RackItemInput("Graphics In 1", SpectrumIn1));
                AddInput(new RackItemInput("Graphics In 2", SpectrumIn2));
                AddOutput(new RackItemOutput("Graphics Out"));
            }
            else
            {
                Load(xml);
            }
        }

        public override IRackItem CreateRackItem(XmlElement xml)
        {
            return new LedItem(xml);
        }

        private void SpectrumIn1(List<byte> data)
        {
            if (data.Count >= 64 * 3) _graphicsIns[0] = data.ToArray();

            var compositeGraphic = new byte[64 * 3];

            var i = 0;
            foreach (var t in _graphicsIns)
            {
                i++;
                if ((i == 1) && !Input1CheckBox.IsChecked.Value) continue;
                if ((i == 2) && !Input2CheckBox.IsChecked.Value) continue;
                for (var p = 0; p < 64; p++)
                {
                    var r = t[p];
                    var g = t[p + 64];
                    var b = t[p + 128];
                    if ((compositeGraphic[p] == 0) && (compositeGraphic[p + 64] == 0) && (compositeGraphic[p + 128] == 0))
                    {
                        compositeGraphic[p] = r;
                        compositeGraphic[p + 64] = g;
                        compositeGraphic[p + 128] = b;
                    }
                    else
                    {
                        compositeGraphic[p] = (byte)((r + compositeGraphic[p]) / 2);
                        compositeGraphic[p + 64] = (byte)((g + compositeGraphic[p + 64]) / 2);
                        compositeGraphic[p + 128] = (byte)((b + compositeGraphic[p + 128]) / 2);
                    }
                }
            }

            LedSimulator.Set(compositeGraphic);

            if (RackItemOutputs.Count > 0)
                RackContainer.OutputPipe(RackItemOutputs.First(), compositeGraphic.ToList());
        }

        private void SpectrumIn2(List<byte> data)
        {
            if (data.Count >= 64 * 3) _graphicsIns[1] = data.ToArray();
        }

        public override void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            sideRailSetter.Invoke(ItemName, new List<UIElement>());
        }

        public override void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement(RackItemName + "-" + ItemName));
            SaveOutputs(xml, node);
            SaveInputs(xml, node);
        }

        public sealed override void Load(XmlNode xml)
        {
            LoadInputsAndOutputs(xml);
            AttachPipeToInput(1, SpectrumIn1);
            AttachPipeToInput(2, SpectrumIn2);
        }
    }
}
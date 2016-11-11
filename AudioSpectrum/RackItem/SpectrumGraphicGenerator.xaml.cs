﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    public partial class SpectrumGraphicGenerator : RackItemBase
    {

        public SpectrumGraphicGenerator(XmlNode xml)
        {
            InitializeComponent();
            ItemName = "SpectrumGraphicGenerator";

            if (xml == null)
            {
                AddInput(new RackItemInput("Spectrum", SpectrumIn));
                AddOutput(new RackItemOutput("Spectrum Graphic"));
            }
            else
            {
                Load(xml);
            }
        }

        public override IRackItem CreateRackItem(XmlElement xml)
        {
            return new SpectrumGraphicGenerator(xml);
        }

        private void SpectrumIn(List<byte> data)
        {
            data = new List<byte>(data);

            if (data.Count == 0) return;

            while (data.Count < 8)
                for (var i = 0; i < data.Count; i += 2)
                    data.Insert(i + 1, data[i]);

            while (data.Count >= 16)
                for (var i = 0; i < data.Count; i += 1)
                    data.RemoveAt(i);

            var graphicsData = new byte[64 * 3];

            var colStart = 0;
            for (var i = 0; i < 8; i += 1)
            {
                var barValue = (data[i] + 1) / 32;
                for (var j = 0; j < barValue; j++)
                {
                    graphicsData[colStart + j] = (byte)(10 + 5 * j);
                    graphicsData[64 + colStart + j] = (byte)(50 - 5 * j);
                    graphicsData[128 + colStart + j] = 10;
                }
                colStart += 8;
            }

            if (RackItemOutputs.Count > 0)
                RackContainer.OutputPipe(RackItemOutputs.First(), graphicsData.ToList());
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
            AttachPipeToInput(1, SpectrumIn);
        }
    }
}
using System;
using System.Threading;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Concurrent;
using System.Linq;
using System.Xml;

namespace AudioSpectrum.RackItems
{
    public partial class SerialInterfaceItem : RackItemBase
    {
        private const int PacketSize = 64;

        private bool _waitForSync;
        private bool _serialInterfaceExists;
        private bool _isSerialThreadRunning;

        private SerialPort Serial { get; set; }

        private readonly ConcurrentQueue<byte[]> _ouputQueue = new ConcurrentQueue<byte[]>();
        private Thread _arduinoInterfaceBufferThread;

        public SerialInterfaceItem()
        {
            InitializeComponent();
            ItemName = "SerialInterface";
            GetPorts();

            if (ComPortSelector.Items.Count > 0)
            {
                ComPortSelector.SelectedIndex = 0;
            }
        }

        public override void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            sideRailSetter?.Invoke(ItemName, new List<Control>());
        }

        private void ComPortSelector_DropDownOpened(object sender, EventArgs e)
        {
            GetPorts();
        }

        private void GetPorts()
        {
            ComPortSelector.Items.Clear();
            var ports = SerialPort.GetPortNames();
            foreach (var port in ports) ComPortSelector.Items.Add(port);
        }

        private void SerialEnable_Click(object sender, RoutedEventArgs e)
        {
            ComPortSelector.IsEnabled = false;
            if (ComPortSelector.Items.Count <= 0 || ComPortSelector.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (SerialEnable.IsChecked == true)
                {
                    Serial = new SerialPort((ComPortSelector.Items[ComPortSelector.SelectedIndex] as string))
                    {
                        BaudRate = 115200,
                        StopBits = StopBits.One,
                        Parity = Parity.None,
                        DataBits = 8,
                        DtrEnable = true
                    };
                    Serial.Open();
                    ThreadStart threadStart = RunArduinoSerialInterfaceBuffer;
                    _serialInterfaceExists = true;
                    SyncCheck.IsEnabled = true;
                    _arduinoInterfaceBufferThread = new Thread(threadStart);
                    _arduinoInterfaceBufferThread.Start();
                }
                else
                {
                    ComPortSelector.IsEnabled = true;
                    if (Serial == null) return;
                    SyncCheck.IsEnabled = false;
                    _serialInterfaceExists = false;
                    while (!_isSerialThreadRunning) { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        public override IRackItem CreateRackItem()
        {
            return new SerialInterfaceItem();
        }

        public override Dictionary<string, Pipe> GetInputs()
        {
            var inputs = new Dictionary<string, Pipe> {{"Graphics Input", GraphicsDataIn}};
            return inputs;
        }

        public override List<string> GetOutputs()
        {
            return new List<string>();
        }

        private void GraphicsDataIn(List<byte> graphicsData, int iteration)
        {
            if (Serial == null || !Serial.IsOpen) return;

            for (var i = 0; i < 64 * 3; i += PacketSize)
            {
                _ouputQueue.Enqueue(graphicsData.GetRange(i, PacketSize).ToArray());
            }

            OutgoingPacketCountLabel.Content = _ouputQueue.Count;
        }

        private void RunArduinoSerialInterfaceBuffer()
        {
            _isSerialThreadRunning = true;
            while (_serialInterfaceExists)
            {
                if (Serial.IsOpen)
                {
                    byte[] wasDequeued;
                    if (_ouputQueue.TryDequeue(out wasDequeued))
                    {
                        Serial.Write(wasDequeued, 0, PacketSize);
                        while (Serial.BytesToRead == 0 && _waitForSync) { }
                    }

                    var arduinoResponse = new byte[Serial.BytesToRead];
                    Serial.Read(arduinoResponse, 0, arduinoResponse.Length);
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
            Serial.Close();
            Serial.Dispose();
            Serial = null;
            _isSerialThreadRunning = false;
        }

        private void SyncCheck_Checked(object sender, RoutedEventArgs e)
        {
            _waitForSync = true;
            SyncCheck.IsChecked = true;
        }

        private void SyncCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            _waitForSync = false;
            SyncCheck.IsChecked = false;
        }

        public override void CleanUp()
        {
            _serialInterfaceExists = false;
            while (_isSerialThreadRunning) { }
        }


        public override void Save(XmlDocument xml, XmlNode parent)
        {
            var node = parent.AppendChild(xml.CreateElement(RackItemName + "-" + ItemName));
            SaveOutputs(xml, node);
            SaveInputs(xml, node);
        }

        public override void Load(XmlNode xml)
        {
            foreach (var node in xml.ChildNodes.OfType<XmlNode>())
            {
                switch (node.Name)
                {
                    case "Outputs":
                        LoadOutputs(node);
                        break;
                    case "Inputs":
                        LoadInputs(node);
                        break;
                }
            }
        }
    }
}

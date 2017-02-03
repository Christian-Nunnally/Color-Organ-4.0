using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using LabeledControlSideRailContainer = AudioSpectrum.Window.SideRailContainer.LabeledControlSideRailContainer;

namespace AudioSpectrum.RackItem
{
    public partial class SerialInterfaceItem : RackItemBase
    {
        private const int PacketSize = 204;

        private static bool _waitForSync;
        private static bool _serialInterfaceExists;
        private static bool _isSerialThreadRunning;
        private static string _currentComPort;

        private static readonly ConcurrentQueue<byte[]> OuputQueue = new ConcurrentQueue<byte[]>();
        private static readonly ConcurrentQueue<string> InputQueue = new ConcurrentQueue<string>();
        private static Thread _arduinoInterfaceBufferThread;

        private readonly ComboBox _comPortSelector = new ComboBox();
        private readonly Label _outgoingPacketCountLabel = new Label();
        private readonly CheckBox _syncCheckBox = new CheckBox();
        private readonly ListBox _serialInput = new ListBox();

        private List<UIElement> _sideRailControls;

        public SerialInterfaceItem(XmlNode xml)
        {
            InitializeComponent();
            ItemName = "SerialInterface";
            GetPorts();

            _syncCheckBox.IsChecked = true;
            _syncCheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            _syncCheckBox.Checked += SyncCheck_Checked;
            _syncCheckBox.Unchecked += SyncCheck_Unchecked;
            _comPortSelector.DropDownOpened += ComPortSelector_DropDownOpened;
            _outgoingPacketCountLabel.HorizontalAlignment = HorizontalAlignment.Right;
            _outgoingPacketCountLabel.Height = 22;
            _waitForSync = true;

            if (_comPortSelector.Items.Count > 0)
                _comPortSelector.SelectedIndex = 0;

            if (xml == null)
            {
                AddInput(new RackItemInput("Graphics Input1", GraphicsDataIn1));
                AddInput(new RackItemInput("Graphics Input2", GraphicsDataIn2));
            }
            else
                Load(xml);
        }


        private static SerialPort Serial { get; set; }

        public override void SetSideRail(SetSideRailDelegate sideRailSetter)
        {
            if (_sideRailControls == null)
                _sideRailControls = new List<UIElement>
                {
                    new LabeledControlSideRailContainer("Com Port", _comPortSelector, Orientation.Horizontal, 180),
                    new LabeledControlSideRailContainer("Sync", _syncCheckBox, Orientation.Horizontal, 180),
                    new LabeledControlSideRailContainer("Outgoing Packets:", _outgoingPacketCountLabel, Orientation.Horizontal, 50),
                    new LabeledControlSideRailContainer("Serial Input", _serialInput, Orientation.Vertical, 200),
                };
            sideRailSetter?.Invoke(ItemName, _sideRailControls);
        }

        private void ComPortSelector_DropDownOpened(object sender, EventArgs e)
        {
            GetPorts();
        }

        private void GetPorts()
        {
            _comPortSelector.Items.Clear();
            var ports = SerialPort.GetPortNames();
            foreach (var port in ports) _comPortSelector.Items.Add(port);
        }

        private void EnableButton_Click(object sender, RoutedEventArgs e)
        {
            if ((_comPortSelector.Items.Count <= 0) || (_comPortSelector.SelectedIndex == -1))
                return;
            _comPortSelector.IsEnabled = false;

            try
            {
                if ((EnableButton != null) && ((string)EnableButton.Content == "Enable Serial"))
                {
                    EnableButton.Content = "Disable Serial";
                    EnabledIndicator.Fill = new SolidColorBrush(Color.FromRgb(85, 255, 85));
                    _currentComPort = _comPortSelector.Items[_comPortSelector.SelectedIndex] as string;
                    Serial = new SerialPort(_currentComPort)
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
                    _syncCheckBox.IsEnabled = true;
                    _arduinoInterfaceBufferThread = new Thread(threadStart);
                    _arduinoInterfaceBufferThread.Start();
                }
                else
                {
                    if (EnableButton != null) EnableButton.Content = "Enable Serial";
                    EnabledIndicator.Fill = new SolidColorBrush(Color.FromRgb(255, 85, 85));
                    _comPortSelector.IsEnabled = true;
                    if (Serial == null) return;
                    _syncCheckBox.IsEnabled = false;
                    _serialInterfaceExists = false;
                    while (!_isSerialThreadRunning)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        public override IRackItem CreateRackItem(XmlElement xml)
        {
            return new SerialInterfaceItem(xml);
        }

        private void GraphicsDataIn1(List<byte> graphicsData)
        {
            if ((Serial == null) || !Serial.IsOpen) return;
            if (graphicsData.Count < 64 * 3) return;

            var header = new byte[12];
            header[0] = 40;
            header[1] = 30;
            header[2] = 20;
            header[3] = 10;
            header[4] = 0;

            OuputQueue.Enqueue(header.Concat(graphicsData).ToArray());

            _outgoingPacketCountLabel.Content = OuputQueue.Count.ToString();

            while (!InputQueue.IsEmpty)
            {
                if (_serialInput.Items.Count > 5) _serialInput.Items.RemoveAt(0);
                string result;
                if (InputQueue.TryDequeue(out result))
                {
                    _serialInput.Items.Add(result);
                }
            }
        }

        private void GraphicsDataIn2(List<byte> graphicsData)
        {
            if ((Serial == null) || !Serial.IsOpen) return;
            if (graphicsData.Count < 64 * 3) return;

            var header = new byte[12];
            header[0] = 40;
            header[1] = 30;
            header[2] = 20;
            header[3] = 10;
            header[4] = 1;

            OuputQueue.Enqueue(header.Concat(graphicsData).ToArray());

            _outgoingPacketCountLabel.Content = OuputQueue.Count.ToString();

            while (!InputQueue.IsEmpty)
            {
                if (_serialInput.Items.Count > 5) _serialInput.Items.RemoveAt(0);
                string result;
                if (InputQueue.TryDequeue(out result))
                {
                    _serialInput.Items.Add(result);
                }
            }
        }

        private static void RunArduinoSerialInterfaceBuffer()
        {
            _isSerialThreadRunning = true;
            while (_serialInterfaceExists)
                if (Serial.IsOpen)
                {
                    while ((Serial.BytesToRead == 0) && _waitForSync)
                    {
                    }

                    var arduinoResponse = new byte[Serial.BytesToRead];
                    Serial.Read(arduinoResponse, 0, arduinoResponse.Length);
                    if (arduinoResponse.Length == 1 && arduinoResponse[0] != 0) InputQueue.Enqueue(arduinoResponse[0].ToString());

                    byte[] wasDequeued;
                    if (OuputQueue.TryDequeue(out wasDequeued))
                    {
                        Serial.Write(wasDequeued, 0, wasDequeued.Length);
                    }
                }
                else
                {
                    Thread.Sleep(50);
                }
            Serial.Close();
            Serial.Dispose();
            Serial = null;
            _isSerialThreadRunning = false;
        }

        private void SyncCheck_Checked(object sender, RoutedEventArgs e)
        {
            _waitForSync = true;
            _syncCheckBox.IsChecked = true;
        }

        private void SyncCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            _waitForSync = false;
            _syncCheckBox.IsChecked = false;
        }

        public override void CleanUp()
        {
            _serialInterfaceExists = false;
            while (_isSerialThreadRunning)
            {

            }
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
            AttachPipeToInput(1, GraphicsDataIn1);
            AttachPipeToInput(2, GraphicsDataIn2);
        }
    }
}
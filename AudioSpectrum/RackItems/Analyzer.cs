using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace AudioSpectrum.RackItems
{
    public class Analyzer
    {
        public delegate void PipeOut(List<byte> data);
        
        private readonly DispatcherTimer _displayRefreshTimer;         //timer that refreshes the display
        private readonly float[] _fft;               //buffer for fft data
        private readonly WASAPIPROC _process;        //callback function to obtain data
        private int _lastlevel;             //last output level
        private int _hanctr;                //last output level counter
        private readonly List<byte> _spectrumdata;   //spectrum data buffer
        private readonly ComboBox _devicelist;       //device list
        private bool _initialized;          //initialized flag
        private int _deviceIndex;               //used device index

        private int _lines = 16;

        public int Lines
        {
            private get
            {
                return _lines;
            }
            set
            {
                if (value > 0 && value <= 32)
                {
                    _lines = value;
                }
            }
        }

        private PipeOut SpectrumPipeOut { get; }

        //ctor
        public Analyzer(PipeOut pipeOut, ComboBox devicelist)
        {
            _fft = new float[1024];
            _lastlevel = 0;
            _hanctr = 0;
            _displayRefreshTimer = new DispatcherTimer();
            _displayRefreshTimer.Tick += _t_Tick;
            _displayRefreshTimer.Interval = TimeSpan.FromMilliseconds(25); //40hz refresh rate
            _displayRefreshTimer.IsEnabled = false;
            _process = Process;
            _spectrumdata = new List<byte>();
            _devicelist = devicelist;
            _initialized = false;
            SpectrumPipeOut = pipeOut;
            Init();
        }

        //flag for enabling and disabling program functionality
        public bool Enable
        {
            set
            {
                if (value)
                {
                    if (!_initialized)
                    {
                        var s = _devicelist.Items[_devicelist.SelectedIndex] as string;
                        if (s != null)
                        {
                            var array = s.Split(' ');
                            _deviceIndex = Convert.ToInt32(array[0]);
                        }
                        var result = BassWasapi.BASS_WASAPI_Init(_deviceIndex, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero);
                        if (!result)
                        {
                            var error = Bass.BASS_ErrorGetCode();
                            MessageBox.Show(error.ToString());
                        }
                        else
                        {
                            _initialized = true;
                            _devicelist.IsEnabled = false;
                        }
                    }
                    BassWasapi.BASS_WASAPI_Start();
                }
                else BassWasapi.BASS_WASAPI_Stop(true);
                System.Threading.Thread.Sleep(500);
                _displayRefreshTimer.IsEnabled = value;
            }
        }

        // initialization
        private void Init()
        {
            for (var i = 0; i < BassWasapi.BASS_WASAPI_GetDeviceCount(); i++)
            {
                var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
                if (device.IsEnabled && device.IsLoopback)
                {
                    _devicelist.Items.Add($"{i} - {device.name}");
                }
            }
            _devicelist.SelectedIndex = 0;
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            var result = Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            if (!result) throw new Exception("Init Error");
        }

        //timer 
        private void _t_Tick(object sender, EventArgs e)
        {
            var ret = BassWasapi.BASS_WASAPI_GetData(_fft, (int)BASSData.BASS_DATA_FFT2048); //get channel fft data
            if (ret < -1) return;
            int x;
            var b0 = 0;

            //computes the spectrum data, the code is taken from a bass_wasapi sample.
            for (x=0; x<Lines; x++)
            {
                float peak = 0;
                var b1 = (int)Math.Pow(2, x * 10.0 / (Lines - 1));
                if (b1 > 1023) b1 = 1023;
                if (b1 <= b0) b1 = b0 + 1;
                for (;b0<b1;b0++)
                {
                    if (peak < _fft[1 + b0]) peak = _fft[1 + b0];
                }
                var y = (int)(Math.Sqrt(peak) * 3 * 255 - 4);
                if (y > 255) y = 255;
                if (y < 0) y = 0;
                _spectrumdata.Add((byte)y);
            }
            
            SpectrumPipeOut.Invoke(_spectrumdata);
            _spectrumdata.Clear();


            int level = BassWasapi.BASS_WASAPI_GetLevel();
            if (level == _lastlevel && level != 0) _hanctr++;
            _lastlevel = level;

            //Required, because some programs hang the output. If the output hangs for a 75ms
            //this piece of code re initializes the output so it doesn't make a gliched sound for long.
            if (_hanctr > 3)
            {
                _hanctr = 0;
                Free();
                Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                _initialized = false;
                Enable = true;
            }
        }

        // WASAPI callback, required for continuous recording
        private static int Process(IntPtr buffer, int length, IntPtr user)
        {
            return length;
        }

        //cleanup
        public static void Free()
        {
            BassWasapi.BASS_WASAPI_Free();
            Bass.FreeMe();
            if (!Bass.BASS_Free()) throw new InvalidOperationException("BASS was not freed");
        }
    }
}

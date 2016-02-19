using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WaveAnalysisProject
{
    //Class that contains win32 functions
    //used to record and play audio
    class Win32User 
    {
        //Initialization of constants
        public static int NOERROR = 0;
        public static int MM_WIM_DATA = 0x3c0;
        public static uint WAVE_MAPPER = 4294967295;
        public static int CALLBACK_FUNCTION = 0x0030000;
        public static int WAVE_FORMAT_PCM = 1;
        public static uint WHDR_BEGINLOOP = 0x00000004;
        public static uint WHDR_ENDLOOP = 0x00000008;

        //Struct for the Wave format
        [StructLayout(LayoutKind.Sequential)]
        public struct WAVEFORMAT
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nblockAlign;
            public ushort wbitspersample;
            public ushort cbsize;
        }

        //Struct for the Wave header
        [StructLayout(LayoutKind.Sequential)]
        public struct WAVEHDR
        {
            public IntPtr lpData;
            public uint dwbufferlength;
            public uint dwBytesRecorded;
            public IntPtr dwuser;
            public uint dwFlags;
            public uint dwLoops;
            public IntPtr lpnext;
            public IntPtr reserved;
        }

        public delegate void DelegateRec(IntPtr deviceHwnd, uint msg, IntPtr instance, ref WAVEHDR wavehdr, IntPtr reserved2);

        //win32 function imports
        [DllImport("winmm.dll")]
        public static extern int waveInAddBuffer(IntPtr hWaveIn, ref WAVEHDR lpWaveHdr, uint cWaveHdrSize);
        [DllImport("winmm.dll")]
        public static extern int waveInPrepareHeader(IntPtr hWaveIn, ref WAVEHDR lpWaveHdr, uint Size);
        [DllImport("winmm.dll")]
        public static extern int waveInStart(IntPtr hWaveIn);

        //Recording imports
        [DllImport("winmm.dll", EntryPoint = "waveInOpen", SetLastError = true)]
        public static extern int waveInOpen(ref IntPtr t, uint id, ref WAVEFORMAT pwfx, IntPtr dwCallback, int dwInstance, int fdwOpen);
        [DllImport("winmm.dll", EntryPoint = "waveInUnprepareHeader", SetLastError = true)]
        public static extern int waveInUnprepareHeader(IntPtr hwi, ref WAVEHDR pwh, uint cbwh);
        [DllImport("winmm.dll", EntryPoint = "waveInStop", SetLastError = true)]
        static extern uint waveInStop(IntPtr hwi);
        [DllImport("winmm.dll", EntryPoint = "waveInClose", SetLastError = true)]
        public static extern uint waveInClose(IntPtr hwnd);
        [DllImport("winmm.dll", EntryPoint = "waveInReset", SetLastError = true)]
        static extern uint waveInReset(IntPtr hwi);

        //Playing imports
        [DllImport("winmm.dll", EntryPoint = "waveOutOpen", SetLastError = true)]
        public static extern int waveOutOpen(ref IntPtr t, uint id, ref WAVEFORMAT pwfx, IntPtr dwCallback, int dwInstance, int fdwOpen);
        [DllImport("winmm.dll", EntryPoint = "waveOutPrepareHeader", SetLastError = true)]
        public static extern int waveOutPrepareHeader(IntPtr hWaveIn, ref WAVEHDR lpWaveHdr, uint Size);
        [DllImport("winmm.dll", EntryPoint = "waveOutWrite", SetLastError = true)]
        public static extern int waveOutWrite(IntPtr hWaveIn, ref WAVEHDR lpWaveHdr, uint Size);
        [DllImport("winmm.dll", EntryPoint = "waveOutUnprepareHeader", SetLastError = true)]
        public static extern int waveOutUnprepareHeader(IntPtr hwi, ref WAVEHDR pwh, uint cbwh);
        [DllImport("winmm.dll", EntryPoint = "waveOutClose", SetLastError = true)]
        public static extern uint waveOutClose(IntPtr hwnd);
        [DllImport("winmm.dll", EntryPoint = "waveOutStart", SetLastError = true)]
        public static extern int waveOutStart(IntPtr hWaveIn);
        [DllImport("winmm.dll", EntryPoint = "waveOutStop", SetLastError = true)]
        static extern uint waveOutStop(IntPtr hwi);
        [DllImport("winmm.dll", EntryPoint = "waveOutReset", SetLastError = true)]
        static extern uint waveOutReset(IntPtr hwi);
        [DllImport("winmm.dll", EntryPoint = "waveOutPause", SetLastError = true)]
        static extern uint waveOutPause(IntPtr hwi);

        //Declaration of variables
        private Win32User.DelegateRec waveIn;
        private IntPtr handle;
        private IntPtr hWaveOut;
        private uint bufferLength;
        private WAVEHDR header;
        private WAVEHDR outheader;
        private GCHandle headerPin;
        private GCHandle bufferPin;
        private GCHandle savePin;
        private byte[] buffer;
        private byte[] save;

        //Begin recording 
        public void record()
        {
            handle = new IntPtr();
            waveIn = callbackWaveIn;
            WAVEFORMAT format;
            format.wFormatTag = (ushort)WAVE_FORMAT_PCM;
            format.nChannels = 1;
            format.nSamplesPerSec = 11025;
            format.wbitspersample = 16;
            format.nblockAlign = 2;
            format.nAvgBytesPerSec = format.nSamplesPerSec * format.nblockAlign;
            bufferLength = 22050;
            buffer = new byte[bufferLength];
            save = null;
            bufferPin = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            format.cbsize = 0;

            int i = waveInOpen(ref handle, WAVE_MAPPER, ref format, Marshal.GetFunctionPointerForDelegate(waveIn),
                                        0, CALLBACK_FUNCTION);
            //Error occured with waveInOpen
            if (i != NOERROR)
            {
                return;
            }

            initBuffer();
            i = waveInStart(handle);
            //Error occured with waveInStart
            if (i != NOERROR)
            {
                return;
            }
        }

        //Initialize buffer
        private void initBuffer()
        {
            header.lpData = bufferPin.AddrOfPinnedObject();
            header.dwbufferlength = bufferLength;
            header.dwFlags = 0;
            header.dwBytesRecorded = 0;
            header.dwLoops = 0;
            header.dwuser = IntPtr.Zero;
            header.lpnext = IntPtr.Zero;
            header.reserved = IntPtr.Zero;
            headerPin = GCHandle.Alloc(header, GCHandleType.Pinned);

            int i = waveInPrepareHeader(this.handle, ref header, Convert.ToUInt32(Marshal.SizeOf(header)));
            //Error occured with waveInPrepareHeader
            if (i != NOERROR)
            {
                return;
            }

            i = waveInAddBuffer(handle, ref header, Convert.ToUInt32(Marshal.SizeOf(header)));
            //Error occured with waveInAddBuffer
            if (i != NOERROR)
            {
                return;
            }
        }

        //stop recording
        public byte[] stopRec()
        {
            waveInStop(handle);
            waveInClose(handle);

            if (save == null)
            {
                return null;
            }
            bufferPin.Free();
            savePin.Free();

            return save;
        }

        //When the small buffer is full, put it in the save buffer
        private void callbackWaveIn(IntPtr deviceHandle, uint message, IntPtr instance, ref WAVEHDR wavehdr, IntPtr reserved2)
        {
            if (message == MM_WIM_DATA)
            {
                if (save != null)
                {
                    List<byte> temp = save.ToList();
                    temp.AddRange(buffer.ToList()); //add to existing buffer
                    save = temp.ToArray();
                }
                else
                {
                    save = buffer;  //first time, assign the buffer
                }

                savePin = GCHandle.Alloc(save, GCHandleType.Pinned);
                buffer = new byte[bufferLength];
                bufferPin = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                int i = waveInUnprepareHeader(deviceHandle, ref header, Convert.ToUInt32(Marshal.SizeOf(header)));
                //Error with waveInUprepareHeader
                if (i != NOERROR)
                {
                    return;
                }

                initBuffer();
            }
        }

        //Play the specified wave object
        public void play(Wave wav)
        {
            save = wav.data;
            hWaveOut = new IntPtr();
            waveIn = callbackWaveOut;
            WAVEFORMAT format;
            format.wFormatTag = (ushort)WAVE_FORMAT_PCM;
            format.nChannels = wav.channels;
            format.nSamplesPerSec = wav.sampleRate;
            format.wbitspersample = wav.bitDepth;
            format.nblockAlign = Convert.ToUInt16(format.nChannels * (format.wbitspersample >> 3));
            format.nAvgBytesPerSec = format.nSamplesPerSec * format.nblockAlign;
            savePin = GCHandle.Alloc(save, GCHandleType.Pinned);
            format.cbsize = 0;

            int i = waveOutOpen(ref hWaveOut, WAVE_MAPPER, ref format, Marshal.GetFunctionPointerForDelegate(waveIn),
                                        0, CALLBACK_FUNCTION);

            //Error occured with waveOutOpen
            if (i != NOERROR)
            {
                return;
            }

            initOutbuffer();
        }

        public void pausePlay()
        {
            waveOutPause(hWaveOut);
        }

        //Initialize the OutBuffer
        private void initOutbuffer()
        {
            outheader.lpData = savePin.AddrOfPinnedObject();
            outheader.dwbufferlength = (uint)save.Length;
            outheader.dwFlags = WHDR_BEGINLOOP | WHDR_ENDLOOP;
            outheader.dwBytesRecorded = 0;
            outheader.dwLoops = 1;
            outheader.lpnext = IntPtr.Zero;
            outheader.reserved = IntPtr.Zero;

            int i = waveOutPrepareHeader(hWaveOut, ref outheader, Convert.ToUInt32(Marshal.SizeOf(outheader)));
            //Error occured with waveOutPrepareHeader
            if (i != NOERROR)
            {
                return;
            }

            i = waveOutWrite(hWaveOut, ref outheader, Convert.ToUInt32(Marshal.SizeOf(outheader)));
            //Error occured with waveOutWrite
            if (i != NOERROR)
            {
                return;
            }
        }

        //Handle messages when playing signal
        private void callbackWaveOut(IntPtr deviceHandle, uint message, IntPtr instance, ref WAVEHDR wavehdr, IntPtr reserved2)
        {
            if (message == MM_WIM_DATA)
            {
                int i = waveInUnprepareHeader(deviceHandle, ref header, Convert.ToUInt32(Marshal.SizeOf(header)));
                //Error with waveInUnprepareHeader
                if (i != NOERROR)
                {
                    return;
                }
                initOutbuffer();
            }
        }

    }
}

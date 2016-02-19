using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveAnalysisProject
{
    //Class that stores the wave file information
    class Wave
    {
        //Default constructor
        public Wave() { }

        //Constructor when recording
        public Wave(byte[] signalData)
        {
            chunkID = System.Text.Encoding.ASCII.GetBytes("RIFF");
            fileSize = 36 + (uint)signalData.Length;
            riffType = System.Text.Encoding.ASCII.GetBytes("WAVE");
            fmtID = System.Text.Encoding.ASCII.GetBytes("fmt ");
            fmtSize = 16;
            fmtCode = 1;
            channels = 1;
            sampleRate = 11025;
            fmtAvgBPS = 22050;
            fmtBlockAlign = 2;
            bitDepth = 16;
            dataID = System.Text.Encoding.ASCII.GetBytes("data");
            dataSize = (uint)signalData.Length;

            data = signalData;
        }

        public byte[] chunkID;
        public uint fileSize;
        public byte[] riffType;
        public byte[] fmtID;
        public uint fmtSize;
        public ushort fmtCode;
        public ushort channels;
        public uint sampleRate;
        public uint fmtAvgBPS;
        public ushort fmtBlockAlign;
        public ushort bitDepth;
        public byte[] dataID;
        public uint dataSize;

        public byte[] data;
    }
}

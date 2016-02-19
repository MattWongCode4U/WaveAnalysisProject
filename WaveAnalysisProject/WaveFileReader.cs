using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;

namespace WaveAnalysisProject
{
    //Helper class
    //Used to read a wave file
    class WaveFileReader
    {
        //Read in a wave file from a stream and store the header information
        //into a Wave object and the data in a byte array
        public Byte[] Read(Stream stream, out Wave wav)
        {
            Wave WaveFile = new Wave();
            BinaryReader br = new BinaryReader(stream);
            WaveFile.chunkID = br.ReadBytes(4);
            WaveFile.fileSize = br.ReadUInt32();
            WaveFile.riffType = br.ReadBytes(4);
            WaveFile.fmtID = br.ReadBytes(4);
            WaveFile.fmtSize = br.ReadUInt32();
            WaveFile.fmtCode = br.ReadUInt16();
            WaveFile.channels = br.ReadUInt16();
            WaveFile.sampleRate = br.ReadUInt32();
            WaveFile.fmtAvgBPS = br.ReadUInt32();
            WaveFile.fmtBlockAlign = br.ReadUInt16();
            WaveFile.bitDepth = br.ReadUInt16();
            WaveFile.dataID = br.ReadBytes(4);
            WaveFile.dataSize = br.ReadUInt32();

            if(WaveFile.fmtSize != 16)
            {
                wav = null;
                return null;
            }

            WaveFile.data = br.ReadBytes((int)WaveFile.dataSize);

            wav = WaveFile;
            stream.Close();
            br.Close();
            return WaveFile.data;
        }
    }
}

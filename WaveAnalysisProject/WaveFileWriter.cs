using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WaveAnalysisProject
{
    //Helper class
    //Used to write a WAVE file
    class WaveFileWriter
    {
        //Write a WAVE file to the specified stream using
        //the information provided by the Wave object
        public void Write(Stream stream, Wave wav)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(wav.chunkID);
            bw.Write(wav.fileSize);
            bw.Write(wav.riffType);
            bw.Write(wav.fmtID);
            bw.Write(wav.fmtSize);
            bw.Write(wav.fmtCode);
            bw.Write(wav.channels);
            bw.Write(wav.sampleRate);
            bw.Write(wav.fmtAvgBPS);
            bw.Write(wav.fmtBlockAlign);
            bw.Write(wav.bitDepth);
            bw.Write(wav.dataID);
            bw.Write(wav.dataSize);
            bw.Write(wav.data);

            bw.Close();
            stream.Close();
        }
    }
}

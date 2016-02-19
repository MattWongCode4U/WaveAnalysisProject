using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms;

namespace WaveAnalysisProject
{
    //Helper class
    //Used for functions found in the Edit tab
    class Editor
    {   
        //Copy elements from selection onto the clipboard
        public void copy(int selStart, int selEnd, Wave wav)
        {
            List<byte> lst = new List<byte>();

            //click on graph
            if(selStart == selEnd)
            {
                return;
            }

            //due to conversion and drawing
            selStart *= 10; 
            selEnd *= 10;

            //left to right
            if(selStart < selEnd)
            {
                for (int i = selStart; i < selEnd; i++)
                {
                    lst.Add(wav.data[i]);
                }
                Clipboard.SetDataObject(lst.ToArray());
            }

            //right to left
            if(selStart > selEnd)
            {
                for (int i = selEnd; i < selStart; i++)
                {
                    lst.Add(wav.data[i]);
                }
                //put data on clipboard
                Clipboard.SetDataObject(lst.ToArray());
            } 
        }

        //Cut data to clipboard
        //Clear selected portion of the signal selected 
        public void cut(int selStart, int selEnd, ref Wave wav)
        {
            List<byte> lst = new List<byte>();

            //click on graph
            if (selStart == selEnd)
            {
                return;
            }

            //call to copy function
            copy(selStart, selEnd, wav);

            //due to conversion and drawing
            selStart *= 10;
            selEnd *= 10;

            //left of selection
            for (int i = 0; i < selStart; i++)
            {
                lst.Add(wav.data[i]);
            }
            //right of selection
            for(int i = selEnd; i < wav.data.Length; i++)
            {
                lst.Add(wav.data[i]);
            }

            //array of original data with selection cut out
            wav.data = lst.ToArray();
        }

        //Paste data from clipboard
        /** 
            Get elements from clipboard
            put those elements onto the graph and array
        */
        public void paste(int selStart, int selEnd, ref Wave wav)
        {
            //get data from clipboard
            IDataObject retrievedObj = Clipboard.GetDataObject();
            if (retrievedObj.GetDataPresent(typeof(byte[])))
            {
                byte[] data = (byte[])retrievedObj.GetData(typeof(byte[]));
                //click on chart
                if (selStart == selEnd)
                {
                    //reassigning the wave data array
                    List<byte> tmp = new List<byte>();
                    
                    int i, j, k;
                    //left of selection
                    for(i = 0, j = 0, k = 0; i < selStart; i++, j++)
                    {
                        tmp.Add(wav.data[j]);
                    }
                    //selection
                    for(i = selStart; i < selEnd + data.Length; i++, k++)
                    {
                        tmp.Add(data[k]);
                    }
                    //right of selection
                    for(i = selEnd + data.Length; i < wav.data.Length + data.Length; i++, j++)
                    {
                        tmp.Add(wav.data[j]);
                    }
                    //put data into wav object
                    wav.data = tmp.ToArray();
                }
            }
        }
    }
}

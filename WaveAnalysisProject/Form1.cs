using System;
using System.Numerics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Threading;

namespace WaveAnalysisProject
{
    //Form used for wave analysis
    public partial class Form1 : Form
    {
        //Declarations of helper objects
        Algorithm algo;
        Wave wav;
        Editor editor;
        Filter filt;
        Win32User win32;

        //Constructor for the form
        public Form1()
        {
            InitializeComponent();
            algo = new Algorithm();
            wav = new Wave();
            editor = new Editor();
            filt = new Filter();
            win32 = new Win32User();
            CheckForIllegalCrossThreadCalls = false;

            //GUI setup
            selectionModeToolStripMenuItem.Checked = true;
            zoomModeToolStripMenuItem.Checked = false;
            timeDomainChart.Cursor = Cursors.IBeam;
            timeDomainChart.ChartAreas[0].CursorX.IsUserSelectionEnabled = false;
            timeDomainChart.ChartAreas[0].CursorX.IsUserEnabled = false;
            chart5.ChartAreas[0].CursorX.IsUserSelectionEnabled = false;
            chart5.ChartAreas[0].CursorX.IsUserEnabled = false;

            rectangleToolStripMenuItem1.Checked = true;

            RecordButton.Enabled = true;
            StopRecordButton.Enabled = false;
            PlayButton.Enabled = false;
            StopPlayButton.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        //Quit Button in the menu  File -> Quit
        //Closes the window
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //Open Button in the menu  File -> Open
        //Opens a .wav file then reads the file and displays the signal
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Select .wav file to analyze";
            openDialog.InitialDirectory = @"c:\";
            openDialog.Filter = "Wav Files (*.wav)|*.wav";
            openDialog.RestoreDirectory = true;
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                WaveFileReader reader = new WaveFileReader();
                FileStream file = new FileStream(openDialog.FileName, FileMode.Open);
                reader.Read(file, out wav);

                draw();

                //GUI setup
                chart5.ChartAreas[0].CursorX.IsUserSelectionEnabled = false;
                chart5.ChartAreas[0].CursorX.IsUserEnabled = false;
                chart5.ChartAreas[0].CursorX.SetSelectionPosition(-1, -1);

                RecordButton.Enabled = true;
                StopRecordButton.Enabled = false;
                PlayButton.Enabled = true;
                StopPlayButton.Enabled = false;
            }
        }

        //Save Button in the menu  File -> Save
        //Saves the signal as a .wav file
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Save a File";
            saveDialog.InitialDirectory = @"c:\";
            saveDialog.Filter = "Wav File (*.wav)|*.wav";
            saveDialog.RestoreDirectory = true;
            if (saveDialog.ShowDialog() == DialogResult.OK) {

                WaveFileWriter writer = new WaveFileWriter();
                FileStream stream = new FileStream(saveDialog.FileName, FileMode.Create);
                writer.Write(stream, wav);
            }
        }

        //Copy Button in the menu   Edit -> Copy
        //Copies a selection of the signal and
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start = (int)timeDomainChart.ChartAreas[0].CursorX.SelectionStart;
            int end = (int)timeDomainChart.ChartAreas[0].CursorX.SelectionEnd;
            
            //error checking
            if (start < 0 || end < 0 || start == end)
            {
                return;
            }

            setEven(ref start, ref end);
            editor.copy(start, end, wav);
        }

        //Cut Button in the menu    Edit -> Cut
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start = (int)timeDomainChart.ChartAreas[0].CursorX.SelectionStart;
            int end = (int)timeDomainChart.ChartAreas[0].CursorX.SelectionEnd;

            //error checking
            if (start < 0 || end < 0 || start == end)
            {
                return;
            }

            setEven(ref start, ref end);
            editor.cut(start, end, ref wav);
            double[] value = new double[wav.data.Length/2 ];

            draw();
        }

        //Paste Button in the menu  Edit -> Paste
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start = (int)timeDomainChart.ChartAreas[0].CursorX.SelectionStart;
            int end = (int)timeDomainChart.ChartAreas[0].CursorX.SelectionEnd;

            //error checking
            if (start < 0 || end < 0)
            {
                return;
            }

            setEven(ref start, ref end);
            editor.paste(start, end, ref wav);
            draw();
        }

        //Change to selection mode  View -> Selection Mode
        private void selectionModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(sender == selectionModeToolStripMenuItem)
            {
                setSelectionMode();
            }
        }

        //Change to zoom mode   View -> Zoom Mode
        private void zoomModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender == zoomModeToolStripMenuItem)
            {
                setZoomMode();
            }
        }

        //Converts two bytes into one double using a bit shift
        public double byteToDouble(byte first, byte second)
        {
            short s = (short)((second << 8) | first);
            return s / 32768.0;
        }

        //Performs DFT on a selection depending on which window is selected
        //Analysis -> DFT
        private void dFTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int selStart = (int)timeDomainChart.ChartAreas[0].CursorX.SelectionStart;
            int selEnd = (int)timeDomainChart.ChartAreas[0].CursorX.SelectionEnd;
            if(selStart < 0 || selEnd < 0 || selStart == selEnd)
            {
                return;
            }
            chart5.Series["Read Frequency"].Points.Clear();
            byte[] data;
            double[] stored;
            double[] AmpValues;
            int N;
            if (selStart < selEnd) {
                setEven(ref selStart, ref selEnd);
                N = selEnd - selStart;
                data = new byte[N];
                Array.Copy(wav.data, selStart, data, 0, N - 1);
                stored = new double[N/2];
                AmpValues = new double[N];
            }
            else
            {
                setEven(ref selStart, ref selEnd);
                N = selStart - selEnd;
                data = new byte[N];
                Array.Copy(wav.data, selEnd, data, 0, N - 1);
                stored = new double[N/2];
                AmpValues = new double[N];
            }

            chart5.Series["Read Frequency"].Points.Clear();

            for (int i = 0, index = 0; i < data.Length - 2; i++, index++)
            {
                stored[index] = byteToDouble(data[i], data[++i]);
            }

            if (rectangleToolStripMenuItem1.Checked == true) {
                AmpValues = algo.performDFT(stored);
            } else if(triangleToolStripMenuItem1.Checked == true){
                AmpValues = algo.performDFT(algo.TriangleWindow(stored));
            } else if(welchToolStripMenuItem1.Checked == true){
                AmpValues = algo.performDFT(algo.WelchWindow(stored));
            } else if(hanningToolStripMenuItem1.Checked == true){
                AmpValues = algo.performDFT(algo.HanningWindow(stored));
            }

            for (int t = 0; t < AmpValues.Length; t++)
            {
                chart5.Series["Read Frequency"].Points.Add(AmpValues[t]);
            }

            chart5.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart5.ChartAreas[0].CursorX.IsUserEnabled = true;
        }

        //Perform a low pass filter based on selection
        //Filter -> Low Pass
        private void lowPassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int selStart = (int)chart5.ChartAreas[0].CursorX.SelectionStart;
            int selEnd = (int)chart5.ChartAreas[0].CursorX.SelectionEnd;
            double[] data = new double[wav.data.Length / 2];
            for (int i = 0, index = 0; i < wav.data.Length - 2; i++, index++)
            {
                data[index] = byteToDouble(wav.data[i], wav.data[++i]);
            }
            Complex[] fil = filt.createLowPassFilter(selStart, selEnd, data);
            
            double[] newSamples = algo.Convolution(fil, data);
            List<byte> lst = new List<byte>();

            timeDomainChart.Series["Read Signal"].Points.Clear();
            for(int i = 0; i < newSamples.Length; i++)
            {
                short s = (short)(newSamples[i] * 32768);
                byte[] arr = { (byte)(s), (byte)(s >> 8) };
                lst.AddRange(arr.ToList());
            }
            for(int i = 0; i < newSamples.Length; i+=5)
            {
                timeDomainChart.Series["Read Signal"].Points.Add(newSamples[i]);
            }
            wav.data = lst.ToArray();
        }

        //Allows the signal to be selected using the mouse
        private void setSelectionMode()
        {
            timeDomainChart.ChartAreas[0].CursorX.IsUserEnabled = false;
            timeDomainChart.ChartAreas[0].CursorX.SelectionStart = -1;
            timeDomainChart.ChartAreas[0].CursorX.SelectionEnd = -1;
            timeDomainChart.ChartAreas[0].CursorX.Position = -1;
            selectionModeToolStripMenuItem.Checked = true;
            zoomModeToolStripMenuItem.Checked = false;
            timeDomainChart.Cursor = Cursors.IBeam;
            timeDomainChart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            timeDomainChart.ChartAreas[0].CursorX.IsUserEnabled = true;
            timeDomainChart.ChartAreas[0].Axes[0].ScaleView.Zoomable = false;

            chart5.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart5.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart5.ChartAreas[0].Axes[0].ScaleView.Zoomable = false;
        }

        //Allows the signal to be zoomed in on
        private void setZoomMode()
        {
            timeDomainChart.ChartAreas[0].CursorX.IsUserEnabled = false;
            timeDomainChart.ChartAreas[0].CursorX.SelectionStart = -1;
            timeDomainChart.ChartAreas[0].CursorX.SelectionEnd = -1;
            timeDomainChart.ChartAreas[0].CursorX.Position = -1;
            selectionModeToolStripMenuItem.Checked = false;
            zoomModeToolStripMenuItem.Checked = true;
            timeDomainChart.Cursor = Cursors.VSplit;
            timeDomainChart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            timeDomainChart.ChartAreas[0].CursorX.IsUserEnabled = false;
            timeDomainChart.ChartAreas[0].Axes[0].ScaleView.Zoomable = true;

            chart5.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart5.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart5.ChartAreas[0].Axes[0].ScaleView.Zoomable = true;
        }

        //Sets the Windowing to Rectangle (default)
        //Windowing -> Rectangle
        private void rectangleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            rectangleToolStripMenuItem1.Checked = true;
            triangleToolStripMenuItem1.Checked = false;
            welchToolStripMenuItem1.Checked = false;
            hanningToolStripMenuItem1.Checked = false;
        }

        //Sets the Windowing to Triangle    
        //Windowing -> Triangle
        private void triangleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            rectangleToolStripMenuItem1.Checked = false;
            triangleToolStripMenuItem1.Checked = true;
            welchToolStripMenuItem1.Checked = false;
            hanningToolStripMenuItem1.Checked = false;
        }

        //Sets the Windowing to Welch
        //Windowing -> Welch
        private void welchToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            rectangleToolStripMenuItem1.Checked = false;
            triangleToolStripMenuItem1.Checked = false;
            welchToolStripMenuItem1.Checked = true;
            hanningToolStripMenuItem1.Checked = false;
        }

        //Sets the Windowing to Hanning
        //Windowing -> Hanning
        private void hanningToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            rectangleToolStripMenuItem1.Checked = false;
            triangleToolStripMenuItem1.Checked = false;
            welchToolStripMenuItem1.Checked = false;
            hanningToolStripMenuItem1.Checked = true;
        }

        //Help menu displayed
        private void howToUseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help form = new Help();
            form.MdiParent = ParentForm;
            form.Show();
        }

        //Fixes an issue with conversion from bytes to double
        private void setEven(ref int start, ref int end)
        {
            if(start < end)
            {
                if(start % 2 != 0)
                {
                    start += 1;
                }
            } else
            {
                if(end % 2 != 0)
                {
                    end++;
                }
            }
        }

        //Record Button
        private void RecordButton_Click(object sender, EventArgs e)
        {
            RecordButton.Enabled = false;
            StopRecordButton.Enabled = true;
            PlayButton.Enabled = false;
            StopPlayButton.Enabled = false;

            timeDomainChart.Series["Read Signal"].Points.Clear();
            chart5.Series["Read Frequency"].Points.Clear();

            win32.record();
        }

        //Stop Record Button
        private void StopRecordButton_Click(object sender, EventArgs e)
        {
            RecordButton.Enabled = true;
            StopRecordButton.Enabled = false;
            PlayButton.Enabled = true;
            StopPlayButton.Enabled = false;

            byte[] temp = win32.stopRec();
            //Error occured during recording
            if (temp == null)
            {
                return;
            }
            //assign new data to wave object
            wav = new Wave(temp);

            draw();
        }

        //Play Button
        private void PlayButton_Click(object sender, EventArgs e)
        {
            RecordButton.Enabled = false;
            StopRecordButton.Enabled = false;
            PlayButton.Enabled = false;
            StopPlayButton.Enabled = true;

            win32.play(wav);
        }

        //Stop Play Button (Not being used right now)
        private void StopPlayButton_Click(object sender, EventArgs e)
        {
            RecordButton.Enabled = true;
            StopRecordButton.Enabled = false;
            PlayButton.Enabled = true;
            StopPlayButton.Enabled = false;

            win32.pausePlay();
        }

        public void draw()
        {
            double[] stored = new double[wav.data.Length / 2];

            timeDomainChart.Series["Read Signal"].Points.Clear();
            chart5.Series["Read Frequency"].Points.Clear();

            for (int i = 0, index = 0; i < wav.data.Length - 2; i++, index++)
            {
                stored[index] = byteToDouble(wav.data[i], wav.data[++i]);
            }

            for (int i = 0; i < stored.Length; i += 5)
            {
                timeDomainChart.Series["Read Signal"].Points.Add(stored[i]);
            }
            setSelectionMode();
        }
    }
}

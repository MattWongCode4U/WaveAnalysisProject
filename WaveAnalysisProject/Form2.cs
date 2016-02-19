using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WaveAnalysisProject
{
    //Parent form that contains analysis forms
    public partial class Form2 : Form
    {
        //form constructor
        public Form2()
        {
            InitializeComponent();
        }

        //Create new analysis window
        //File -> New
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1 form = new Form1();
            form.MdiParent = this;
            form.Show();
        }

        //Close this window
        //File -> Quit
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Danfoss
{
    public partial class SampleSN : Form
    {
        public SampleSN()
        {
            InitializeComponent();
        }
        public delegate void TransfDelegate(String value);
        public delegate void TransintfDelegate(int valuet);
        public event TransfDelegate TransfEvent;
        public event TransintfDelegate TransintfEvent;

        private void SampleSN_Load(object sender, EventArgs e)
        {
           samplesninput.Focus();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            string samples;
            int slength;
            samples = samplesninput.Text;
            slength = samples.Length;
            //  MessageBox.Show(slength.ToString());
            TransfEvent(samples);
            TransintfEvent(slength);
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            TransintfEvent(0);
            this.Close();
        }

        private void samplesninput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

                this.OK_Click(sender, e);
            }
        }


    }
}

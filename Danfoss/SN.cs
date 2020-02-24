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
    public partial class SN : Form
    {
        public delegate void TransfDelegate(String value);
        public delegate void TransintfDelegate(int valuet);
        public event TransfDelegate TransfEvent;
        public event TransintfDelegate TransintfEvent;
      //  public event TransintfDelegate TransintfEvent2;
        public SN()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            int length;
            length = sninput.Text.Length;
            TransfEvent(sninput.Text);
            TransintfEvent(length);
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            TransintfEvent(0);
            this.Close();
        }

        private void sninput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

               
                e.Handled = true;   //将Handled设置为true，指示已经处理过KeyPress事件
                OK.PerformClick();////执行单击confirm1的动作

            }
        }
    }
}

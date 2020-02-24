using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO.Ports;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Danfoss
{
    public partial class Form1 : Form
    {
        delegate void mydelegate();
        public Form1()
        {
            InitializeComponent();
        }
        string ff, ff1, Save_Data,keithleyadd,snno="0000",pow1add="0",pow2add="0",NC1add,NO2add,NC3add,NO4add,LEDadd,DCRADD,CanEnable="0",CoilNumber, snsample = "", rev, pcbrev, pdays, smodel,savemodel,path2,savepath;
        SerialPort sp,power1sp, power2sp;
        FileStream savefile;
        StreamWriter sw3;
        int[] Seq;
        string savedata, canmodel = "";
        int samplelength,teston=1,snlength,Item=0,pass=1,pass_item=1,stop=0,u=0,open=0,signal=0,ofile=0,actframe=0;
        bool flag = true,filenew,pause=false;
        // ManualResetEvent manualEvent;// = new ManualResetEvent(true);//为trur,一开始就可以执行
        public Thread thread1,thread2;
        [DllImport(@"D:\nidaqdll.dll", EntryPoint = "CreatTask", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 CreatTask();
        [DllImport(@"D:\nidaqdll.dll", EntryPoint = "GetErrInfo", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr GetErr(Int32 code);
        [DllImport(@"D:\nidaqdll.dll", EntryPoint = "ReadDCVol", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern Int32 GetDCVol(IntPtr taskhandel, Int32 numSampsPerChan, double timeout, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] double[] readArray, out Int32 sampsPerChanRead);
        [DllImport(@"D:\nidaqdll.dll", EntryPoint = "Writeport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern Int32 Writeport(IntPtr taskhandel, Int32 numSampsPerChan,UInt32 autostart, double timeout, UInt32 data, out Int32 sampsPerChanWrite);

        [DllImport(@"D:\nidaqdll.dll", EntryPoint = "ConfigChann", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern Int32 ConfigChann(IntPtr taskhandel, [MarshalAs(UnmanagedType.LPStr)]string param1, [MarshalAs(UnmanagedType.LPStr)]string param2, Int32 terminalConfig, double minVal, double maxVal, Int32 units);
        [DllImport(@"D:\nidaqdll.dll", EntryPoint = "ConfigDOChann", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern Int32 ConfigDOChann(IntPtr taskhandel, [MarshalAs(UnmanagedType.LPStr)]string param1, [MarshalAs(UnmanagedType.LPStr)]string param2);

        private void button3_Click(object sender, EventArgs e)
        {
            int i, x;
            signal = 1;
            i = dataGridView1.Rows.Count;//所有行数
            for (x = 0; x < i; x++)
            {
                this.dataGridView1.Rows[x].DefaultCellStyle.BackColor = Color.White;
                for (int ll = 0; ll < 7; ll++)
                { dataGridView1.Rows[x].Cells[ll + 2].Value = ""; }
            }
            listBox1.Items.Clear();
            Statue.Text = "Testing"; this.Statue.ForeColor = Color.Blue;// 颜色 
            if (MODEL.Text.IndexOf("-90") == -1) { outport("Dev2/port0", 0x1A); }
            if (MODEL.Text.IndexOf("-90") != -1) { outport("Dev2/port0", 0x1A); Delay(2000); outport("Dev2/port0", 0x18); }
            thread2 = new Thread(Thread_STest);
            //   thread1 = new Thread(new ThreadStart(Thread_Test));              
                 thread2.IsBackground = true;
            thread2.Start();

        }

        [DllImport(@"D:\nidaqdll.dll", EntryPoint = "ConfigSampleClk", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern Int32 ConfigSampleClk(IntPtr taskhandel, double rate, Int32 activeEdge, Int32 sampleMode, UInt64 sampsPerChanToAcquire);
        [DllImport(@"D:\nidaqdll.dll", EntryPoint = "StartTask", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 StartTask(IntPtr taskhandel);

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] sendData;
            string errinfo;
            sendData = null;
            sendData = Encoding.UTF8.GetBytes("VSET1:0");
            power1sp.Write(sendData, 0, sendData.Length); ; Delay(10); outport("Dev2/port0", 0x1F);
            errinfo = sendcomm(sp, "ffaa030800000000b4");
            if (errinfo != "")
            {
                ErrBox.Items.Add("StepMotor Com Err:" + errinfo);
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {


                e.Handled = true;   //将Handled设置为true，指示已经处理过KeyPress事件
                Test.PerformClick();////执行单击confirm1的动作

            }
        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            Test.Focus();
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                SampleSN sminput = new SampleSN();
                Regex regnum = new Regex("^[0~9]");
                sminput.TransfEvent += frm_TransfEvent;
                sminput.TransintfEvent += frm_TransintfEvent;
                DialogResult ddr = sminput.ShowDialog();

                int sindex;
                string revx, revr;
                if (samplelength != 0)
                { Test.Enabled = true; }
                revr = "";
                sindex = snsample.IndexOf(":");
                smodel = snsample.Substring(0, sindex);
                //  MessageBox.Show(smodel);
                revx = getrev(snsample, revr);
                //  MessageBox.Show(rev);  

                if (regnum.IsMatch(revx.Substring(0, 1)))
                {
                    // MessageBox.Show("Num");
                    rev = revx.Substring(0, 2);
                    pdays = revx.Substring(2, 4);
                    // MessageBox.Show(rev);
                }
                else
                {
                    rev = revx.Substring(0, 1);
                    pdays = revx.Substring(1, 4);
                    // MessageBox.Show(rev);
                }
                revr = "";
                pcbrev = getpcbrev(snsample, revr);
            }
            Test.Focus();

        }

        private void Form1_Click(object sender, EventArgs e)
        {
            Test.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Pause")
            { pause = true; Statue.Text = "Pause"; button1.Text = "Continue"; }
            else
            { pause = false; Statue.Text = "Testing"; button1.Text = "Pause"; }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] sendData;
            string errinfo="";
            if (signal == 0) { flag = false; pause = false; stop = 1; Statue.Text = "STOP"; }
            else {
                flag = false; pause = false; stop = 1; Statue.Text = "STOP";
                sendData = null;
                sendData = Encoding.UTF8.GetBytes("VSET1:0");
                power1sp.Write(sendData, 0, sendData.Length); ; Delay(10); outport("Dev2/port0", 0x1F);
                errinfo = sendcomm(sp, "ffaa030800000000b4");
                if (errinfo != "") { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
            }
        }

        [DllImport(@"D:\nidaqdll.dll", EntryPoint = "StopTask", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 StopTask(IntPtr taskhandel);
        [DllImport(@"D:\nidaqdll.dll", EntryPoint = "ClearTask", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 ClearTask(IntPtr taskhandel);
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);
        [DllImport(@"D:\GPIBDLL.dll", EntryPoint = "Gpread", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr gpread([MarshalAs(UnmanagedType.LPStr)]string add, [MarshalAs(UnmanagedType.LPStr)]string cmd);


        void Sn_TransfEvent(string value)
        {
            snno = value;
        }
        void Sn_TransfintEvent(int value)
        {
            snlength = value;
        }
        private void Test_Click(object sender, EventArgs e)
        {
            pass = 1;pass_item = 1;flag = true;stop = 0; teston = 1;pause = false;signal = 0;
            int i, x;
            i = dataGridView1.Rows.Count;//所有行数
            for (x = 0; x < i; x++)
            {
                this.dataGridView1.Rows[x].DefaultCellStyle.BackColor = Color.White;
                for (int ll = 0; ll < 27; ll++)
                { dataGridView1.Rows[x].Cells[ll+2].Value = "";}
            }
            listBox1.Items.Clear();
            ErrBox.Items.Clear();

            savedata = "";
            SN snbox = new SN();
            Regex regnum = new Regex("^[0~9]");
            int snpass = 1, index;
            string snmodel2, revx, rev2, days2, revr = "", pcbrev2;
            if (checkBox1.Checked==true)
            {
                snbox.TransfEvent += Sn_TransfEvent;
                snbox.TransintfEvent += Sn_TransfintEvent;
                DialogResult ddr = snbox.ShowDialog();
                textBox1.Text = snno;
                if (snlength == 0) { teston = 0; }
                if (snlength != 0)
                {
                    if (snlength != samplelength){this.Statue.Text = "Length Err"; this.Statue.ForeColor = Color.Red; teston = 0;snpass = 0; }
                    if (snpass == 1)
                    {
                        index = snno.IndexOf(":");
                        snmodel2 = snno.Substring(0, index);
                        if (snmodel2 != smodel)
                        {
                            this.Statue.Text = "Model Err";
                            this.Statue.ForeColor = Color.Red;// 颜色 
                            snpass = 0; teston = 0;
                        }
                    }
                    if (snpass == 1)
                    {
                        revx = getrev(snno, revr);
                        //  MessageBox.Show(rev);  
                        if (regnum.IsMatch(revx.Substring(0, 1)))
                        {
                            // MessageBox.Show("Num");
                            rev2 = revx.Substring(0, 2);
                            days2 = revx.Substring(2, 4);
                            // MessageBox.Show(rev);
                        }
                        else
                        {
                            rev2 = revx.Substring(0, 1);
                            days2 = revx.Substring(1, 4);
                            // MessageBox.Show(rev);
                        }
                        if (rev2 != rev)
                        {
                            this.Statue.Text = "rev Err";
                            this.Statue.ForeColor = Color.Red;// 颜色 
                            snpass = 0; teston = 0;
                        }
                        if (days2 != pdays)
                        {
                            this.Statue.Text = "day Err";
                            this.Statue.ForeColor = Color.Red;// 颜色 
                            snpass = 0; teston = 0;
                        }
                    }
                    if (snpass == 1)
                    {
                        revr = "";
                        pcbrev2 = getpcbrev(snno, revr);
                        if (pcbrev2 != pcbrev)
                        {
                            this.Statue.Text = "PCBREV Err";
                            this.Statue.ForeColor = Color.Red;// 颜色 
                            snpass = 0; teston = 0;
                        }
                    }
                }//snlegth 0
            }//checkbox true
            if (checkBox1.Checked == false) { teston = 1; }
            if(teston==1)
            {
                Statue.Text = "Testing"; this.Statue.ForeColor = Color.Blue;// 颜色 
                if (MODEL.Text.IndexOf("-90") == -1) { outport("Dev2/port0", 0x1A); }
                if (MODEL.Text.IndexOf("-90") != -1) { outport("Dev2/port0", 0x1A); Delay(2000); outport("Dev2/port0", 0x18); }
                thread1 = new Thread(Thread_Test);
                    //   thread1 = new Thread(new ThreadStart(Thread_Test));              
                         thread1.IsBackground = true;
                thread1.Start();
            }
        


            //  outport("Dev2/port0", 0x0A);
        }

        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        [DllImport("kernel32.dll")]
        public static extern uint GetTickCount();
        public static void Delay(uint ms)
        {
            uint start = GetTickCount();
            while (GetTickCount() - start < ms)
            {
                Application.DoEvents();
            }
        }

        //  private delegate string CanCom(Ecan.CAN_OBJ caninfo,out Ecan.CAN_OBJ resultobj);
        // CanCom cansend = (caninfo,out resultobj) =>
        private string canreceive2(out Ecan.CAN_OBJ[] resultobj,int count)
        {
            string canresult = "";
            Ecan.CAN_ERR_INFO errinfo;
            int workStationCount = count;
            int size = Marshal.SizeOf(typeof(Ecan.CAN_OBJ));
            IntPtr infosIntptr = Marshal.AllocHGlobal(size * workStationCount);          
            resultobj = new Ecan.CAN_OBJ[workStationCount];

        //    MessageBox.Show(count.ToString());

            Delay(40);
            if (Ecan.Receive2(4, 0, 0, infosIntptr, (ushort)count, 10) == Ecan.ECANStatus.STATUS_OK) {/* MessageBox.Show(resultobj.ID.ToString("X"));*/ Delay(30); }
            else
            {
                Ecan.ReadErrInfo(4, 0, 0, out errinfo);
                canresult = "ReciveCanbus ErrCode:" + errinfo.ErrCode.ToString("X");
             //   for (int i = 0; i < 8; i++) { resultobj.data[i] = 0xFF; }
               // resultobj.ID = 268435455;
            }
            for (int inkIndex = 0; inkIndex < workStationCount; inkIndex++)
            {
                IntPtr ptr = (IntPtr)((UInt32)infosIntptr + inkIndex * size);
                resultobj[inkIndex] = (Ecan.CAN_OBJ)Marshal.PtrToStructure(ptr, typeof(Ecan.CAN_OBJ));
            }
      
         //   MessageBox.Show(resultobj[0].ID.ToString());
           // MessageBox.Show(resultobj[1].ID.ToString());
            Ecan.ClearCanbuf(4, 0, 0);
            Delay(30);
            return canresult;
        }



        private string canreceive(out Ecan.CAN_OBJ resultobj)
        {
            string canresult = "";
            Ecan.CAN_ERR_INFO errinfo;
            resultobj = new Ecan.CAN_OBJ();
         
            
            
                Delay(40);
                if (Ecan.Receive(4, 0, 0, out resultobj, (ushort)1, 10) == Ecan.ECANStatus.STATUS_OK) {/* MessageBox.Show(resultobj.ID.ToString("X"));*/ Delay(30); }
                else
                {
                    Ecan.ReadErrInfo(4, 0, 0, out errinfo);
                    canresult = "ReciveCanbus ErrCode:" + errinfo.ErrCode.ToString("X");
                    for (int i = 0; i < 8; i++) { resultobj.data[i] = 0xFF; }
                    resultobj.ID = 268435455;
                }

            
            Ecan.ClearCanbuf(4, 0, 0);
            return canresult;
        }

        private string cansend2(Ecan.CAN_OBJ caninfo, out Ecan.CAN_OBJ[] resultobj,int count)
        {
            string canresult = "";
            Ecan.CAN_ERR_INFO errinfo;
            int workStationCount = count;
            int size = Marshal.SizeOf(typeof(Ecan.CAN_OBJ));
            IntPtr infosIntptr = Marshal.AllocHGlobal(size * workStationCount);
            resultobj = new Ecan.CAN_OBJ[workStationCount];
            //resultobj = new Ecan.CAN_OBJ[count];
            try
            {
                if (Ecan.Transmit(4, 0, 0, ref caninfo, (ushort)1) != Ecan.ECANStatus.STATUS_OK)
                {
                    Ecan.ReadErrInfo(4, 0, 0, out errinfo);
                    canresult = "SendCanBus ErrCode:" + errinfo.ErrCode.ToString("X");
                   // for (int i = 0; i < 8; i++) { resultobj.data[i] = 0xFF; }
                   // resultobj.ID = 268435455;

                }
                else
                {
                   // if (canmodel == "HW Status") { Delay(13000); }
                    Delay(40);
                    if (Ecan.Receive2(4, 0, 0, infosIntptr, (ushort)count, 10) == Ecan.ECANStatus.STATUS_OK) {/* MessageBox.Show(resultobj.ID.ToString("X"));*/ Delay(30); }
                    else
                    {
                        Ecan.ReadErrInfo(4, 0, 0, out errinfo);
                        canresult = "ReciveCanbus ErrCode:" + errinfo.ErrCode.ToString("X");
                      //  for (int i = 0; i < 8; i++) { resultobj.data[i] = 0xFF; }
                       // resultobj.ID = 268435455;
                    }
                    for (int inkIndex = 0; inkIndex < workStationCount; inkIndex++)
                    {
                        IntPtr ptr = (IntPtr)((UInt32)infosIntptr + inkIndex * size);
                        resultobj[inkIndex] = (Ecan.CAN_OBJ)Marshal.PtrToStructure(ptr, typeof(Ecan.CAN_OBJ));
                    }

                }

            }
            catch (Exception ee)
            { ErrBox.Items.Add(ee.Message); }
            Ecan.ClearCanbuf(4, 0, 0);







            return canresult;
        }




        private string cansend(Ecan.CAN_OBJ caninfo,out Ecan.CAN_OBJ resultobj)    
        {
              string canresult = "";
              Ecan.CAN_ERR_INFO errinfo;
            resultobj = new Ecan.CAN_OBJ();
            try
            {
                if (Ecan.Transmit(4, 0, 0, ref caninfo, (ushort)1) != Ecan.ECANStatus.STATUS_OK)
                {
                    Ecan.ReadErrInfo(4, 0, 0, out errinfo);
                    canresult = "SendCanBus ErrCode:" + errinfo.ErrCode.ToString("X");
                    for (int i = 0; i < 8; i++) { resultobj.data[i] = 0xFF; }
                    resultobj.ID = 268435455;

                }
                else
                {
                    if (canmodel == "HW Status") { Delay(13000); }
                    Delay(40);
                    if (Ecan.Receive(4, 0, 0, out resultobj, (ushort)1, 10) == Ecan.ECANStatus.STATUS_OK) {/* MessageBox.Show(resultobj.ID.ToString("X"));*/ Delay(30); }
                    else
                    {
                        Ecan.ReadErrInfo(4, 0, 0, out errinfo);
                        canresult = "ReciveCanbus ErrCode:" + errinfo.ErrCode.ToString("X");
                        for (int i = 0; i < 8; i++) { resultobj.data[i] = 0xFF; }
                        resultobj.ID = 268435455;
                    }

                }
            }
            catch (Exception ee)
            { ErrBox.Items.Add(ee.Message); }
                Ecan.ClearCanbuf(4, 0, 0);
              return canresult;
          }

        public delegate string MEAS_FRE(string add2100);
        MEAS_FRE measres = (add2100) =>
          {
              string result = "";
              IntPtr intPtr = gpread(add2100, "MEAS:FRES? 1,100");
               result = Marshal.PtrToStringAnsi(intPtr);
              if (result == "") { result = "-1E+2"+"\n"; }
              return result;
          };

        public delegate string Outport(string Chan, UInt32 Data);
        Outport outport = (Chan, Data) =>
          {
              string message = "";
              UInt32 handel;
              IntPtr pA,intPtr;
              Int32 errcode;
              Int32 writenum;
              handel = CreatTask();
              pA = new IntPtr(handel);
              errcode = ConfigDOChann(pA, Chan, "");
              errcode = StartTask(pA);
              errcode = Writeport(pA, 1, 1, 5.0, Data, out writenum);
              if (errcode != 0)
              {
                  intPtr = GetErr(errcode);
                  message = Marshal.PtrToStringAnsi(intPtr);

              }
              errcode = StopTask(pA);
              errcode = ClearTask(pA);
              return message;
          };
        public delegate double[] MeasVol2(string Chan,int count);
        MeasVol2 measvol2 = (Chan, count) =>
        {
            int i;
            Int32 errcode, readnum;
            IntPtr intPtr, pA;
            UInt32 handel;
            double[] data;
            data = new double[count];
            handel = CreatTask();
            pA = new IntPtr(handel);
            ConfigChann(pA, Chan, "", 10083, -10.0, 10.0, 10348); ConfigSampleClk(pA, 10000.0, 10280, 10178, 1000); StartTask(pA);
            errcode = GetDCVol(pA, 1000, 10, data, out readnum);
            StopTask(pA);
            ClearTask(pA);


            return data;
        };

        public delegate double MeasVol(string Chan);
        MeasVol measvol = (Chan) =>
          {
              double result = -1000.0,zong=0.0;
              int i;
              Int32 errcode, readnum;
              IntPtr intPtr, pA;
              UInt32 handel;
              double[] data;
              data = new double[1000];
              handel = CreatTask();
              pA = new IntPtr(handel);
              ConfigChann(pA, Chan, "", 10083, -10.0, 10.0, 10348);ConfigSampleClk(pA, 10000.0, 10280, 10178, 1000);StartTask(pA);
              errcode = GetDCVol(pA, 1000, 10, data, out readnum);
              if (errcode == 0)
              {
                  for (i=0; i<1000;i++) { zong = zong + data[i]; }
                  result = zong / 1000;
              }
              StopTask(pA);
              ClearTask(pA);


              return result;
          };





        public delegate string Send(SerialPort sp1, string common);
        Send sendcomm = (sp1, common) =>
        {
            // SerialPort sp1;
            //sp1 = new SerialPort("com1");
            string errmessage="";
            byte[] ee2 = new byte[common.Length / 2];
            for (int i = 0; i < ee2.Length; i++)
            {
                ee2[i] = Convert.ToByte(common.Substring(i * 2, 2), 16);
            }
            Delay(50);
                //    sp.Open();
                try
            {
                sp1.Write(ee2, 0, 9);
            }
            catch (Exception ee)
            {
                //MessageBox.Show(ee.Message);
                errmessage = ee.ToString();
            }
            return errmessage;
        };

        private void equipAddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Equip equ = new Equip();
            DialogResult ddr = equ.ShowDialog();
        }
        FileStream passcheck;
        private void Form1_Load(object sender, EventArgs e)
        {
            #region //判断系统是否已启动

            System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcessesByName("NPETEST");//获取指定的进程名   
            if (myProcesses.Length > 1) //如果可以获取到知道的进程名则说明已经启动
            {
                MessageBox.Show("程序已启动！");
                Application.Exit();              //关闭系统
            }
            //

            #endregion
            string checkp;
            Test.Enabled = false;
            dataGridView1.ColumnCount = 29;
            dataGridView1.Columns[0].HeaderText = "Seq";
            dataGridView1.Columns[0].Width = 30;
            dataGridView1.Columns[1].HeaderText = "Item";
            dataGridView1.Columns[1].Width = 150;
            dataGridView1.Columns[2].HeaderText = "NC1";
            dataGridView1.Columns[2].Width = 40;
            dataGridView1.Columns[3].HeaderText = "NO2";
            dataGridView1.Columns[3].Width = 40;
            dataGridView1.Columns[4].HeaderText = "NC3";
            dataGridView1.Columns[4].Width = 40;
            dataGridView1.Columns[5].HeaderText = "NO4";
            dataGridView1.Columns[5].Width = 40;
            dataGridView1.Columns[6].HeaderText = "LED";
            dataGridView1.Columns[6].Width = 40;
            dataGridView1.Columns[7].HeaderText = "LVDT";
            dataGridView1.Columns[7].Width = 40;
            dataGridView1.Columns[8].HeaderText = "AD1";
            dataGridView1.Columns[8].Width = 40;
            dataGridView1.Columns[9].HeaderText = "AD2";
            dataGridView1.Columns[9].Width = 40;
            dataGridView1.Columns[10].HeaderText = "AD3";
            dataGridView1.Columns[10].Width = 40;
            dataGridView1.Columns[11].HeaderText = "AD4";
            dataGridView1.Columns[11].Width = 40;
            dataGridView1.Columns[12].HeaderText = "AD5";
            dataGridView1.Columns[12].Width = 40;
            dataGridView1.Columns[13].HeaderText = "AD6";
            dataGridView1.Columns[13].Width = 40;
            dataGridView1.Columns[14].HeaderText = "AD7";
            dataGridView1.Columns[14].Width = 40;
            dataGridView1.Columns[15].HeaderText = "AD8";
            dataGridView1.Columns[15].Width = 40;
            dataGridView1.Columns[16].HeaderText = "ILS1";
            dataGridView1.Columns[16].Width = 40;
            dataGridView1.Columns[17].HeaderText = "ILS2";
            dataGridView1.Columns[17].Width = 40;
            dataGridView1.Columns[18].HeaderText = "ILS3";
            dataGridView1.Columns[18].Width = 40;
            dataGridView1.Columns[19].HeaderText = "ILS4";
            dataGridView1.Columns[19].Width = 40;
            dataGridView1.Columns[20].HeaderText = "ISH1";
            dataGridView1.Columns[20].Width = 40;
            dataGridView1.Columns[21].HeaderText = "FIN1DC";
            dataGridView1.Columns[21].Width = 40;
            dataGridView1.Columns[22].HeaderText = "FIN2DC";
            dataGridView1.Columns[22].Width = 40;
            dataGridView1.Columns[23].HeaderText = "FIN1FREQ";
            dataGridView1.Columns[23].Width = 40;
            dataGridView1.Columns[24].HeaderText = "FIN2FREQ";
            dataGridView1.Columns[24].Width = 40;
            dataGridView1.Columns[25].HeaderText = "KEYVOL";
            dataGridView1.Columns[25].Width = 40;
            dataGridView1.Columns[26].HeaderText = "DCR1";
            dataGridView1.Columns[26].Width = 40;
            dataGridView1.Columns[27].HeaderText = "DCR2";
            dataGridView1.Columns[27].Width = 40;
            dataGridView1.Columns[28].HeaderText = "Result";
            dataGridView1.Columns[28].Width = 40;
            dataGridView1.ReadOnly = true;
            this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.MODEL.Font = new Font("隶书", 18, FontStyle.Bold); //第一个是字体，第二个大小，第三个是样式，
            this.MODEL.ForeColor = Color.Blue;// 颜色 
            this.Statue.Font = new Font("隶书", 24, FontStyle.Bold); //第一个是字体，第二个大小，第三个是样式，
            this.Statue.ForeColor = Color.Blue;// 颜色 
            this.Statue.Text = "Ready";
            FileStream passtext;
            if (!File.Exists("C:\\Windows\\System\\password.txt"))
            {
                passtext = new FileStream("C:\\Windows\\System\\password.txt", FileMode.Create);
                StreamWriter checkok = new StreamWriter(passtext); checkok.Write("test2019"); checkok.Flush(); checkok.Close(); passcheck.Close();
            }
          
            // C:\ProgramData
              if (!File.Exists("C:\\Windows\\System32\\dcheck.txt"))
              { 
                  passcheck = new FileStream("C:\\Windows\\System32\\dcheck.txt", FileMode.Create);
                  StreamWriter checkok= new StreamWriter(passcheck); checkok.Write("PASS");checkok.Flush();checkok.Close();passcheck.Close();
              }
              else
              {
                  passcheck = new FileStream("C:\\Windows\\System32\\dcheck.txt", FileMode.Open);
                  StreamReader sr3= new StreamReader(passcheck);checkp = sr3.ReadLine(); passcheck.Close();
                  if (checkp.IndexOf("FAIL") != -1) { Password pwprd = new Password();DialogResult ddr = pwprd.ShowDialog(); }

                          }
              
          


        }
        void frm_TransfEvent(string value)
        {
            snsample = value;
        }
        void frm_TransintfEvent(int valuet)
        {
            samplelength = valuet;
        }
        static string getrev(String num, String result)
        {
            char[] numch = num.ToCharArray();
            int x, revposition, revposition2;
            x = 0;
            revposition = 0;
            revposition2 = 0;
            for (int i = 0; i < num.Length; i++)
            {
                if (numch[i].ToString() == ":")
                {
                    x++;
                    if (x == 1)
                    {
                        revposition = i;
                        // break;
                    }
                    if (x == 2)
                    {
                        revposition2 = i;
                        // break;
                    }

                }

            }
            result = num.Substring(revposition + 1, revposition2 - revposition - 1);
            // MessageBox.Show(result);
            return result;
        }
        static string getpcbrev(String num, String result)
        {
            char[] numch = num.ToCharArray();
            int x, revposition, revposition2, count;
            x = 0;
            count = 0;
            revposition = 0;
            revposition2 = 0;
            for (int i2 = 0; i2 < num.Length; i2++)
            {
                if (numch[i2].ToString() == ":")
                {
                    count++;
                }
            }
            if (count > 2)
            {
                for (int i = 0; i < num.Length; i++)
                {
                    if (numch[i].ToString() == ":")
                    {
                        x++;

                        if (x == 2)
                        {
                            revposition = i;
                            // break;
                        }
                        if (x == 3)
                        {
                            revposition2 = i;
                            // break;
                        }


                    }
                }
            }

            if (count > 2)
            { result = num.Substring(revposition + 1, revposition2 - revposition - 1); }
            else
            { result = ""; }
            return result;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            byte[] sendData;
            if (int.Parse(CanEnable) == 1)
            { Ecan.CloseDevice(4, 0); }
            if (open == 1) { sp.Close(); }

           
            if (pow1add!="0")
            {
                sendData = null;
                sendData = Encoding.UTF8.GetBytes("VSET1:0");
                power1sp.Write(sendData, 0, sendData.Length);
                power1sp.Close();
            }
            if (pow2add != "0")
            { power2sp.Close(); }

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // MessageBox.Show(dataGridView1.Rows.Count.ToString());
           for (int rw=dataGridView1.Rows.Count;rw>1;rw--)
            {
                if (rw >= 2)
                {
                    DataGridViewRow row = dataGridView1.Rows[rw - 2];
                    dataGridView1.Rows.Remove(row);
                }
            }
            Item = 0;
            open = 1;
            ofile = ofile + 1;
            StringBuilder temp = new StringBuilder(500);
            OpenFileDialog ofd = new OpenFileDialog();
            string motorstr,errinfo,model;
            string[] seq;
            ofd.Filter = "ini文件(*.ini;*.txt)|*.ini;*.txt|所有文件|*.*";
            ofd.ValidateNames = true;
            ofd.CheckPathExists = true;
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
              //  opstatue = 1;
                ff = ofd.FileName;
                GetPrivateProfileString("HardInfo", "StepMotor", "0", temp, 500, "D:\\HardInfo.ini");
                motorstr = temp.ToString();
                motorstr=motorstr.Replace("ASRL", "com");
                if (ofile == 1)
                {
                    sp = new SerialPort(motorstr.Substring(0, 4));
                    sp.BaudRate = 9600;
                    sp.DataBits = 8;
                    sp.Parity = Parity.None;
                    sp.StopBits = StopBits.One;
                    sp.Handshake = Handshake.None;
                    try
                    {

                        sp.Open();

                    }

                    catch (Exception ee)
                    {
                        ErrBox.Items.Add("StepMotor Com Err:" + ee);
                        //sp.Close();
                        //sp.Open();
                    }
                    sp.ReceivedBytesThreshold = 1;
                    sp.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(sp_DataReceived);
                }
                errinfo = sendcomm(sp, "FFAA03019001B400F2");
                if(errinfo!="")
                { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                errinfo = sendcomm(sp, "FFAA03059600010048");
                if (errinfo != "")
                { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                errinfo = sendcomm(sp, "FFAA030C01000000B9");
                if (errinfo != "")
                { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                errinfo = sendcomm(sp, "FFAA030A00000000B6");
                if (errinfo != "")
                { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                errinfo = sendcomm(sp, "FFAA030E00000000BA");
                if (errinfo != "")
                { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                errinfo = sendcomm(sp, "FFAA030800000000B4"); 
                if (errinfo != "")
                { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                GetPrivateProfileString("HardInfo", "Keithley", "0", temp, 500, "D:\\HardInfo.ini");
                keithleyadd = temp.ToString();
                GetPrivateProfileString("HardInfo", "Pow1", "0", temp, 500, "D:\\HardInfo.ini");
                pow1add = temp.ToString();
                if (pow1add != "0")
                {
                    pow1add = pow1add.Replace("ASRL", "com");
                    if (ofile == 1)
                    {
                        power1sp = new SerialPort(pow1add.Substring(0, 4));
                        power1sp.BaudRate = 9600;
                        power1sp.DataBits = 8;
                        power1sp.Parity = Parity.None;
                        power1sp.StopBits = StopBits.Two;
                        power1sp.Handshake = Handshake.None;
                        try
                        {

                            power1sp.Open();

                        }

                        catch (Exception ee)
                        {
                            ErrBox.Items.Add("Power1 Com Err:" + ee);
                     //       power1sp.Close();
                       //     power1sp.Open();

                        }
                    }

                    power1sp.ReceivedBytesThreshold = 1;
                    power1sp.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(sp2_DataReceived);
                }
                    GetPrivateProfileString("HardInfo", "Pow2", "0", temp, 500, "D:\\HardInfo.ini");
                pow2add = temp.ToString();
                if (pow2add != "0")
                {
                    pow2add = pow2add.Replace("ASRL", "com");
                    if (ofile == 1)
                    {
                        power2sp = new SerialPort(pow2add.Substring(0, 4));
                        power2sp.BaudRate = 9600;
                        power2sp.DataBits = 8;
                        power2sp.Parity = Parity.None;
                        power2sp.StopBits = StopBits.One;
                        power2sp.Handshake = Handshake.None;
                        try
                        {

                            power2sp.Open();

                        }

                        catch (Exception ee)
                        {
                            ErrBox.Items.Add("Power2 Com Err:" + ee);
                 //           power2sp.Close();
                   //         power2sp.Open();
                        }
                    }
                    power2sp.ReceivedBytesThreshold = 1;
                    power2sp.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(sp3_DataReceived);
                }
                    GetPrivateProfileString("HardInfo", "NC1", "0", temp, 500, "D:\\HardInfo.ini");
                NC1add = temp.ToString();
                GetPrivateProfileString("HardInfo", "NO2", "0", temp, 500, "D:\\HardInfo.ini");
                NO2add = temp.ToString();
                GetPrivateProfileString("HardInfo", "NC3", "0", temp, 500, "D:\\HardInfo.ini");
                NC3add = temp.ToString();
                GetPrivateProfileString("HardInfo", "NO4", "0", temp, 500, "D:\\HardInfo.ini");
                NO4add = temp.ToString();
                GetPrivateProfileString("HardInfo", "LED", "0", temp, 500, "D:\\HardInfo.ini");
                LEDadd = temp.ToString();
                GetPrivateProfileString("HardInfo", "Keithley", "0", temp, 500, "D:\\HardInfo.ini");
                DCRADD = temp.ToString();
                errinfo = outport("Dev2/port0",0x1F);
                if (errinfo != "")
                { ErrBox.Items.Add("Daqoutport Err:" + errinfo); }
                GetPrivateProfileString("Info", "CanEnable", "0", temp, 500, ff);
                CanEnable = temp.ToString();
if(int.Parse(CanEnable)==1)
                {
                   
                    if (Ecan.OpenDevice(4, 0, 0) != Ecan.ECANStatus.STATUS_OK)
                    { ErrBox.Items.Add("Open CanDevice Err" ); }
                    Ecan.INIT_CONFIG init_config = new Ecan.INIT_CONFIG();
                    init_config.AccCode = 0;
                    init_config.AccMask = 0xffffffff;
                    init_config.Filter = 0;
                    init_config.Timing0 = 0x01;
                    init_config.Timing1 = 0x1C;
                    init_config.Mode = 0;
                    init_config.Reserved = 0x00;
                    if (Ecan.InitCAN(4, 0, 0, ref init_config) != Ecan.ECANStatus.STATUS_OK)
                    { ErrBox.Items.Add("Init Can0 fault!"); }
                    if (Ecan.InitCAN(4, 0, 1, ref init_config) != Ecan.ECANStatus.STATUS_OK)
                    { ErrBox.Items.Add("Init Can1 fault!"); }
                    if (Ecan.StartCAN(4, 0, 0) != Ecan.ECANStatus.STATUS_OK)
                    { ErrBox.Items.Add("Start Can0 Fail!"); }
                    if (Ecan.StartCAN(4, 0, 1) != Ecan.ECANStatus.STATUS_OK)
                    { ErrBox.Items.Add("Start Can1 Fail!"); }



                }
                GetPrivateProfileString("Info", "CoilNumber", "0", temp, 500, ff);
                CoilNumber = temp.ToString();
                GetPrivateProfileString("Info", "Model", "Model", temp, 500, ff);
                model = temp.ToString();
                MODEL.Text = model;
                path2 = DateTime.Now.ToString("yyyy");            
                savepath = "E:\\DanfossTEST";
                savepath = savepath + "\\" + model;
                if (!Directory.Exists(savepath))
                {
                    Directory.CreateDirectory(savepath);
                }
                savepath = savepath + "\\" + path2 + ".txt";
                if (!File.Exists(savepath))
                {
                    savefile = new FileStream(savepath, FileMode.Create);
                    sw3 = new StreamWriter(savefile);
                    filenew = true;
                    sw3.Flush();
                    //关闭流
                    sw3.Close();
                    savefile.Close();
                }
              /*  else
                {
                    savefile = new FileStream(savepath, FileMode.Append);
                    sw3 = new StreamWriter(savefile);
                    filenew = false;
                  // sw3.Write("\r\n" + "\r\n" + "\r\n");
                    sw3.Flush();
                    //关闭流
                    sw3.Close();
                    savefile.Close();
                }
                */


                GetPrivateProfileString("Info", "TestItem", "3", temp, 500, ff);
                seq = temp.ToString().Split(',');
                int x2 = 0,x=0;
                string item = "Item";
                foreach (string azu in seq)
                {x++;}
                dataGridView1.Rows.Add(x);Seq = new int[x]; 
                foreach (string azu in seq)
                {
                    // x = 0;
                    //  seqs[x] = azu;
                    Seq[x2] = int.Parse(azu);
                    item = item + Seq[x2].ToString();
                    dataGridView1.Rows[x2].Cells[0].Value = x2 + 1;
                    GetPrivateProfileString(item, "Item", "N/A", temp, 500, ff);
                    dataGridView1.Rows[x2].Cells[1].Value = temp.ToString();
                    // MessageBox.Show(Seq[x2].ToString()); 
                    item = "Item";
                    x2++;
                    Item++;
                }
            /*    SampleSN sminput = new SampleSN();
                Regex regnum = new Regex("^[0~9]");
                sminput.TransfEvent += frm_TransfEvent;
                sminput.TransintfEvent += frm_TransintfEvent;
                DialogResult ddr = sminput.ShowDialog();

                int sindex;
                string revx, revr;
                if(samplelength!=0)
                { Test.Enabled = true; }
                revr = "";
                sindex = snsample.IndexOf(":");
                smodel = snsample.Substring(0, sindex);
                //  MessageBox.Show(smodel);
                revx = getrev(snsample, revr);
                //  MessageBox.Show(rev);  

                if (regnum.IsMatch(revx.Substring(0, 1)))
                {
                    // MessageBox.Show("Num");
                    rev = revx.Substring(0, 2);
                    pdays = revx.Substring(2, 4);
                    // MessageBox.Show(rev);
                }
                else
                {
                    rev = revx.Substring(0, 1);
                    pdays = revx.Substring(1, 4);
                    // MessageBox.Show(rev);
                }
                revr = "";
                pcbrev = getpcbrev(snsample, revr);*/

            }
            Test.Enabled = true;
            Test.Focus();

        }








        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Byte[] result = new Byte[6];
            string resultbuff,result2="";
            int i;
            resultbuff = "";
            sp.Read(result, 0, 6);
          //  result2 = power1sp.ReadLine();
            //  MessageBox.Show(result.ToString());
            if (result != null)
            {
                for (i = 0; i < result.Length; i++)
                { resultbuff += result[i].ToString("X2"); }
            }
            ErrBox.Items.Add("Step Common:" + resultbuff);

        }

        private void sp2_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
          //  Byte[] result = new Byte[6];
            string resultbuff;
            int i;
            Byte[] receivedData = new Byte[power1sp.BytesToRead];        //创建接收字节数组
                                power1sp.Read(receivedData, 0, receivedData.Length);         //读取数据
            resultbuff=new UTF8Encoding().GetString(receivedData);//
                                                        /* resultbuff = "";
                                                         power1sp.Read(result, 0, 6);
                                                         if (result != null)
                                                         {
                                                             for (i = 0; i < result.Length; i++)
                                                             { resultbuff += result[i].ToString("X2"); }
                                                         }*/
                                                        // if (result2!="")
                                                        // {
            ErrBox.Items.Add("Vbat:" + resultbuff);
          //  }
           

        }
        private void sp3_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //  Byte[] result = new Byte[6];
            string resultbuff;
            int i;
            Byte[] receivedData = new Byte[power2sp.BytesToRead];        //创建接收字节数组
            power2sp.Read(receivedData, 0, receivedData.Length);         //读取数据
            resultbuff = new UTF8Encoding().GetString(receivedData);//
                                                                    /* resultbuff = "";
                                                                     power1sp.Read(result, 0, 6);
                                                                     if (result != null)
                                                                     {
                                                                         for (i = 0; i < result.Length; i++)
                                                                         { resultbuff += result[i].ToString("X2"); }
                                                                     }*/
                                                                    // if (result2!="")
                                                                    // {
            ErrBox.Items.Add("Vbat2:" + resultbuff);
            //  }


        }
        string motor_Dis(string disstr)
        {
            string dis="";
            int pulse, i;
            string pbuff2, comm, checksum;
            string[] pbuff;
            Int32[] sum;
            Int32 sum2;
            sum2 = 0;
            pbuff = new string[3];
            sum = new Int32[8];
            pulse = Int32.Parse(disstr);
            pbuff2 = pulse.ToString("X");
            if (pulse.ToString("X").Length < 6)
            {
                for (i = 0; i < 6 - pulse.ToString("X").Length; i++)
                { pbuff2 = "0" + pbuff2; }
            }
            for (i = 0; i < 3; i++){ pbuff[i] = pbuff2.Substring(i * 2, 2); }
            comm = "ffaa0303" + pbuff[2] + pbuff[1] + pbuff[0] + "00";
            byte[] ee3 = new byte[comm.Length / 2];
            for (i = 0; i < ee3.Length; i++)
            {
                ee3[i] = Convert.ToByte(comm.Substring(i * 2, 2), 16);
            }
            for (i = 0; i < ee3.Length; i++) { sum[i] = Convert.ToInt32(ee3[i].ToString("X"), 16); }
            for (i = 0; i < ee3.Length; i++){ sum2 = sum2 + sum[i]; }
            if (sum2.ToString("X").Length >= 2){ checksum = sum2.ToString("X").Substring(sum2.ToString("X").Length - 2, 2); }
            else{ checksum = "0" + sum2.ToString("X"); }
            comm = comm + checksum;dis = comm;
            return dis;
        }

        private int hextodec(string ss )//hex transform bin then bin transform dec can transform negative number
        {
            int i, shi, actdata = 0;
            long ww;
            string er1;
            string[] er;
            int leng, ix = 0;
            string erx = "";
            string erx2, erx4 = "";
            string[] erx3;
            erx3 = new string[16];
            er = new string[16];
            ww = Int32.Parse(ss, System.Globalization.NumberStyles.HexNumber);
            er1 = Convert.ToString(ww, 2);
            leng = er1.Length;
            if (leng < 16)
            {
                for (i = 0; i < 16 - leng; i++)
                {
                    er[i] = "0";
                }


                for (i = 16 - leng; i < 16; i++)
                {
                    er[i] = er1.Substring(ix, 1);
                    ix = ix + 1;
                    // MessageBox.Show(er[i]);
                }
                // MessageBox.Show("33");
            }
            else
            {

                for (i = 0; i < 16; i++)
                {
                    er[i] = er1.Substring(i, 1);
                }
            }

            if (er[0] == "1")
            {
                for (i = 0; i < 16; i++)
                {
                    erx = erx + er[i];
                }
                // MessageBox.Show(erx);
                shi = Convert.ToInt32(erx, 2);
                shi = shi - 1;
                erx2 = Convert.ToString(shi, 2);
                for (i = 0; i < 16; i++)
                {
                    if (erx2.Substring(i, 1) == "1")
                    {
                        erx3[i] = "0";
                    }
                    if (erx2.Substring(i, 1) == "0")
                    {
                        erx3[i] = "1";
                    }
                }
                for (i = 0; i < 16; i++)
                {
                    erx4 = erx4 + erx3[i];
                }
                actdata = Convert.ToInt32(erx4, 2);
                actdata = 0 - actdata;


            }
            if (er[0] == "0")
            {
                for (i = 0; i < 16; i++)
                {
                    erx = erx + er[i];
                }
                actdata = Convert.ToInt32(erx, 2);
            }
            return actdata;
        }


        void Thread_Test()
        {
            System.Diagnostics.Stopwatch sw= System.Diagnostics.Stopwatch.StartNew();
            StringBuilder temp = new StringBuilder(500);
            int i, i2, i3, i4, i5, fx,passitem=1;
            uint dtime;
            string canerrinfo,item1 = "Item", seqq = "SEQ",vbat,v34,errinfo="",distance,dishex,led="",ledact="",nc1,no2,nc3,no4,nc1act="",no2act="",nc3act="",no4act="",ccid="",crid="",cclength="0",crlength="0",ccdata="";
            byte[] sendData = null;
            double ledresult, nc1data, no2data,nc3data,no4data ;           
            if (listBox1.InvokeRequired == false)
            {
                for (i = 0; i < Item; i++)
                {
                  //  if (pause == true) { manualEvent = new AutoResetEvent(false); manualEvent.WaitOne();Thread.Sleep(1000); }
                   // this.manualEvent.WaitOne();              
                    if ((checkBox2.Checked == true) && (pass == 0)) {flag = false;}
                    if (flag == false){break;}
                    while (pause == true) { Delay(10); }
                    if (flag==true)
                    {
                        dataGridView1.CurrentCell = this.dataGridView1.Rows[i].Cells[0];
                        this.dataGridView1.Rows[i].Selected = true;
                        dataGridView1.Rows[i].Cells[0].Value = (i+1).ToString();
                        item1 = item1 + Seq[i].ToString();
                        GetPrivateProfileString(item1, "Item", "N/A", temp, 500, ff);
                        listBox1.Items.Add(temp.ToString());
                        dataGridView1.Rows[i].Cells[1].Value = temp.ToString();
                        GetPrivateProfileString(item1, "Delay", "500", temp, 500, ff);
                        dtime = uint.Parse(temp.ToString());
                        GetPrivateProfileString(item1, "Vbat", "0", temp, 500, ff);
                        vbat = temp.ToString();
                        listBox1.Items.Add("Vbat="+vbat);
                        if (vbat != "0")
                        {
                            sendData = Encoding.UTF8.GetBytes("VSET1:" + vbat);
                            power1sp.Write(sendData, 0, sendData.Length);
                            Delay(60);
                            sendData = null;
                            sendData = Encoding.UTF8.GetBytes("ISET1:3");
                            power1sp.Write(sendData, 0, sendData.Length);
                            Delay(60);
                            sendData = null;
                            sendData = Encoding.UTF8.GetBytes("OUT1");
                            power1sp.Write(sendData, 0, sendData.Length);
                            Delay(60);
                            sendData = null;
                            sendData = Encoding.UTF8.GetBytes("VOUT1?");
                            power1sp.Write(sendData, 0, sendData.Length);
                            Delay(60);
                        }
                        sendData = null;
                        GetPrivateProfileString(item1, "V34", "0", temp, 500, ff);
                        v34 = temp.ToString();
                        listBox1.Items.Add("V34=" + v34);
                        if (v34 != "0")
                        {
                            sendData = Encoding.UTF8.GetBytes("VSET1:" + v34);
                            power1sp.Write(sendData, 0, sendData.Length);
                            Delay(60);
                            sendData = null;
                            sendData = Encoding.UTF8.GetBytes("ISET1:3");
                            power1sp.Write(sendData, 0, sendData.Length);
                            Delay(60);
                            sendData = null;
                            sendData = Encoding.UTF8.GetBytes("OUT1");
                            power1sp.Write(sendData, 0, sendData.Length);
                            Delay(60);
                            sendData = null;
                            sendData = Encoding.UTF8.GetBytes("VOUT1?");
                            power1sp.Write(sendData, 0, sendData.Length);
                            Delay(60);
                        }
                        if ((vbat == "0") && (v34 == "0")&& (MODEL.Text.IndexOf("CLS"))==-1)
                        { errinfo = outport("Dev1/port0", 0x1F); }
                        if ((vbat!="0")&&(v34!="0") && (MODEL.Text.IndexOf("CLS")) == -1)
                        { errinfo = outport("Dev1/port0", 0x1A);}
                        if ((vbat != "0") && (v34 == "0") && (MODEL.Text.IndexOf("CLS")) == -1)
                        { errinfo = outport("Dev1/port0", 0x1E); }
                        if ((vbat == "0") && (v34 != "0") && (MODEL.Text.IndexOf("CLS")) == -1)
                        { errinfo = outport("Dev1/port0", 0x1B); }
                        if ((vbat != "0") && (v34 == "0") && (MODEL.Text.IndexOf("0118-CLS")) != -1)
                        { errinfo = outport("Dev3/port0", 0x1F); }
                        if ((vbat != "0") && (v34 == "0") && (MODEL.Text.IndexOf("0123-CLS")) != -1)
                        { errinfo = outport("Dev3/port0", 0x19); }

                        if (errinfo != "") { ErrBox.Items.Add("Daqoutport Err:" + errinfo); }

                        GetPrivateProfileString(item1, "Distance", "0", temp, 500, ff);
                        distance= temp.ToString();
if(int.Parse(distance)>0)
                        {
                            errinfo = sendcomm(sp, "FFAA030401010000B2");
                            if (errinfo != ""){ ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                            dishex = motor_Dis(distance);
                            errinfo = sendcomm(sp, dishex);
                            if (errinfo != ""){ ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                            errinfo = sendcomm(sp, "FFAA030900000000B5");
                            if (errinfo != "") { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                        }
                        if (int.Parse(distance) < 0)
                        {
                            errinfo = sendcomm(sp, "FFAA030400010000B1");
                            if (errinfo != "") { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                            distance =( 0 - int.Parse(distance)).ToString();
                         
                            //   MessageBox.Show(distance);
                            dishex = motor_Dis(distance);
                            errinfo = sendcomm(sp, dishex);
                            if (errinfo != "") { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                            errinfo = sendcomm(sp, "FFAA030900000000B5");
                            if (errinfo != "") { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                        }
                        if (int.Parse(distance) != 0) { Delay(dtime); }                       
                        string sendcandata = "", str1buf,recivecandata = "", lvdt="",lvdtmax="",lvdtmin="",dcr1="",dcr2="",dcr1max="",dcr1min="",dcr2max="",dcr2min="";
                        int tlen=0,hexTooct=0,ican=0,ind,ibuf;
                        Ecan.CAN_OBJ sendbusinfo = new Ecan.CAN_OBJ(), recbusinfo = new Ecan.CAN_OBJ();
                        Ecan.CAN_OBJ[] recbusinfo2;
                        GetPrivateProfileString(item1, "CCID", "", temp, 500, ff); ccid = temp.ToString();
                        GetPrivateProfileString(item1, "CRID", "", temp, 500, ff); crid = temp.ToString();
                        string[] seqcrid,crinfoid;
                        int cridcount = 0,px=0;
                        if (crid != "")
                        {
                            seqcrid = crid.ToString().Split(',');
                            //int xx = 0;
                            foreach (string azu in seqcrid) { cridcount++; }
                            crinfoid = new string[cridcount];
                            foreach (string azu in seqcrid) { crinfoid[px] = azu;px++; }
                        }
                        else { crinfoid = new string[cridcount+1]; }
                        recbusinfo2 = new Ecan.CAN_OBJ[cridcount+1];
                      //  MessageBox.Show(cridcount.ToString());
                        if (ccid == "FF")
                        {
                            canerrinfo = canreceive2(out recbusinfo2, cridcount);
                           // MessageBox.Show(recbusinfo2[0].ID.ToString());
                           // MessageBox.Show(recbusinfo2[1].ID.ToString());
                            for (int pfx = 0; pfx < cridcount; pfx++)
                            //  canerrinfo = cansend(sendbusinfo, out recbusinfo);
                            {
                                if (canerrinfo != "") { ErrBox.Items.Add(canerrinfo); }
                                if (canerrinfo == "")
                                {
                                    ErrBox.Items.Add("CanRecID :" + recbusinfo2[pfx].ID.ToString("X"));

                                }
                            }
                            }
                       
                        if ((ccid != "")&&(ccid != "FF"))
                        {
                            GetPrivateProfileString(item1, "CCLENGTH", "0", temp, 500, ff); cclength = temp.ToString();
                            sendbusinfo.SendType = 0; sendbusinfo.data = new byte[8]; sendbusinfo.Reserved = new byte[3]; sendbusinfo.RemoteFlag = 0; sendbusinfo.ExternFlag = 1; 
                            sendbusinfo.DataLen = Convert.ToByte(int.Parse(cclength));
                            GetPrivateProfileString(item1, "CCDATA", "", temp, 500, ff); ccdata = temp.ToString();
                             tlen = sendbusinfo.DataLen - 1;
                              for (ican = 0; ican <= tlen; ican++)
                               { sendbusinfo.data[ican] = Convert.ToByte(ccdata.Substring(0 + ican * 2, 2), 0X10); sendcandata = sendcandata + sendbusinfo.data[ican].ToString("X2") + " "; }
                                sendbusinfo.ID = Convert.ToUInt32(ccid, 16);
                          /*  sendbusinfo.data[0] = Convert.ToByte("01", 0X10);
                            sendbusinfo.data[1] = Convert.ToByte("01", 0X10);
                            sendbusinfo.data[2] = Convert.ToByte("02", 0X10);
                            sendbusinfo.data[3] = Convert.ToByte("01", 0X10);
                            sendbusinfo.ID = Convert.ToUInt32("4012010", 16);*/
                            listBox1.Items.Add("CCID :"+ sendbusinfo.ID.ToString("X")); listBox1.Items.Add("CCDATA :" + sendcandata);
                            GetPrivateProfileString(item1, "Canmodel", "", temp, 500, ff); canmodel = temp.ToString();
                           // canerrinfo = cansend(sendbusinfo, out recbusinfo);
                            canerrinfo = cansend2(sendbusinfo, out recbusinfo2,cridcount);
                            for (int pfx = 0; pfx < cridcount; pfx++)
                            {
                                if (canerrinfo != "") { ErrBox.Items.Add(canerrinfo); }
                                if (canerrinfo == "")
                                {

                                    ErrBox.Items.Add("CanRecID :" + recbusinfo2[pfx].ID.ToString("X"));

                                }
                            }
                            }
                        string actidstr;
                        actidstr = "Act CRID:";
                        for (int pfx = 0; pfx < cridcount; pfx++)
                        { actidstr = actidstr + " " + recbusinfo2[pfx].ID.ToString("X"); }
                        listBox1.Items.Add(actidstr);
                        float canfresult=0;
                        //   recivecandata = "";
                        string cistatue = "",LVDTmax="",LVDTmin="",LVDTstatue="xxxx";
                        string[] seq,seqx,seq2,swv;
                        seqx = new string[8];
                        swv= new string[3];
                        int xx = 0,pp=0,idright=0,pf=0,pfxx=0;
                      //  MessageBox.Show(cridcount.ToString());
                        if (crid != "")
                        {
                            for (pfxx = 0; pfxx < cridcount; pfxx++)
                            {
                                idright = 0;
                                pf = 0;
                              //  MessageBox.Show(cridcount.ToString());
                                do
                                {
                                  //  MessageBox.Show(pf.ToString());
                                    if (crinfoid[pfxx].ToUpper() == recbusinfo2[pf].ID.ToString("X").ToUpper())
                                    {
                                        idright = 1;
                                        tlen = recbusinfo2[pf].DataLen - 1;
                                        for (ican = 0; ican <= tlen; ican++)
                                        { recivecandata = recivecandata + recbusinfo2[pf].data[ican].ToString("X2") + " "; }
                                        listBox1.Items.Add("CRID :" + recbusinfo2[pf].ID.ToString("X")); listBox1.Items.Add("CRDATA :" + recivecandata);
                                        GetPrivateProfileString(item1, "LVDTMAX", "", temp, 500, ff); lvdtmax = temp.ToString();
                                        GetPrivateProfileString(item1, "LVDTMIN", "", temp, 500, ff); lvdtmin = temp.ToString();
                                        if (((lvdtmax != "") || (lvdtmin != "")) && (MODEL.Text.IndexOf("CI") != -1))
                                        {
                                            GetPrivateProfileString(item1, "Canmodel", "", temp, 500, ff); canmodel = temp.ToString();
                                            if (canmodel == "SW version")
                                            {
                                                seq = lvdtmax.ToString().Split(',');
                                                //int xx = 0;
                                                foreach (string azu in seq) { seqx[xx] = azu; xx++; }

                                                for (pp = 0; pp < 3; pp++) { swv[pp] = seqx[pp]; }
                                                for (pp = 0; pp < 3; pp++)
                                                {
                                                    if (recbusinfo2[pf].data[pp + 2].ToString("X2") != swv[pp]) { pass = 0; passitem = 0; }
                                                    dataGridView1.Rows[i].Cells[7].Value = dataGridView1.Rows[i].Cells[7].Value + recbusinfo2[pf].data[pp + 2].ToString("X2") + " "; listBox1.Items.Add("SW Verion :" + recbusinfo2[pf].data[pp + 2].ToString("X2") + " Spec :" + swv[pp]);
                                                }

                                            }
                                            if (canmodel == "HW Status")
                                            {
                                                Delay(dtime);
                                                if (recbusinfo2[pf].data[0].ToString("X2") != lvdtmax) { pass = 0; passitem = 0; }
                                                dataGridView1.Rows[i].Cells[7].Value = recbusinfo2[pf].data[0].ToString("X2"); listBox1.Items.Add("HW Status :" + recbusinfo2[pf].data[0].ToString("X2"));
                                            }
                                            if (canmodel == "D2 5V")
                                            {
                                                lvdt = recbusinfo2[pf].data[7].ToString("X2") + recbusinfo2[pf].data[6].ToString("X2"); hexTooct = hextodec(lvdt);
                                                if ((hexTooct > int.Parse(lvdtmax)) || (hexTooct < int.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                                dataGridView1.Rows[i].Cells[7].Value = hexTooct.ToString(); listBox1.Items.Add("D2 5V :" + hexTooct.ToString());

                                            }

                                            /*seq = lvdtmax.ToString().Split(',');

                                        foreach(string azu in seq) { seqx[xx] = azu;xx++; }
                                        lvdtmax = seqx[1]; LVDTstatue = seqx[0];xx = 0; seq = lvdtmin.ToString().Split(',');
                                        foreach (string azu in seq) { seqx[xx] = azu; xx++; }
                                        lvdtmin = seqx[1];
                                        if (recbusinfo.data[2].ToString("X2") == "30") { cistatue = "blocked"; }
                                        if (recbusinfo.data[2].ToString("X2") == "33") { cistatue = "floating"; }
                                        if (recbusinfo.data[2].ToString("X2") == "31") { cistatue = "retracted";hexTooct= int.Parse(recbusinfo.data[0].ToString("X2"), System.Globalization.NumberStyles.HexNumber)-128; }
                                        if (recbusinfo.data[2].ToString("X2") == "32") { cistatue = "extended"; hexTooct = -(int.Parse(recbusinfo.data[1].ToString("X2"), System.Globalization.NumberStyles.HexNumber) - 128); }
                                        if (cistatue!=LVDTstatue) { pass = 0; passitem = 0; }
                                        dataGridView1.Rows[i].Cells[7].Value = cistatue; listBox1.Items.Add("LVDTstatue :" + cistatue);
                                        if ((hexTooct > int.Parse(lvdtmax)) || (hexTooct < int.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                        dataGridView1.Rows[i].Cells[7].Value = hexTooct.ToString(); listBox1.Items.Add("LVDT :" + hexTooct.ToString());
    */
                                        }
                                        if (((lvdtmax != "") || (lvdtmin != "")) && (MODEL.Text.IndexOf("CI") == -1))
                                        {
                                            lvdt = recbusinfo2[pf].data[1].ToString("X2") + recbusinfo2[pf].data[0].ToString("X2"); hexTooct = hextodec(lvdt);
                                            if ((hexTooct > int.Parse(lvdtmax)) || (hexTooct < int.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                            dataGridView1.Rows[i].Cells[7].Value = hexTooct.ToString(); listBox1.Items.Add("LVDT :" + hexTooct.ToString());
                                        }
                                        for (ibuf = 1; ibuf < 9; ibuf++)
                                        {
                                            str1buf = "AD" + ibuf.ToString() + "MAX"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmax = temp.ToString();
                                            str1buf = "AD" + ibuf.ToString() + "MIN"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmin = temp.ToString();
                                            if ((lvdtmax != "") || (lvdtmin != ""))
                                            {
                                                if (ibuf % 2 != 0) { lvdt = recbusinfo2[pf].data[ibuf].ToString("X2") + recbusinfo2[pf].data[ibuf - 1].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f / 1000; }
                                                if (ibuf % 2 == 0) { lvdt = recbusinfo2[pf].data[ibuf - 1].ToString("X2") + recbusinfo2[pf].data[ibuf - 2].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f / 1000; }
                                                if ((canfresult > float.Parse(lvdtmax)) || (canfresult < float.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                                dataGridView1.Rows[i].Cells[8 + ibuf - 1].Value = canfresult.ToString(); str1buf = "AD" + ibuf.ToString() + " :"; listBox1.Items.Add(str1buf + canfresult.ToString());
                                            }
                                        }
                                        for (ibuf = 1; ibuf < 5; ibuf++)
                                        {
                                            str1buf = "ILS" + ibuf.ToString() + "MAX"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmax = temp.ToString();
                                            str1buf = "ILS" + ibuf.ToString() + "MIN"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmin = temp.ToString();
                                            if ((lvdtmax != "") || (lvdtmin != ""))
                                            {
                                                if (ibuf % 2 != 0) { lvdt = recbusinfo2[pf].data[ibuf].ToString("X2") + recbusinfo2[pf].data[ibuf - 1].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f; }
                                                if (ibuf % 2 == 0) { lvdt = recbusinfo2[pf].data[ibuf - 1].ToString("X2") + recbusinfo2[pf].data[ibuf - 2].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f; }
                                                if ((canfresult > float.Parse(lvdtmax)) || (canfresult < float.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                                dataGridView1.Rows[i].Cells[16 + ibuf - 1].Value = canfresult.ToString(); str1buf = "ILS" + ibuf.ToString() + " :"; listBox1.Items.Add(str1buf + canfresult.ToString());
                                            }
                                        }
                                        GetPrivateProfileString(item1, "IHS1MAX", "", temp, 500, ff); lvdtmax = temp.ToString();
                                        GetPrivateProfileString(item1, "IHS1MIN", "", temp, 500, ff); lvdtmin = temp.ToString();
                                        if ((lvdtmax != "") || (lvdtmin != ""))
                                        {
                                            lvdt = recbusinfo2[pf].data[5].ToString("X2") + recbusinfo2[pf].data[4].ToString("X2"); hexTooct = hextodec(lvdt);
                                            if ((hexTooct > int.Parse(lvdtmax)) || (hexTooct < int.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                            dataGridView1.Rows[i].Cells[20].Value = hexTooct.ToString(); listBox1.Items.Add("ISH1 :" + hexTooct.ToString());
                                        }
                                        for (ibuf = 1; ibuf < 3; ibuf++)
                                        {
                                            str1buf = "FIN" + ibuf.ToString() + "DCMAX"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmax = temp.ToString();
                                            str1buf = "FIN" + ibuf.ToString() + "DCMIN"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmin = temp.ToString();
                                            if ((lvdtmax != "") || (lvdtmin != ""))
                                            {
                                                if (ibuf == 1) { lvdt = recbusinfo2[pf].data[1].ToString("X2") + recbusinfo2[pf].data[0].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f / 100; }
                                                if (ibuf == 2) { lvdt = recbusinfo2[pf].data[5].ToString("X2") + recbusinfo2[pf].data[4].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f / 100; }
                                                if ((canfresult > float.Parse(lvdtmax)) || (canfresult < float.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                                dataGridView1.Rows[i].Cells[21 + ibuf - 1].Value = canfresult.ToString(); str1buf = "FIN" + ibuf.ToString() + "DC :"; listBox1.Items.Add(str1buf + canfresult.ToString());
                                            }
                                        }
                                        for (ibuf = 1; ibuf < 3; ibuf++)
                                        {
                                            str1buf = "FIN" + ibuf.ToString() + "FREQMAX"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmax = temp.ToString();
                                            str1buf = "FIN" + ibuf.ToString() + "FREQMIN"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmin = temp.ToString();
                                            if ((lvdtmax != "") || (lvdtmin != ""))
                                            {
                                                if (ibuf == 1) { lvdt = recbusinfo2[pf].data[3].ToString("X2") + recbusinfo2[pf].data[2].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f ; }
                                                if (ibuf == 2) { lvdt = recbusinfo2[pf].data[7].ToString("X2") + recbusinfo2[pf].data[6].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f ; }
                                                if ((canfresult > float.Parse(lvdtmax)) || (canfresult < float.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                                dataGridView1.Rows[i].Cells[23 + ibuf - 1].Value = canfresult.ToString(); str1buf = "FIN" + ibuf.ToString() + "FREQ :"; listBox1.Items.Add(str1buf + canfresult.ToString());
                                            }
                                        }
                                        GetPrivateProfileString(item1, "KEYMAX", "", temp, 500, ff); lvdtmax = temp.ToString();
                                        GetPrivateProfileString(item1, "KEYMIN", "", temp, 500, ff); lvdtmin = temp.ToString();
                                        if ((lvdtmax != "") || (lvdtmin != ""))
                                        {
                                            lvdt = recbusinfo2[pf].data[5].ToString("X2") + recbusinfo2[pf].data[4].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f / 1000;
                                            if ((canfresult > float.Parse(lvdtmax)) || (canfresult < float.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                            dataGridView1.Rows[i].Cells[25].Value = canfresult.ToString(); listBox1.Items.Add("KEYVOL :" + canfresult.ToString());
                                        }

                                    }//crid.ToUpper()== recbusinfo.ID.ToString("X").ToUpper()
                                     //   MessageBox.Show(pf.ToString());
                                    
                                    pf++;
                                } while (pf < cridcount);
                                if (idright == 0)
                                {

                                    listBox1.Items.Add("CRID : " + crinfoid[pfxx].ToUpper() + "don't exit");
                                  
                                   pass = 0; passitem = 0; dataGridView1.Rows[i].Cells[7].Value = "ID ERR";
                                }
                            } 
                       
                            }//crid!=""
                      //  }
                        if (crid == "") { dataGridView1.Rows[i].Cells[7].Value = ""; }
                        double dcrr;
                        int cmi,ind2;
                        GetPrivateProfileString(item1, "DCR1MAX", "", temp, 500, ff); dcr1max = temp.ToString();
                        GetPrivateProfileString(item1, "DCR1MIN", "", temp, 500, ff); dcr1min = temp.ToString();
                        if ((dcr1max != "") || (dcr1min != ""))
                        {
                            if ((vbat != "0") && (v34 != "0")) { outport("Dev1/port0", 0x12); }
                            if ((vbat != "0") && (v34 == "0")) { outport("Dev1/port0", 0x16); }
                            if ((vbat == "0") && (v34 == "0")) { outport("Dev1/port0", 0x17); }
                            if ((vbat == "0") && (v34 != "0")) { outport("Dev1/port0", 0x13); }
                            Delay(dtime); dcr1 = measres(DCRADD); ind = dcr1.IndexOf("E"); ind2 = dcr1.IndexOf("\n");
                            //MessageBox.Show(dcr1.Length.ToString());
                           // MessageBox.Show(dcr1.Substring(1, ind-1)); MessageBox.Show(dcr1.Substring(ind + 1, ind2 - ind - 1));
                            cmi = int.Parse(dcr1.Substring(ind + 1, ind2-ind-1 ));
                            
                            dcrr = double.Parse(dcr1.Substring(1, ind-1)) * System.Math.Pow(10, cmi);
                            if ((dcrr-0.8>float.Parse(dcr1max))|| (dcrr-0.8 < float.Parse(dcr1min)))
                            { pass = 0; passitem = 0; }
                            dataGridView1.Rows[i].Cells[26].Value = (dcrr-0.8).ToString("#0.000"); listBox1.Items.Add("DCR1 :" + dcr1);
                        }
                        GetPrivateProfileString(item1, "DCR2MAX", "", temp, 500, ff); dcr2max = temp.ToString();
                        GetPrivateProfileString(item1, "DCR2MIN", "", temp, 500, ff); dcr2min = temp.ToString();
                        if ((dcr2max != "") || (dcr2min != ""))
                        {
                            if ((vbat != "0") && (v34 != "0")) { outport("Dev1/port0", 0x0A); }
                            if ((vbat != "0") && (v34 == "0")) { outport("Dev1/port0", 0x0E); }
                            if ((vbat == "0") && (v34 == "0")) { outport("Dev1/port0", 0x0F); }
                            if ((vbat == "0") && (v34 != "0")) { outport("Dev1/port0", 0x0B); }
                            Delay(dtime); dcr2 = measres(DCRADD); ind = dcr2.IndexOf("E"); ind2 = dcr2.IndexOf("\n");
                            cmi = int.Parse(dcr2.Substring(ind + 1, ind2-ind-1));
                            dcrr = double.Parse(dcr2.Substring(1, ind-1)) * System.Math.Pow(10, cmi);
                            if ((dcrr-0.8 > float.Parse(dcr2max)) || (dcrr-0.8 < float.Parse(dcr2min)))
                            { pass = 0; passitem = 0; }
                            dataGridView1.Rows[i].Cells[27].Value = (dcrr-0.8).ToString("#0.000"); listBox1.Items.Add("DCR2 :" + dcr2);
                        }

                        GetPrivateProfileString(item1, "LED", "", temp, 500, ff);
                        led = temp.ToString();
                        double[] ledresult2;
                        int ledred = 0, ledgreen = 0, ledblue = 0,ledtestcount=0;
                        if (led != "")
                        {
                            Delay(dtime);
                            ledresult2 = new double[10000];
                            do
                            {
                                ledred = 0; ledgreen = 0; ledblue = 0;
                                ledresult2 = measvol2(LEDadd, 10000);
                                for (int ledcounti = 0; ledcounti < 10000; ledcounti++)
                                {
                                    if ((ledresult2[ledcounti] >= 1.25) && (ledresult2[ledcounti] <= 1.8)) { ledgreen++; }
                                    if ((ledresult2[ledcounti] >= 2.48) && (ledresult2[ledcounti] <= 3.5)) { ledred++; }
                                    if ((ledresult2[ledcounti] >= 0.7) && (ledresult2[ledcounti] <= 1.2)) { ledblue++; }
                                }
                                //   MessageBox.Show("Green :" + ledgreen.ToString());
                                ledtestcount++;
                                Delay(5);
                            } while ((ledtestcount<50)&&(ledgreen<100)&&(ledred<100)&&(ledblue<100));
                            if ((ledgreen >100) && (ledred <10)&&(ledblue < 10)) { ledact = "Green"; listBox1.Items.Add("LED :" + ledact + " " ); dataGridView1.Rows[i].Cells[6].Value = ledact; }
                            if ((ledred > 100) && (ledgreen < 10) && (ledblue < 10)) { ledact = "Red"; listBox1.Items.Add("LED :" + ledact + " " ); dataGridView1.Rows[i].Cells[6].Value = ledact; }
                            if ((ledblue > 100) && (ledgreen < 10) && (ledred < 10)) { ledact = "Blue"; listBox1.Items.Add("LED :" + ledact + " " ); dataGridView1.Rows[i].Cells[6].Value = ledact; }
                            if (led != ledact) { pass = 0; passitem = 0;  }
                            if ((ledact != "Green") && (ledact != "Red") && (ledact != "Blue")) { dataGridView1.Rows[i].Cells[6].Value = "Err"; listBox1.Items.Add("LED :Err"); }
                            /*   ledresult = measvol(LEDadd);
                            if (ledresult == -1000.0) { ErrBox.Items.Add("Meas_LED_Vol err"); pass = 0; dataGridView1.Rows[i].Cells[6].Value = ledact; listBox1.Items.Add("LED :Err");passitem = 0; }                           
                            if ((ledresult >= 1.25) && (ledresult <= 1.8)) { ledact = "Green"; listBox1.Items.Add("LED :" + ledact+" " + ledresult.ToString()); dataGridView1.Rows[i].Cells[6].Value = ledact; }
                            if ((ledresult >= 2.48) && (ledresult <= 3.5)) { ledact = "Red"; listBox1.Items.Add("LED :" + ledact + " " + ledresult.ToString()); dataGridView1.Rows[i].Cells[6].Value = ledact; }
                            if ((ledresult >= 0.7) && (ledresult <= 1.2)) { ledact = "Blue"; listBox1.Items.Add("LED :" + ledact + " " + ledresult.ToString()); dataGridView1.Rows[i].Cells[6].Value = ledact; }
                            if (ledresult >3.5) { ErrBox.Items.Add("LED_Vol>3.5"); pass = 0; dataGridView1.Rows[i].Cells[6].Value = ledact; listBox1.Items.Add("LED :Err "+ ledresult.ToString()); passitem = 0; }
                            if ((ledresult <0.7)&&(ledresult>-1000)) { ErrBox.Items.Add("LED_Vol<0.7"); pass = 0; dataGridView1.Rows[i].Cells[6].Value = "<0.7"; listBox1.Items.Add("LED :Err "+ ledresult.ToString()); passitem = 0; }
                            if (led != ledact) { pass = 0;  passitem = 0; listBox1.Items.Add("LED Vol :" + ledresult.ToString()); }
                            if((ledact!="Green")&& (ledact != "Red")&& (ledact != "Blue")) { dataGridView1.Rows[i].Cells[6].Value = "Err"; listBox1.Items.Add("LED :Err" ); }
                            */
                        }
                        if (led == "") { dataGridView1.Rows[i].Cells[6].Value = "None"; listBox1.Items.Add("LED :Don't Care"); }

                        if (CoilNumber!="0")
                        {
                            GetPrivateProfileString(item1, "NC1", "", temp, 500, ff);nc1 = temp.ToString();
                            GetPrivateProfileString(item1, "NO2", "", temp, 500, ff);no2 = temp.ToString();
                            GetPrivateProfileString(item1, "NC3", "", temp, 500, ff); nc3 = temp.ToString();
                            GetPrivateProfileString(item1, "NO4", "", temp, 500, ff); no4 = temp.ToString();

                            if (nc1 != "")
                            {
                                Delay(dtime);
                                nc1data = measvol(NC1add);
                                listBox1.Items.Add("NC1 :" + nc1data.ToString("#0.00"));
                                if (nc1data > 4.5) { nc1act = "OFF"; }
                                if ((nc1data <1)&&(nc1data>=0)) { nc1act = "ON";  }
                                if ((nc1data >= 1)&&(nc1data<=4.5)) { nc1act = "ERR"; }
                                if (nc1data <0) { nc1act = "ERR"; }
                                dataGridView1.Rows[i].Cells[2].Value = nc1act;
                                if (nc1 != nc1act) { pass = 0;passitem = 0; }
                            }
                            if (nc1 == "") { dataGridView1.Rows[i].Cells[2].Value = ""; listBox1.Items.Add("NC1 :Don't Care"); }
                            if (no2 != "")
                            {
                                Delay(dtime);
                                no2data = measvol(NO2add);
                                listBox1.Items.Add("NO2 :" + no2data.ToString("#0.00"));
                                if (no2data > 4.5) { no2act = "OFF"; }
                                if ((no2data < 1.4) && (no2data >= 0)) { no2act = "ON"; }
                                if ((no2data >= 1.4) && (no2data <= 4.5)) { no2act = "ERR"; }
                                if (no2data < 0) { no2act = "ERR"; }
                                dataGridView1.Rows[i].Cells[3].Value = no2act;
                                if (no2 != no2act) { pass = 0; passitem = 0; }
                            }
                            if (no2 == "") { dataGridView1.Rows[i].Cells[3].Value = ""; listBox1.Items.Add("NO2 :Don't Care"); }

                            if (nc3 != "")
                            {
                                Delay(dtime);
                                nc3data = measvol(NC3add);
                                listBox1.Items.Add("NC3 :" + nc3data.ToString("#0.00"));
                                if (nc3data > 4.5) { nc3act = "OFF"; }
                                if ((nc3data < 1) && (nc3data >= 0)) { nc3act = "ON"; }
                                if ((nc3data >= 1) && (nc3data <= 4.5)) { nc3act = "ERR"; }
                                if (nc3data < 0) { nc3act = "ERR"; }
                                dataGridView1.Rows[i].Cells[4].Value = nc3act;
                                if (nc3 != nc3act) { pass = 0; passitem = 0; }
                            }
                            if (nc3 == "") { dataGridView1.Rows[i].Cells[4].Value = ""; listBox1.Items.Add("NC3 :Don't Care"); }

                            if (no4 != "")
                            {
                                Delay(dtime);
                                //MessageBox.Show("33333");
                                no4data = measvol(NO4add);
                                listBox1.Items.Add("NO4 :" + no4data.ToString("#0.00"));
                                if (no4data > 4.5) { no4act = "OFF"; }
                                if ((no4data < 1) && (no4data >= 0)) { no4act = "ON"; }
                                if ((no4data >= 1) && (no4data <= 4.5)) { no4act = "ERR"; }
                                if (no4data < 0) { no4act = "ERR"; }
                                dataGridView1.Rows[i].Cells[5].Value = no4act;
                                if (no4 != no4act) { pass = 0; passitem = 0; }
                            }
                            if (no4 == "") { dataGridView1.Rows[i].Cells[5].Value = ""; listBox1.Items.Add("NO4 :Don't Care"); }
                        }//CoilNumber=="4"Zx

                      



                        if (passitem==0) {dataGridView1.Rows[i].Cells[28].Value = "FAIL";this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Red;}
                        if (passitem == 1) { dataGridView1.Rows[i].Cells[28].Value = "PASS";}
                        item1 = "Item"; passitem = 1;





                    }//flag true



                    listBox1.Items.Add(""); listBox1.Items.Add("");listBox1.Items.Add("");
                    // savedata = savedata + snno+",";
                    for (int ll = 0; ll < 26; ll++)
                    {
                        if ((dataGridView1.Rows[i].Cells[ll + 2].Value != "")&&(dataGridView1.Rows[i].Cells[ll + 2].Value != "None"))
                        { savedata = savedata + dataGridView1.Rows[i].Cells[ll + 2].Value + ","; }
                    }

                }

                sendData = null;
                sendData = Encoding.UTF8.GetBytes("VSET1:0");
                power1sp.Write(sendData, 0, sendData.Length); 
              Delay(10); outport("Dev2/port0", 0x1F);
                errinfo = sendcomm(sp, "ffaa030800000000b4");
                if (errinfo != "") { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                if (stop == 0)
                {
                    savefile = new FileStream(savepath, FileMode.Append);
                    sw3 = new StreamWriter(savefile);
                    if (pass == 1) { savedata = savedata + "PASS" + "\r\n";  }
                    if (pass == 0)
                    {
                        savedata = savedata + "FAIL" + "\r\n"; passcheck = new FileStream("C:\\Windows\\System32\\dcheck.txt", FileMode.Truncate);
                        StreamWriter checkok = new StreamWriter(passcheck); checkok.Write("FAIL"); checkok.Flush(); checkok.Close(); passcheck.Close();
                    }
                    savedata = snno +"," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "," + savedata;
                    sw3.Write(savedata); sw3.Flush(); sw3.Close(); savefile.Close();
                }
                if ((pass == 1)&&(stop==0)){ outport("Dev2/port0", 0x17); Delay(700); outport("Dev2/port0", 0x1F); this.Statue.ForeColor = Color.Green; this.Statue.Text = "PASS";}
                if ((pass == 0)&&(stop==0)) { this.Statue.ForeColor = Color.Red; this.Statue.Text = "FAIL"; Password pwprd = new Password(); DialogResult ddr = pwprd.ShowDialog(); }
                int listcount = 0,li;
                string listbuf="",listbuf2="";
                FileStream Debuginfo;
                StreamWriter sw4;
                //MessageBox.Show("d:\\" + snno + "-debuginfo.txt");
                string snno1;
                snno1=snno.Replace(":", "_");
                Debuginfo = new FileStream("d:\\"+snno1+"-debuginfo.txt", FileMode.Create);
                sw4 = new StreamWriter(Debuginfo);
                listcount = listBox1.Items.Count;
                for(li=0;li<listcount;li++)
                {
                    listbuf2 = listBox1.Items[li].ToString();
                    listbuf = listbuf +listbuf2+ "\r\n";
                }
                sw4.Write(listbuf); sw4.Flush(); sw4.Close(); Debuginfo.Close();
                sw.Stop();label2.Text ="TestTime:"+sw.ElapsedMilliseconds.ToString()+"MS";
                if (checkBox1.Checked == true) { Test.PerformClick(); }
            }//listbox false
            else
            {
                // MessageBox.Show("Mid2");
                mydelegate mytest = new mydelegate(Thread_Test);              
                listBox1.BeginInvoke(mytest);
              
            }




        }

        void Thread_STest()
        {
            StringBuilder temp = new StringBuilder(500);
            int i, i2, i3, i4, i5, fx, passitem = 1;
            uint dtime;
            string canerrinfo, item1 = "Item", seqq = "SEQ", vbat,vbat2,v34, errinfo = "", distance, dishex, led = "", ledact = "", nc1, no2, nc3, no4, nc1act = "", no2act = "", nc3act = "", no4act = "", ccid = "", crid = "", cclength = "0", crlength = "0", ccdata = "",dcr1act="",dcr2act="";
            byte[] sendData = null;
            double ledresult, nc1data, no2data, nc3data, no4data;
            if (listBox1.InvokeRequired == false)
            {
                i = dataGridView1.CurrentRow.Index;item1 = item1 + Seq[i].ToString();
                GetPrivateProfileString(item1, "Item", "N/A", temp, 500, ff);
                listBox1.Items.Add(temp.ToString());
                dataGridView1.Rows[i].Cells[1].Value = temp.ToString();
                GetPrivateProfileString(item1, "Delay", "500", temp, 500, ff);
                dtime = uint.Parse(temp.ToString());
                GetPrivateProfileString(item1, "Vbat", "0", temp, 500, ff);
                vbat = temp.ToString();
                listBox1.Items.Add("Vbat=" + vbat);
                if (vbat != "0")
                {
                    sendData = Encoding.UTF8.GetBytes("VSET1:" + vbat);
                    power1sp.Write(sendData, 0, sendData.Length);
                    Delay(60);
                    sendData = null;
                    sendData = Encoding.UTF8.GetBytes("ISET1:3");
                    power1sp.Write(sendData, 0, sendData.Length);
                    Delay(60);
                    sendData = null;
                    sendData = Encoding.UTF8.GetBytes("OUT1");
                    power1sp.Write(sendData, 0, sendData.Length);
                    Delay(60);
                    sendData = null;
                    sendData = Encoding.UTF8.GetBytes("VOUT1?");
                    power1sp.Write(sendData, 0, sendData.Length);
                    Delay(60);
                }
                sendData = null;
                GetPrivateProfileString(item1, "V34", "0", temp, 500, ff);
                v34 = temp.ToString();
                listBox1.Items.Add("V34=" + v34);
                if (v34 != "0")
                {
                    sendData = Encoding.UTF8.GetBytes("VSET1:" + v34);
                    power1sp.Write(sendData, 0, sendData.Length);
                    Delay(60);
                    sendData = null;
                    sendData = Encoding.UTF8.GetBytes("ISET1:3");
                    power1sp.Write(sendData, 0, sendData.Length);
                    Delay(60);
                    sendData = null;
                    sendData = Encoding.UTF8.GetBytes("OUT1");
                    power1sp.Write(sendData, 0, sendData.Length);
                    Delay(60);
                    sendData = null;
                    sendData = Encoding.UTF8.GetBytes("VOUT1?");
                    power1sp.Write(sendData, 0, sendData.Length);
                    Delay(60);
                }
                if ((vbat == "0") && (v34 == "0") && (MODEL.Text.IndexOf("CLS")) == -1)
                { errinfo = outport("Dev1/port0", 0x1F); }
                if ((vbat != "0") && (v34 != "0") && (MODEL.Text.IndexOf("CLS")) == -1)
                { errinfo = outport("Dev1/port0", 0x1A); }
                if ((vbat != "0") && (v34 == "0") && (MODEL.Text.IndexOf("CLS")) == -1)
                { errinfo = outport("Dev1/port0", 0x1E); }
                if ((vbat == "0") && (v34 != "0") && (MODEL.Text.IndexOf("CLS")) == -1)
                { errinfo = outport("Dev1/port0", 0x1B); }
                if ((vbat != "0") && (v34 == "0") && (MODEL.Text.IndexOf("0118-CLS")) != -1)
                { errinfo = outport("Dev3/port0", 0x1F); }
                if ((vbat != "0") && (v34 == "0") && (MODEL.Text.IndexOf("0123-CLS")) != -1)
                { errinfo = outport("Dev3/port0", 0x19); }
                if (errinfo != "") { ErrBox.Items.Add("Daqoutport Err:" + errinfo); }
                sendData = null;
                GetPrivateProfileString(item1, "Vbat2", "0", temp, 500, ff);
                vbat2 = temp.ToString();
                listBox1.Items.Add("Vbat2=" + vbat2);
                if (vbat2 != "0")
                {
                    sendData = Encoding.UTF8.GetBytes("VSET1:" + vbat);
                    power2sp.Write(sendData, 0, sendData.Length);
                    Delay(60);
                    sendData = null;
                    sendData = Encoding.UTF8.GetBytes("ISET1:2");
                    power2sp.Write(sendData, 0, sendData.Length);
                    Delay(60);
                    sendData = null;
                    sendData = Encoding.UTF8.GetBytes("OUT1");
                    power2sp.Write(sendData, 0, sendData.Length);
                    Delay(60);
                    sendData = null;
                    sendData = Encoding.UTF8.GetBytes("VOUT1?");
                    power2sp.Write(sendData, 0, sendData.Length);
                    Delay(60);
                }
                sendData = null;

                GetPrivateProfileString(item1, "Distance", "0", temp, 500, ff);
                distance = temp.ToString();
                if (int.Parse(distance) > 0)
                {
                    errinfo = sendcomm(sp, "FFAA030401010000B2");
                    if (errinfo != "") { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                    dishex = motor_Dis(distance);
                    errinfo = sendcomm(sp, dishex);
                    if (errinfo != "") { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                    errinfo = sendcomm(sp, "FFAA030900000000B5");
                    if (errinfo != "") { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                }
                if (int.Parse(distance) < 0)
                {
                    errinfo = sendcomm(sp, "FFAA030400010000B1");
                    if (errinfo != "") { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                    distance = (0 - int.Parse(distance)).ToString();
                    //   MessageBox.Show(distance);
                    dishex = motor_Dis(distance);
                    errinfo = sendcomm(sp, dishex);
                    if (errinfo != "") { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                    errinfo = sendcomm(sp, "FFAA030900000000B5");
                    if (errinfo != "") { ErrBox.Items.Add("StepMotor Com Err:" + errinfo); }
                }
                if (int.Parse(distance) != 0) { Delay(dtime); }
                string sendcandata = "", recivecandata = "",str1buf, lvdt = "", lvdtmax = "", lvdtmin = "",dcr1="",dcr2="",dcr1max="",dcr1min="",dcr2max="",dcr2min="";
                int tlen = 0, hexTooct = 0, ican = 0,ind,ibuf;
              //  float canfresult=0;
                Ecan.CAN_OBJ sendbusinfo = new Ecan.CAN_OBJ(), recbusinfo = new Ecan.CAN_OBJ();
                GetPrivateProfileString(item1, "CCID", "", temp, 500, ff); ccid = temp.ToString();
                GetPrivateProfileString(item1, "CRID", "", temp, 500, ff); crid = temp.ToString();
                string[] seqcrid, crinfoid;
                Ecan.CAN_OBJ[] recbusinfo2;
                int cridcount = 0, px = 0;
                if (crid != "")
                {
                    seqcrid = crid.ToString().Split(',');
                    //int xx = 0;
                    foreach (string azu in seqcrid) { cridcount++; }
                    crinfoid = new string[cridcount];
                    foreach (string azu in seqcrid) { crinfoid[px] = azu; px++; }
                }
                else { crinfoid = new string[cridcount + 1]; }
                recbusinfo2 = new Ecan.CAN_OBJ[cridcount + 1];
                //  MessageBox.Show(cridcount.ToString());
                if (ccid == "FF")
                {
                    canerrinfo = canreceive2(out recbusinfo2, cridcount);
                    // MessageBox.Show(recbusinfo2[0].ID.ToString());
                    // MessageBox.Show(recbusinfo2[1].ID.ToString());
                    for (int pfx = 0; pfx < cridcount; pfx++)
                    //  canerrinfo = cansend(sendbusinfo, out recbusinfo);
                    {
                        if (canerrinfo != "") { ErrBox.Items.Add(canerrinfo); }
                        if (canerrinfo == "")
                        {
                            ErrBox.Items.Add("CanRecID :" + recbusinfo2[pfx].ID.ToString("X"));

                        }
                    }
                }

                if ((ccid != "") && (ccid != "FF"))
                {
                    GetPrivateProfileString(item1, "CCLENGTH", "0", temp, 500, ff); cclength = temp.ToString();
                    sendbusinfo.SendType = 0; sendbusinfo.data = new byte[8]; sendbusinfo.Reserved = new byte[3]; sendbusinfo.RemoteFlag = 0; sendbusinfo.ExternFlag = 1;
                    sendbusinfo.DataLen = Convert.ToByte(int.Parse(cclength));
                    GetPrivateProfileString(item1, "CCDATA", "", temp, 500, ff); ccdata = temp.ToString();
                    tlen = sendbusinfo.DataLen - 1;
                    for (ican = 0; ican <= tlen; ican++)
                    { sendbusinfo.data[ican] = Convert.ToByte(ccdata.Substring(0 + ican * 2, 2), 0X10); sendcandata = sendcandata + sendbusinfo.data[ican].ToString("X2") + " ";
                    }
                    sendbusinfo.ID = Convert.ToUInt32(ccid, 16);
                    /*  sendbusinfo.data[0] = Convert.ToByte("01", 0X10);
                      sendbusinfo.data[1] = Convert.ToByte("01", 0X10);
                      sendbusinfo.data[2] = Convert.ToByte("02", 0X10);
                      sendbusinfo.data[3] = Convert.ToByte("01", 0X10);
                      sendbusinfo.ID = Convert.ToUInt32("4012010", 16);*/
                    listBox1.Items.Add("CCID :" + sendbusinfo.ID.ToString("X")); listBox1.Items.Add("CCDATA :" + sendcandata);
                    GetPrivateProfileString(item1, "Canmodel", "", temp, 500, ff); canmodel = temp.ToString();
                    // canerrinfo = cansend(sendbusinfo, out recbusinfo);
                    canerrinfo = cansend2(sendbusinfo, out recbusinfo2, cridcount);
                    for (int pfx = 0; pfx < cridcount; pfx++)
                    {
                        if (canerrinfo != "") { ErrBox.Items.Add(canerrinfo); }
                        if (canerrinfo == "")
                        {

                            ErrBox.Items.Add("CanRecID :" + recbusinfo2[pfx].ID.ToString("X"));

                        }
                    }
                }
                string actidstr;
                actidstr = "Act CRID:";
                for (int pfx = 0; pfx < cridcount; pfx++)
                { actidstr = actidstr + " " + recbusinfo2[pfx].ID.ToString("X");
                }
                listBox1.Items.Add(actidstr);
                float canfresult = 0;
                //   recivecandata = "";
                string cistatue = "", LVDTmax = "", LVDTmin = "", LVDTstatue = "xxxx";
                string[] seq, seqx, seq2, swv;
                seqx = new string[8];
                swv = new string[3];
                int xx = 0, pp = 0, idright = 0, pf = 0, pfxx = 0;
                //  MessageBox.Show(cridcount.ToString());
                if (crid != "")
                {
                    for (pfxx = 0; pfxx < cridcount; pfxx++)
                    {
                        idright = 0;
                        pf = 0;
                        //  MessageBox.Show(cridcount.ToString());
                        do
                        {
                            //  MessageBox.Show(pf.ToString());
                            if (crinfoid[pfxx].ToUpper() == recbusinfo2[pf].ID.ToString("X").ToUpper())
                            {
                                idright = 1;
                                tlen = recbusinfo2[pf].DataLen - 1;
                                for (ican = 0; ican <= tlen; ican++)
                                { recivecandata = recivecandata + recbusinfo2[pf].data[ican].ToString("X2") + " ";
                                }
                                listBox1.Items.Add("CRID :" + recbusinfo2[pf].ID.ToString("X")); listBox1.Items.Add("CRDATA :" + recivecandata);
                                GetPrivateProfileString(item1, "LVDTMAX", "", temp, 500, ff); lvdtmax = temp.ToString();
                                GetPrivateProfileString(item1, "LVDTMIN", "", temp, 500, ff); lvdtmin = temp.ToString();
                                if (((lvdtmax != "") || (lvdtmin != "")) && (MODEL.Text.IndexOf("CI") != -1))
                                {
                                    GetPrivateProfileString(item1, "Canmodel", "", temp, 500, ff); canmodel = temp.ToString();
                                    if (canmodel == "SW version")
                                    {
                                        seq = lvdtmax.ToString().Split(',');
                                        //int xx = 0;
                                        foreach (string azu in seq) { seqx[xx] = azu; xx++; }

                                        for (pp = 0; pp < 3; pp++) { swv[pp] = seqx[pp]; }
                                        for (pp = 0; pp < 3; pp++)
                                        {
                                            if (recbusinfo2[pf].data[pp + 2].ToString("X2") != swv[pp]) { pass = 0; passitem = 0; }
                                            dataGridView1.Rows[i].Cells[7].Value = dataGridView1.Rows[i].Cells[7].Value + recbusinfo2[pf].data[pp + 2].ToString("X2") + " "; listBox1.Items.Add("SW Verion :" + recbusinfo2[pf].data[pp + 2].ToString("X2") + " Spec :" + swv[pp]);
                                        }

                                    }
                                    if (canmodel == "HW Status")
                                    {
                                        Delay(dtime);
                                        if (recbusinfo2[pf].data[0].ToString("X2") != lvdtmax) { pass = 0; passitem = 0; }
                                        dataGridView1.Rows[i].Cells[7].Value = recbusinfo2[pf].data[0].ToString("X2"); listBox1.Items.Add("HW Status :" + recbusinfo2[pf].data[0].ToString("X2"));
                                    }
                                    if (canmodel == "D2 5V")
                                    {
                                        lvdt = recbusinfo2[pf].data[7].ToString("X2") + recbusinfo2[pf].data[6].ToString("X2"); hexTooct = hextodec(lvdt);
                                        if ((hexTooct > int.Parse(lvdtmax)) || (hexTooct < int.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                        dataGridView1.Rows[i].Cells[7].Value = hexTooct.ToString(); listBox1.Items.Add("D2 5V :" + hexTooct.ToString());

                                    }

                                    /*seq = lvdtmax.ToString().Split(',');

                                foreach(string azu in seq) { seqx[xx] = azu;xx++; }
                                lvdtmax = seqx[1]; LVDTstatue = seqx[0];xx = 0; seq = lvdtmin.ToString().Split(',');
                                foreach (string azu in seq) { seqx[xx] = azu; xx++; }
                                lvdtmin = seqx[1];
                                if (recbusinfo.data[2].ToString("X2") == "30") { cistatue = "blocked"; }
                                if (recbusinfo.data[2].ToString("X2") == "33") { cistatue = "floating"; }
                                if (recbusinfo.data[2].ToString("X2") == "31") { cistatue = "retracted";hexTooct= int.Parse(recbusinfo.data[0].ToString("X2"), System.Globalization.NumberStyles.HexNumber)-128; }
                                if (recbusinfo.data[2].ToString("X2") == "32") { cistatue = "extended"; hexTooct = -(int.Parse(recbusinfo.data[1].ToString("X2"), System.Globalization.NumberStyles.HexNumber) - 128); }
                                if (cistatue!=LVDTstatue) { pass = 0; passitem = 0; }
                                dataGridView1.Rows[i].Cells[7].Value = cistatue; listBox1.Items.Add("LVDTstatue :" + cistatue);
                                if ((hexTooct > int.Parse(lvdtmax)) || (hexTooct < int.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                dataGridView1.Rows[i].Cells[7].Value = hexTooct.ToString(); listBox1.Items.Add("LVDT :" + hexTooct.ToString());
*/
                                }
                                if (((lvdtmax != "") || (lvdtmin != "")) && (MODEL.Text.IndexOf("CI") == -1))
                                {
                                    lvdt = recbusinfo2[pf].data[1].ToString("X2") + recbusinfo2[pf].data[0].ToString("X2"); hexTooct = hextodec(lvdt);
                                    if ((hexTooct > int.Parse(lvdtmax)) || (hexTooct < int.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                    dataGridView1.Rows[i].Cells[7].Value = hexTooct.ToString(); listBox1.Items.Add("LVDT :" + hexTooct.ToString());
                                }
                                for (ibuf = 1; ibuf < 9; ibuf++)
                                {
                                    str1buf = "AD" + ibuf.ToString() + "MAX"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmax = temp.ToString();
                                    str1buf = "AD" + ibuf.ToString() + "MIN"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmin = temp.ToString();
                                    if ((lvdtmax != "") || (lvdtmin != ""))
                                    {
                                        if (ibuf % 2 != 0) { lvdt = recbusinfo2[pf].data[ibuf].ToString("X2") + recbusinfo2[pf].data[ibuf - 1].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f / 1000; }
                                        if (ibuf % 2 == 0) { lvdt = recbusinfo2[pf].data[ibuf - 1].ToString("X2") + recbusinfo2[pf].data[ibuf - 2].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f / 1000; }
                                        if ((canfresult > float.Parse(lvdtmax)) || (canfresult < float.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                        dataGridView1.Rows[i].Cells[8 + ibuf - 1].Value = canfresult.ToString(); str1buf = "AD" + ibuf.ToString() + " :"; listBox1.Items.Add(str1buf + canfresult.ToString());
                                    }
                                }
                                for (ibuf = 1; ibuf < 5; ibuf++)
                                {
                                    str1buf = "ILS" + ibuf.ToString() + "MAX"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmax = temp.ToString();
                                    str1buf = "ILS" + ibuf.ToString() + "MIN"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmin = temp.ToString();
                                    if ((lvdtmax != "") || (lvdtmin != ""))
                                    {
                                        if (ibuf % 2 != 0) { lvdt = recbusinfo2[pf].data[ibuf].ToString("X2") + recbusinfo2[pf].data[ibuf - 1].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f; }
                                        if (ibuf % 2 == 0) { lvdt = recbusinfo2[pf].data[ibuf - 1].ToString("X2") + recbusinfo2[pf].data[ibuf - 2].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f; }
                                        if ((canfresult > float.Parse(lvdtmax)) || (canfresult < float.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                        dataGridView1.Rows[i].Cells[16 + ibuf - 1].Value = canfresult.ToString(); str1buf = "ILS" + ibuf.ToString() + " :"; listBox1.Items.Add(str1buf + canfresult.ToString());
                                    }
                                }
                                GetPrivateProfileString(item1, "IHS1MAX", "", temp, 500, ff); lvdtmax = temp.ToString();
                                GetPrivateProfileString(item1, "IHS1MIN", "", temp, 500, ff); lvdtmin = temp.ToString();
                                if ((lvdtmax != "") || (lvdtmin != ""))
                                {
                                    lvdt = recbusinfo2[pf].data[5].ToString("X2") + recbusinfo2[pf].data[4].ToString("X2"); hexTooct = hextodec(lvdt);
                                    if ((hexTooct > int.Parse(lvdtmax)) || (hexTooct < int.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                    dataGridView1.Rows[i].Cells[20].Value = hexTooct.ToString(); listBox1.Items.Add("ISH1 :" + hexTooct.ToString());
                                }
                                for (ibuf = 1; ibuf < 3; ibuf++)
                                {
                                    str1buf = "FIN" + ibuf.ToString() + "DCMAX"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmax = temp.ToString();
                                    str1buf = "FIN" + ibuf.ToString() + "DCMIN"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmin = temp.ToString();
                                    if ((lvdtmax != "") || (lvdtmin != ""))
                                    {
                                        if (ibuf == 1) { lvdt = recbusinfo2[pf].data[1].ToString("X2") + recbusinfo2[pf].data[0].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f / 100; }
                                        if (ibuf == 2) { lvdt = recbusinfo2[pf].data[5].ToString("X2") + recbusinfo2[pf].data[4].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f / 100; }
                                        if ((canfresult > float.Parse(lvdtmax)) || (canfresult < float.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                        dataGridView1.Rows[i].Cells[21 + ibuf - 1].Value = canfresult.ToString(); str1buf = "FIN" + ibuf.ToString() + "DC :"; listBox1.Items.Add(str1buf + canfresult.ToString());
                                    }
                                }
                                for (ibuf = 1; ibuf < 3; ibuf++)
                                {
                                    str1buf = "FIN" + ibuf.ToString() + "FREQMAX"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmax = temp.ToString();
                                    str1buf = "FIN" + ibuf.ToString() + "FREQMIN"; GetPrivateProfileString(item1, str1buf, "", temp, 500, ff); lvdtmin = temp.ToString();
                                    if ((lvdtmax != "") || (lvdtmin != ""))
                                    {
                                        if (ibuf == 1) { lvdt = recbusinfo2[pf].data[3].ToString("X2") + recbusinfo2[pf].data[2].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f ; }
                                        if (ibuf == 2) { lvdt = recbusinfo2[pf].data[7].ToString("X2") + recbusinfo2[pf].data[6].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f ; }
                                        if ((canfresult > float.Parse(lvdtmax)) || (canfresult < float.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                        dataGridView1.Rows[i].Cells[23 + ibuf - 1].Value = canfresult.ToString(); str1buf = "FIN" + ibuf.ToString() + "FREQ :"; listBox1.Items.Add(str1buf + canfresult.ToString());
                                    }
                                }
                                GetPrivateProfileString(item1, "KEYMAX", "", temp, 500, ff); lvdtmax = temp.ToString();
                                GetPrivateProfileString(item1, "KEYMIN", "", temp, 500, ff); lvdtmin = temp.ToString();
                                if ((lvdtmax != "") || (lvdtmin != ""))
                                {
                                    lvdt = recbusinfo2[pf].data[5].ToString("X2") + recbusinfo2[pf].data[4].ToString("X2"); hexTooct = hextodec(lvdt); canfresult = hexTooct * 1.0f / 1000;
                                    if ((canfresult > float.Parse(lvdtmax)) || (canfresult < float.Parse(lvdtmin))) { pass = 0; passitem = 0; }
                                    dataGridView1.Rows[i].Cells[25].Value = canfresult.ToString(); listBox1.Items.Add("KEYVOL :" + canfresult.ToString());
                                }

                            }//crid.ToUpper()== recbusinfo.ID.ToString("X").ToUpper()
                             //   MessageBox.Show(pf.ToString());

                            pf++;
                        } while (pf < cridcount);
                        if (idright == 0)
                        {

                            listBox1.Items.Add("CRID : " + crinfoid[pfxx].ToUpper()+"don't exit");
                            
                   pass = 0; passitem = 0; dataGridView1.Rows[i].Cells[7].Value = "ID ERR";
                        }
                    }

                }//crid!=""
                 //  }
                if (crid == ""){ dataGridView1.Rows[i].Cells[7].Value = ""; }

                    double dcrr;
                int cmi,ind2;
                GetPrivateProfileString(item1, "DCR1MAX", "", temp, 500, ff); dcr1max = temp.ToString();
                GetPrivateProfileString(item1, "DCR1MIN", "", temp, 500, ff); dcr1min = temp.ToString();
                if ((dcr1max != "") || (dcr1min != ""))
                {
                    if ((vbat != "0") && (v34 != "0")) { outport("Dev1/port0", 0x12); }
                    if ((vbat != "0") && (v34 == "0")) { outport("Dev1/port0", 0x16); }
                    if ((vbat == "0") && (v34 == "0")) { outport("Dev1/port0", 0x17); }
                    if ((vbat == "0") && (v34 != "0")) { outport("Dev1/port0", 0x13); }
                    Delay(dtime); dcr1 = measres(DCRADD); ind = dcr1.IndexOf("E"); ind2 = dcr1.IndexOf("\n");
                    cmi = int.Parse(dcr1.Substring(ind + 1, ind2 - ind - 1));
                    dcrr = double.Parse(dcr1.Substring(1, ind-1)) * System.Math.Pow(10, cmi);
                    if ((dcrr-0.8> float.Parse(dcr1max)) || (dcrr-0.8 < float.Parse(dcr1min)))
                    { pass = 0; passitem = 0; }
                    dataGridView1.Rows[i].Cells[8].Value = (dcrr-0.8).ToString("#0.000"); listBox1.Items.Add("DCR1 :" + dcr1);
                }
                GetPrivateProfileString(item1, "DCR2MAX", "", temp, 500, ff); dcr2max = temp.ToString();
                GetPrivateProfileString(item1, "DCR2MIN", "", temp, 500, ff); dcr2min = temp.ToString();
                if ((dcr2max != "") || (dcr2min != ""))
                {
                    if ((vbat != "0") && (v34 != "0")) { outport("Dev1/port0", 0x0A); }
                    if ((vbat != "0") && (v34 == "0")) { outport("Dev1/port0", 0x0E); }
                    if ((vbat == "0") && (v34 == "0")) { outport("Dev1/port0", 0x0F); }
                    if ((vbat == "0") && (v34 != "0")) { outport("Dev1/port0", 0x0B); }
                    Delay(dtime); dcr2 = measres(DCRADD); ind = dcr2.IndexOf("E"); ind2 = dcr2.IndexOf("\n");
                    cmi = int.Parse(dcr2.Substring(ind + 1, ind2 - ind - 1));
                    dcrr = double.Parse(dcr2.Substring(1, ind-1)) * System.Math.Pow(10, cmi);
                    if ((dcrr-0.8 > float.Parse(dcr2max)) || (dcrr-0.8 < float.Parse(dcr2min)))
                    { pass = 0; passitem = 0; }
                    dataGridView1.Rows[i].Cells[9].Value = (dcrr-0.8).ToString("#0.000"); listBox1.Items.Add("DCR2 :" + dcr2);
                }


                GetPrivateProfileString(item1, "LED", "", temp, 500, ff);
                led = temp.ToString();
                double[] ledresult2;
                int ledred = 0, ledgreen = 0, ledblue = 0, ledtestcount = 0;
                if (led != "")
                {
                    Delay(dtime);
                    ledresult2 = new double[10000];
                    do
                    {
                        ledred = 0; ledgreen = 0; ledblue = 0;
                        ledresult2 = measvol2(LEDadd, 10000);
                        for (int ledcounti = 0; ledcounti < 10000; ledcounti++)
                        {
                            if ((ledresult2[ledcounti] >= 1.25) && (ledresult2[ledcounti] <= 1.8)) { ledgreen++; }
                            if ((ledresult2[ledcounti] >= 2.48) && (ledresult2[ledcounti] <= 3.5)) { ledred++; }
                            if ((ledresult2[ledcounti] >= 0.7) && (ledresult2[ledcounti] <= 1.2)) { ledblue++; }
                        }
                        //   MessageBox.Show("Green :" + ledgreen.ToString());
                        ledtestcount++;
                        Delay(5);
                    } while ((ledtestcount < 50) && (ledgreen < 100) && (ledred < 100) && (ledblue < 100));
                    if ((ledgreen > 100) && (ledred < 10) && (ledblue < 10)) { ledact = "Green"; listBox1.Items.Add("LED :" + ledact + " "); dataGridView1.Rows[i].Cells[6].Value = ledact; }
                    if ((ledred > 100) && (ledgreen < 10) && (ledblue < 10)) { ledact = "Red"; listBox1.Items.Add("LED :" + ledact + " "); dataGridView1.Rows[i].Cells[6].Value = ledact; }
                    if ((ledblue > 100) && (ledgreen < 10) && (ledred < 10)) { ledact = "Blue"; listBox1.Items.Add("LED :" + ledact + " "); dataGridView1.Rows[i].Cells[6].Value = ledact; }
                    if (led != ledact) { pass = 0; passitem = 0; }
                    if ((ledact != "Green") && (ledact != "Red") && (ledact != "Blue")) { dataGridView1.Rows[i].Cells[6].Value = "Err"; listBox1.Items.Add("LED :Err"); }
                  /*  ledresult = measvol(LEDadd);
                    if (ledresult == -1000.0) { ErrBox.Items.Add("Meas_LED_Vol err"); pass = 0; dataGridView1.Rows[i].Cells[6].Value = ledact; listBox1.Items.Add("LED :Err"); passitem = 0; }
                    if ((ledresult >= 1.25) && (ledresult <= 1.8)) { ledact = "Green"; listBox1.Items.Add("LED :" + ledact + " " + ledresult.ToString()); dataGridView1.Rows[i].Cells[6].Value = ledact; }
                    if ((ledresult >= 2.48) && (ledresult <= 3.5)) { ledact = "Red"; listBox1.Items.Add("LED :" + ledact + " " + ledresult.ToString()); dataGridView1.Rows[i].Cells[6].Value = ledact; }
                    if ((ledresult >= 0.7) && (ledresult <= 1.2)) { ledact = "Blue"; listBox1.Items.Add("LED :" + ledact + " " + ledresult.ToString()); dataGridView1.Rows[i].Cells[6].Value = ledact; }
                    if (ledresult > 3.5) { ErrBox.Items.Add("LED_Vol>3.5"); pass = 0; dataGridView1.Rows[i].Cells[6].Value = ledact; listBox1.Items.Add("LED :Err"); passitem = 0; }
                    if ((ledresult < 0.7) && (ledresult > -1000)) { ErrBox.Items.Add("LED_Vol<0.7"); pass = 0; dataGridView1.Rows[i].Cells[6].Value = "<0.7"; listBox1.Items.Add("LED :Err"); passitem = 0; }
                    if (led != ledact) { pass = 0; passitem = 0; listBox1.Items.Add("LED Vol :" + ledresult.ToString()); }
                    if ((ledact != "Green") && (ledact != "Red") && (ledact != "Blue")) { dataGridView1.Rows[i].Cells[6].Value = "Err"; listBox1.Items.Add("LED :Err"); }
                    */
                }
                if (led == "") { dataGridView1.Rows[i].Cells[6].Value = "None"; listBox1.Items.Add("LED :Don't Care"); }



                if (CoilNumber != "0")
                {
                    GetPrivateProfileString(item1, "NC1", "", temp, 500, ff); nc1 = temp.ToString();
                    GetPrivateProfileString(item1, "NO2", "", temp, 500, ff); no2 = temp.ToString();
                    GetPrivateProfileString(item1, "NC3", "", temp, 500, ff); nc3 = temp.ToString();
                    GetPrivateProfileString(item1, "NO4", "", temp, 500, ff); no4 = temp.ToString();
                    if (nc1 != "")
                    {
                        Delay(dtime);
                        nc1data = measvol(NC1add);
                        listBox1.Items.Add("NC1 :" + nc1data.ToString("#0.00"));
                        if (nc1data > 4.5) { nc1act = "OFF"; }
                        if ((nc1data < 1) && (nc1data >= 0)) { nc1act = "ON"; }
                        if ((nc1data >= 1) && (nc1data <= 4.5)) { nc1act = "ERR"; }
                        if (nc1data < 0) { nc1act = "ERR"; }
                        dataGridView1.Rows[i].Cells[2].Value = nc1act;
                        if (nc1 != nc1act) { pass = 0; passitem = 0; }
                    }
                    if (nc1 == "") { dataGridView1.Rows[i].Cells[2].Value = ""; listBox1.Items.Add("NC1 :Don't Care"); }
                    if (no2 != "")
                    {
                        Delay(dtime);
                        no2data = measvol(NO2add);
                        listBox1.Items.Add("NO2 :" + no2data.ToString("#0.00"));
                        if (no2data > 4.5) { no2act = "OFF"; }
                        if ((no2data < 1.4) && (no2data >= 0)) { no2act = "ON"; }
                        if ((no2data >= 1.4) && (no2data <= 4.5)) { no2act = "ERR"; }
                        if (no2data < 0) { no2act = "ERR"; }
                        dataGridView1.Rows[i].Cells[3].Value = no2act;
                        if (no2 != no2act) { pass = 0; passitem = 0; }
                    }
                    if (no2 == "") { dataGridView1.Rows[i].Cells[3].Value = ""; listBox1.Items.Add("NO2 :Don't Care"); }

                    if (nc3 != "")
                    {
                        Delay(dtime);
                        nc3data = measvol(NC3add);
                        listBox1.Items.Add("NC3 :" + nc3data.ToString("#0.00"));
                        if (nc3data > 4.5) { nc3act = "OFF"; }
                        if ((nc3data < 1) && (nc3data >= 0)) { nc3act = "ON"; }
                        if ((nc3data >= 1) && (nc3data <= 4.5)) { nc3act = "ERR"; }
                        if (nc3data < 0) { nc3act = "ERR"; }
                        dataGridView1.Rows[i].Cells[4].Value = nc3act;
                        if (nc3 != nc3act) { pass = 0; passitem = 0; }
                    }
                    if (nc3 == "") { dataGridView1.Rows[i].Cells[4].Value = ""; listBox1.Items.Add("NC3 :Don't Care"); }

                    if (no4 != "")
                    {
                        Delay(dtime);
                        no4data = measvol(NO4add);
                        listBox1.Items.Add("NO4 :" + no4data.ToString("#0.00"));
                        if (no4data > 4.5) { no4act = "OFF"; }
                        if ((no4data < 1) && (no4data >= 0)) { no4act = "ON"; }
                        if ((no4data >= 1) && (no4data <= 4.5)) { no4act = "ERR"; }
                        if (no4data < 0) { no4act = "ERR"; }
                        dataGridView1.Rows[i].Cells[5].Value = no4act;
                        if (no4 != no4act) { pass = 0; passitem = 0; }
                    }
                    if (no4 == "") { dataGridView1.Rows[i].Cells[5].Value = ""; listBox1.Items.Add("NO4 :Don't Care"); }
                }//CoilNumber=="4"

              
                if (passitem == 0) { dataGridView1.Rows[i].Cells[28].Value = "FAIL"; this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Red; this.Statue.Text = "FAIL"; this.Statue.ForeColor = Color.Red; }
                if (passitem == 1) { dataGridView1.Rows[i].Cells[28].Value = "PASS"; this.Statue.Text = "PASS"; this.Statue.ForeColor = Color.Green; }
            }
            else
            {
                mydelegate mytest = new mydelegate(Thread_STest);
                listBox1.BeginInvoke(mytest);

            }






        }















    }
}

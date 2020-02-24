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

namespace Danfoss
{
    public partial class Equip : Form
    {
        public Equip()
        {
            InitializeComponent();
        }
        Dictionary<string, ComboBox> comboxList = new Dictionary<string, ComboBox>();
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        [DllImport(@"D:\GPIBDLL.dll", EntryPoint = "Gpiblist", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr list();

        private void Equip_Load(object sender, EventArgs e)
        {
            //  string[] ports = SerialPort.GetPortNames();
            StringBuilder temp = new StringBuilder(500);
            int i;
            FileStream fs;
            foreach (Control item in this.Controls)
            {
                if (item.GetType() == typeof(System.Windows.Forms.ComboBox) && item.Name.StartsWith("comboBox"))
                { comboxList.Add(item.Name, (ComboBox)item);}
            }
            string[] equ;
            IntPtr intPtr = list();
            string str = Marshal.PtrToStringAnsi(intPtr);
           // Delay(100);
            IntPtr intPtr2 = list();

            string str2 = Marshal.PtrToStringAnsi(intPtr2);
            equ = str2.Split(',');
            foreach (string azu in equ)
               // foreach (string port in ports)
            {
                for (i = 1; i < 5; i++)
                {
                    string actual_test = "comboBox" + i.ToString();
                    comboxList[actual_test].Items.Add(azu);
                }
            }
            if (!File.Exists("D:\\HardInfo.ini"))
            {

                fs = new FileStream("D:\\HardInfo.ini", FileMode.Create);
                StreamWriter checksn = new StreamWriter(fs);
                checksn.Flush();
                checksn.Close();
                fs.Close();
            }
            else
            {
                int index;
                for (i = 1; i < 10; i++)
                {
                    string actual_test = "comboBox" + i.ToString();
                    if (i == 1)
                    {
                        GetPrivateProfileString("HardInfo", "StepMotor", "0", temp, 500, "D:\\HardInfo.ini"); index = comboxList[actual_test].FindString(temp.ToString());
                        comboxList[actual_test].SelectedIndex = index;
                    }
                    if (i == 2)
                    {
                        GetPrivateProfileString("HardInfo", "Keithley", "0", temp, 500, "D:\\HardInfo.ini"); index = comboxList[actual_test].FindString(temp.ToString());
                        comboxList[actual_test].SelectedIndex = index;
                    }
                    if (i == 3)
                    {
                        GetPrivateProfileString("HardInfo", "Pow1", "0", temp, 500, "D:\\HardInfo.ini"); index = comboxList[actual_test].FindString(temp.ToString());
                        comboxList[actual_test].SelectedIndex = index;
                    }
                    if (i == 4)
                    {
                        GetPrivateProfileString("HardInfo", "Pow2", "0", temp, 500, "D:\\HardInfo.ini"); index = comboxList[actual_test].FindString(temp.ToString());
                        comboxList[actual_test].SelectedIndex = index;
                    }
                    if (i == 5)
                    {
                        GetPrivateProfileString("HardInfo", "NC1", "0", temp, 500, "D:\\HardInfo.ini"); comboxList[actual_test].Items.Add(temp.ToString());
                        comboxList[actual_test].SelectedIndex = 0;
                    }
                    if (i == 6)
                    {
                        GetPrivateProfileString("HardInfo", "NO2", "0", temp, 500, "D:\\HardInfo.ini"); comboxList[actual_test].Items.Add(temp.ToString());
                        comboxList[actual_test].SelectedIndex = 0;
                    }
                    if (i == 7)
                    {
                        GetPrivateProfileString("HardInfo", "NC3", "0", temp, 500, "D:\\HardInfo.ini"); comboxList[actual_test].Items.Add(temp.ToString());
                        comboxList[actual_test].SelectedIndex = 0;
                    }
                    if (i == 8)
                    {
                        GetPrivateProfileString("HardInfo", "NO4", "0", temp, 500, "D:\\HardInfo.ini"); comboxList[actual_test].Items.Add(temp.ToString());
                        comboxList[actual_test].SelectedIndex = 0;
                    }
                    if (i == 9)
                    {
                        GetPrivateProfileString("HardInfo", "LED", "0", temp, 500, "D:\\HardInfo.ini"); comboxList[actual_test].Items.Add(temp.ToString());
                        comboxList[actual_test].SelectedIndex = 0;
                    }
                    


                }
            }
                    
                    
                    
                    }

        private void button1_Click(object sender, EventArgs e)
        {
            int i;
            for (i = 1; i < 10; i++)
            {
                string actual_test = "comboBox" + i.ToString();
                if (i == 1)
                { WritePrivateProfileString("Hardinfo", "StepMotor", comboxList[actual_test].SelectedItem.ToString(), "D:\\HardInfo.ini"); }
                if (i == 2)
                { WritePrivateProfileString("Hardinfo", "Keithley", comboxList[actual_test].SelectedItem.ToString(), "D:\\HardInfo.ini"); }
                if (i == 3)
                { WritePrivateProfileString("Hardinfo", "Pow1", comboxList[actual_test].SelectedItem.ToString(), "D:\\HardInfo.ini"); }
                if (i == 4)
                { WritePrivateProfileString("Hardinfo", "Pow2", comboxList[actual_test].SelectedItem.ToString(), "D:\\HardInfo.ini"); }
                if (i == 5)
                {WritePrivateProfileString("Hardinfo", "NC1", comboxList[actual_test].Text.ToString(), "D:\\HardInfo.ini"); }
                if (i == 6)
                { WritePrivateProfileString("Hardinfo", "NO2", comboxList[actual_test].Text.ToString(), "D:\\HardInfo.ini"); }
                if (i == 7)
                { WritePrivateProfileString("Hardinfo", "NC3", comboxList[actual_test].Text.ToString(), "D:\\HardInfo.ini"); }
                if (i == 8)
                { WritePrivateProfileString("Hardinfo", "NO4", comboxList[actual_test].Text.ToString(), "D:\\HardInfo.ini"); }
                if (i == 9)
                { WritePrivateProfileString("Hardinfo", "LED", comboxList[actual_test].Text.ToString(), "D:\\HardInfo.ini"); }
                
            }
            this.Close();   
        }


















    }
}

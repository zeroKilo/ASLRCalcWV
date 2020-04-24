using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace ALSRCalc
{
    public partial class Form1 : Form
    {
        public bool _exit = false;
        Process[] list;
        List<ProcessModule> modules;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int n = comboBox1.SelectedIndex;
            int m = comboBox2.SelectedIndex;
            if (n == -1 || m == -1)
                return;
            string pName = list[n].ProcessName;
            string mName = modules[m].ModuleName;
            RefreshProcess();
            for(int i = 0; i<list.Length;i++)
                if (list[i].ProcessName == pName)
                {
                    comboBox1.SelectedIndex = i;
                    RefreshModule();
                    for(int j = 0; j < modules.Count;j++)
                        if (modules[j].ModuleName == mName)
                        {
                            comboBox2.SelectedIndex = j;
                            break;
                        }
                    break;
                }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RefreshProcess();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshModule();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            CalcForward();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            CalcForward();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            CalcBackward();
        }

        private void CalcForward()
        {
            if (_exit)
                return;
            _exit = true;
            try
            {
                ulong rebaseaddress = Convert.ToUInt64(textBox1.Text.Trim().Replace(" ", "").Replace("0x", ""), 16);
                ulong address = Convert.ToUInt64(textBox2.Text.Trim().Replace(" ", "").Replace("0x", ""), 16);
                ulong baseaddress = (ulong)modules[comboBox2.SelectedIndex].BaseAddress.ToInt64();
                if (baseaddress < address)
                {
                    address -= baseaddress;
                    address += rebaseaddress;
                    textBox3.Text = address.ToString("X8");
                }
            }
            catch { textBox3.Text = "ERROR"; }
            _exit = false;
        }

        private void CalcBackward()
        {
            if (_exit)
                return;
            _exit = true;
            try
            {
                ulong rebaseaddress = Convert.ToUInt64(textBox1.Text.Trim().Replace(" ", "").Replace("0x", ""), 16);
                ulong address = Convert.ToUInt64(textBox3.Text.Trim().Replace(" ", "").Replace("0x", ""), 16);
                ulong baseaddress = (ulong)modules[comboBox2.SelectedIndex].BaseAddress.ToInt64();
                if (rebaseaddress < address)
                {
                    address -= rebaseaddress;
                    address += baseaddress;
                    textBox2.Text = address.ToString("X8");
                }
            }
            catch { textBox2.Text = "ERROR"; }
            _exit = false;
        }

        private void RefreshProcess()
        {
            try
            {
                list = Process.GetProcesses();
                while (true)
                {
                    bool found = false;
                    for (int i = 0; i < list.Length - 1; i++)
                        if (list[i].ProcessName.CompareTo(list[i + 1].ProcessName) > 0)
                        {
                            Process p = list[i];
                            list[i] = list[i + 1];
                            list[i + 1] = p;
                            found = true;
                        }
                    if (!found)
                        break;
                }
                comboBox1.Items.Clear();
                foreach (Process p in list)
                    comboBox1.Items.Add(p.Id.ToString("D5") + " - " + p.ProcessName);
                comboBox1.SelectedIndex = 0;
            }
            catch { textBox3.Text = "ERROR"; }        
        }

        private void RefreshModule()
        {
            try
            {
                int n = comboBox1.SelectedIndex;
                if (n == -1 || list == null || list.Length <= n)
                    return;
                Process p = list[n];
                comboBox2.Items.Clear();
                modules = new List<ProcessModule>();
                foreach (ProcessModule m in p.Modules)
                    modules.Add(m);
                while (true)
                {
                    bool found = false;
                    for (int i = 0; i < modules.Count - 1; i++)
                        if (modules[i].BaseAddress.ToInt64() > modules[i + 1].BaseAddress.ToInt64())
                        {
                            ProcessModule m = modules[i];
                            modules[i] = modules[i + 1];
                            modules[i + 1] = m;
                            found = true;
                        }
                    if (!found)
                        break;
                }
                foreach (ProcessModule m in modules)
                    comboBox2.Items.Add(m.BaseAddress.ToString("X8") + " - " + m.ModuleName);
                if (comboBox2.Items.Count > 0)
                    comboBox2.SelectedIndex = 0;
            }
            catch { textBox3.Text = "ERROR"; }
        }
    }
}

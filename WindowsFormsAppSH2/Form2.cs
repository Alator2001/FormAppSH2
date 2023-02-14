using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsAppSH2
{
    public partial class Form2 : Form
    {

        readonly string defpath = "C:\\Users\\starc\\source\\repos\\WindowsFormsAppSH2\\data.txt";
        string path = "C:\\Users\\starc\\source\\repos\\WindowsFormsAppSH2\\data.txt";

        double min = double.MaxValue;
        double max = double.MinValue;

        public Form2()
        {
            InitializeComponent();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            this.chart1.Series[0].Points.Clear();
            string[] msg = File.ReadAllLines(path);
            int j = 0;
            for (int i = 0; i < msg.Length; i++)
            {
                if (msg[i] == "" || msg[i] == "|")
                {
                    continue;
                }
                else
                {
                    string[] values = msg[i].Split('\t');
                    СomparePlot(values[1]);
                    this.chart1.Series[0].Points.AddXY(j, values[1]);
                    j++;
                }
            }
            if (min != double.MaxValue && max != double.MinValue)
            {
                textBox6.Text = "min:" + min + " " + "max:" + max;
            }
            Avr();
        }
        private void Avr() //Avarage
        {
            try
            {
                Convert.ToUInt32(textBox3.Text);
            }
            catch
            {
                MessageBox.Show("Please enter a valid value Size Window AVR");
                return;
            }
            uint sizeWindow = Convert.ToUInt32(textBox3.Text);
            this.chart1.Series[1].Points.Clear();
            double[] arrWindow = new double[sizeWindow];
            string[] msg = File.ReadAllLines(path);
            double[] msgs = new double[msg.Length];
            int k = 0;
            for (int i = 0; i < msg.Length; i++)
            {
                try
                {
                    if (msg[i] == "|" || msg[i] == "")
                    {
                        continue;
                    }
                    string[] values = msg[i].Split('\t');
                    msgs[k] = Convert.ToDouble(values[1]);
                    k++;
                }
                catch
                {
                    continue;
                }
            }
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < sizeWindow; j++)
                {
                    if (j == sizeWindow - 1)
                    {
                        arrWindow[j] = msgs[i];
                    }
                    else
                    {
                        arrWindow[j] = arrWindow[j + 1];
                    }
                }
                double sum = 0;
                for (int j = 0; j < sizeWindow; j++)
                {
                    sum += arrWindow[j] * arrWindow[j];
                }
                double avr = Math.Sqrt(sum / sizeWindow);
                this.chart1.Series[1].Points.AddXY(i, avr);
            }

        }
        private void СomparePlot(string msg) //Сompare
        {
            double msgs = Convert.ToDouble(msg);
            if (msgs > max)
            {
                max = msgs;
            }
            if (msgs < min)
            {
                min = msgs;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog1.FileName;
            } 
        }
    }
}

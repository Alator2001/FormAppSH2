using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;



namespace WindowsFormsAppSH2
{

    public partial class Form1 : Form
    {

        bool isConnected = false;
        bool readingFlag = false;
        bool plotFlag = false;

        private int _countSeconds = 0;
        string defpath = "C:\\Users\\starc\\source\\repos\\WindowsFormsAppSH2\\data.txt";
        string path = "C:\\Users\\starc\\source\\repos\\WindowsFormsAppSH2\\data.txt";

        public Form1()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void button1_Click(object sender, EventArgs e) //Обновление списка портов
        {
            comboBox1.Items.Clear();
            // Получаем список COM портов доступных в системе
            string[] portnames = SerialPort.GetPortNames();
            // Проверяем есть ли доступные
            if (portnames.Length == 0)
            {
                MessageBox.Show("COM PORT not found");
            }
            foreach (string portName in portnames)
            {
                //добавляем доступные COM порты в список           
                comboBox1.Items.Add(portName);
                Console.WriteLine(portnames.Length);
                if (portnames[0] != null)
                {
                    comboBox1.SelectedItem = portnames[0];
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e) // Статус подключения
        {
            if (!isConnected)
            {
                сonnectToBoard();
            }
            else
            {
                disconnectFromBoard();
            }
        }

        private void сonnectToBoard()
        {
            string selectedPort = comboBox1.GetItemText(comboBox1.SelectedItem);
            if (string.IsNullOrEmpty(selectedPort))
            {
                MessageBox.Show("Please select a COM port");
            }
            else
            {
                try
                {
                    isConnected = true;
                    serialPort1.PortName = selectedPort;
                    serialPort1.BaudRate = Convert.ToInt32(textBox1.Text);
                    serialPort1.StopBits = StopBits.One;
                    serialPort1.Open();
                    button2.Text = "Disconnect";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error connecting to port: " + ex.Message);
                    isConnected = false;
                }
            }
        }

        private void disconnectFromBoard()
        {
            isConnected = false;
            serialPort1.Close();
            button2.Text = "Connect";
        }

        private void button3_Click(object sender, EventArgs e) // Запись данных в файл
        {
            if (!isConnected)
            {
                MessageBox.Show("Device not connected");
            }
            else
            {
                Thread toFile = new Thread(readingClick);
                if (!readingFlag)
                {
                    toFile.Start();
                    button3.Text = "Stop writing to file...";
                }
                else
                {
                    readingFlag = false;
                    MessageBox.Show("File writing stopped");
                    button3.Text = "Write to file";
                }
            }
        }

        private void readingClick()
        {
            readingFlag = true;
            string msg = "|";
            File.AppendAllText(defpath, Environment.NewLine +  msg + Environment.NewLine);
            while (readingFlag)
            {
                msg = serialPort1.ReadLine();
                File.AppendAllText(defpath, msg);
            }
        }


        private void button4_Click(object sender, EventArgs e) // построение графика
        {
            
            this.chart1.Series[0].Points.Clear();
            string[] msg = File.ReadAllLines(path);
            double[] msgs = new double[msg.Length];
            int j = 0;
            for (int i = 0; i < msg.Length; i++)
            {
                if (msg[i] == "" || msg[i] == "|")
                {
                    continue;
                }
                else
                {
                    try
                    {
                        msgs[j] = Convert.ToDouble(msg[i]);
                        this.chart1.Series[0].Points.AddXY(j, msg[i]);
                        j++;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            Avr();
        }

        private void button5_Click(object sender, EventArgs e) // Live Plot
        {
            chart2.ChartAreas[0].AxisX.LabelStyle.Format = "H:mm:ss";
            chart2.Series[0].XValueType = ChartValueType.DateTime;
            chart2.ChartAreas[0].AxisX.Minimum = DateTime.Now.ToOADate();
            chart2.ChartAreas[0].AxisX.Maximum = DateTime.Now.AddMinutes(1).ToOADate();
            chart2.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Seconds;
            chart2.ChartAreas[0].AxisX.Interval = 5;
            if (!plotFlag)
            {
                plotFlag = true;
                timer1.Enabled = true;
                button5.Text = "Stop";
            }
            else
            {
                plotFlag = false;
                timer1.Enabled = false;
                button5.Text = "Live Plot";
            }
            
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                string check = File.ReadAllLines(defpath).Last();
                Convert.ToDouble(check);
            }
            catch
            {
                return;
            }
            string msg = File.ReadAllLines(defpath).Last();
            DateTime timeNow = DateTime.Now;
            chart2.Series[0].Points.AddXY(timeNow, msg);
            _countSeconds++;
            if (_countSeconds == 60)
            {
                _countSeconds = 0;
                chart2.ChartAreas[0].AxisX.Minimum = DateTime.Now.ToOADate();
                chart2.ChartAreas[0].AxisX.Maximum = DateTime.Now.AddMinutes(1).ToOADate();
                chart2.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Seconds;
                chart2.ChartAreas[0].AxisX.Interval = 5;
            }
           
        }

        private void button6_Click(object sender, EventArgs e) //Open File
        {
            if (openFileDialog1.ShowDialog()==DialogResult.OK)
            {
                path = openFileDialog1.FileName;
            }
        }


        private void button7_Click(object sender, EventArgs e) // Save as
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = saveFileDialog1.FileName;
                string[] msg = File.ReadAllLines(defpath);
                int stopSession = msg.Length - 1;
                for (int i = msg.Length - 1; i >= 0; i--)
                {
                    if (msg[i]=="|")
                    {
                        stopSession = i;
                        break;
                    }
                }
                for (int i = stopSession + 1; i < msg.Length; i++)
                {
                    File.AppendAllText(path, msg[i] + Environment.NewLine);
                }
            }
        }

        //private void Avr() //Average
        //{
        //    int sizeWindow = Convert.ToInt32(textBox3.Text);
        //    this.chart1.Series[1].Points.Clear();
        //    double[] arrWindow = new double[sizeWindow];
        //    string[] msgs = File.ReadAllLines(path);
        //    double[] msg = msgs.Where(x => double.TryParse(x, out _)).Select(x => double.Parse(x)).ToArray();
        //    for (int i = 0; i < msg.Length; i++)
        //    {
        //        for (int j = 0; j < sizeWindow - 1; j++)
        //        {
        //            arrWindow[j] = arrWindow[j + 1];
        //        }
        //        arrWindow[sizeWindow - 1] = msg[i];
        //        double avr = Math.Sqrt(arrWindow.Sum(x => x * x) / sizeWindow);
        //        this.chart1.Series[1].Points.AddXY(i, avr);
        //    }
        //}

        //private void Avr()
        //{
        //    int sizeWindow = Convert.ToInt32(textBox3.Text);
        //    this.chart1.Series[1].Points.Clear();
        //    double[] arrWindow = new double[sizeWindow];
        //    string[] msgs = File.ReadAllLines(path);
        //    double sum = 0;
        //    int k = 0;
        //    for (int i = 0; i < msgs.Length; i++)
        //    {
        //        if (msgs[i] == "|" || msgs[i] == "")
        //        {
        //            continue;
        //        }
        //        double msg;
        //        if (double.TryParse(msgs[i], out msg))
        //        {
        //            sum += msg * msg;
        //            k++;
        //            if (k >= sizeWindow)
        //            {
        //                double avr = Math.Sqrt(sum / sizeWindow);
        //                this.chart1.Series[1].Points.AddXY(i - sizeWindow + 1, avr);
        //                sum -= arrWindow[k % sizeWindow] * arrWindow[k % sizeWindow];
        //            }
        //            arrWindow[k % sizeWindow] = msg;
        //        }
        //    }
        //}





        private void Avr()
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
            string[] msgs = File.ReadAllLines(path);
            double[] msg = new double[msgs.Length];
            int k = 0;
            for (int i = 0; i < msgs.Length; i++)
            {
                try
                {
                    if (msgs[i] == "|" || msgs[i] == "")
                    {
                        continue;
                    }
                    msg[k] = Convert.ToDouble(msgs[i]);
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
                        arrWindow[j] = msg[i];
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


    }
}








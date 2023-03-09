using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
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
using System.Net.Configuration;
using System.Diagnostics.Contracts;
using System.Data.SqlTypes;
using System.Reflection;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using System.Windows.Markup;

namespace WindowsFormsAppSH2
{

    public partial class Form1 : Form
    {
        readonly string defpath = "D:\\олд\\source\\diplom\\FormAppSH2\\data.txt";
        string path = "D:\\олд\\source\\diplom\\FormAppSH2\\data.txt";

        const int sizeBufferRecord = 256;

        bool filterFlag = false;
        bool isConnected = false;
        bool readingFlag = false;
        bool plotFlag = false;
        bool errorFlag = false;
        bool mouseMoveFlag = false;

        string[] bufferLive = new string[sizeBufferRecord];

        private int _countBuffer = 0;
        private int _countSeconds = 0;
        private int _countSumTicks = 0;

        double min = double.MaxValue;
        double max = double.MinValue;


        public Form1()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e) // Список COM портов
        {
            if (isConnected && !serialPort1.IsOpen)
            {
                disconnectFromBoard();
                errorFlag = true;
                Invoke(new Action(() => callError()));
            }
            comboBox1.Items.Clear();
            // Получаем список COM портов доступных в системе
            string[] portnames = SerialPort.GetPortNames();
            foreach (string portName in portnames)
            {
                //добавляем доступные COM порты в список           
                comboBox1.Items.Add(portName);
                //Console.WriteLine(portnames.Length);
                if (portnames[0] != null)
                {
                    comboBox1.SelectedItem = portnames[0];
                }
            }
        }

        private void button2_Click(object sender, EventArgs e) // Статус подключения
        {
            if (!isConnected)
            {
                сonnectToBoard();
            }
            else
            {
                readingFlag = false;
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
            string msg;
            long fileSize = new FileInfo(defpath).Length;
            if (fileSize == 0)
            {
                msg = "Время[c]\tОборт/мин\tdelta[V2-V1]\tКрутящий момент[Нм]\tМощность\n";
                File.AppendAllText(defpath, msg);
            }
            readingFlag = true;
            msg = "|";
            File.AppendAllText(defpath, Environment.NewLine + msg + Environment.NewLine);
            _countBuffer = 0;
            while (readingFlag)
            {
                try
                {
                    msg = serialPort1.ReadLine();
                    int ticks = Convert.ToInt32(msg);
                    if (filterFlag)
                    {
                        if (60 / (ticks * 0.0001) >= Convert.ToInt32(textBox16.Text) && 
                            60 / (ticks * 0.0001) <= Convert.ToInt32(textBox17.Text))
                        {
                            calculat(ticks);
                        }
                    }
                    else
                    {
                        calculat(ticks);
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        double sumTTime;
        double tTime;
        int V;
        double V1;
        double V2;
        double deltaV;
        double M;
        double P;

        private void calculat(double Ticks)
        {
            string msg;
            tTime = Ticks * Math.Pow(10, -4);
            V = Convert.ToInt32(60 / (Ticks * 0.0001));
            if (_countSumTicks == 2)
            {
                V2 = V;
                deltaV = V2 - V1;
                V1 = V2;
                M = (deltaV * 0.065) / (sumTTime * 10);
                M = Math.Round(M, 2);
                P = M * (V2 / 10);
                P = Math.Round(P, 2);
                //LS = P / 735.49875;
                msg = Convert.ToString(tTime) + "\t" +
                      Convert.ToString(V) + "\t" +
                      Convert.ToString(deltaV) + "\t" +
                      Convert.ToString(M) + "\t" +
                      Convert.ToString(P) + "\n";
                bufferLive[_countBuffer] = msg;
                _countBuffer++;
                if (_countBuffer == sizeBufferRecord)
                {
                    _countBuffer = 0;
                }
                File.AppendAllText(defpath, msg);
                sumTTime = 0;
                _countSumTicks = 0;
            }
            else
            {
                msg = Convert.ToString(tTime) + "\t" + Convert.ToString(V) + "\n";
                bufferLive[_countBuffer] = msg;
                _countBuffer++;
                if (_countBuffer == sizeBufferRecord)
                {
                    _countBuffer = 0;
                }
                File.AppendAllText(defpath, msg);
                sumTTime += tTime;
                _countSumTicks++;
            }
        }


        private void callError()
        {
            readingFlag = false;
            MessageBox.Show("Device disconnect, file writing stopped");
            button3.Text = "Write to file";
        }


        private void button4_Click(object sender, EventArgs e) // построение графика
        {
            this.chart1.Series[0].Points.Clear();
            this.chart1.Series[2].Points.Clear();
            chart1.ChartAreas[0].AxisX.Title = "Частота вращения коленвала, об/мин";
            chart1.ChartAreas[0].AxisY.Title = "Крутящий момент, Нм";
            chart1.ChartAreas[0].AxisY2.Title = "Мощность";
            chart1.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            chart1.Series[2].YAxisType = AxisType.Secondary;
            string[] msg = File.ReadAllLines(path);
            int j = 0;
            for (int i = 1; i < msg.Length; i++)
            {
                if (msg[i] == "" || msg[i] == "|")
                {
                    continue;
                }
                else
                {
                    string[] values = msg[i].Split('\t');
                    СomparePlot(values[1]);
                    if (values.Length > 2)
                    {
                        if (Convert.ToDouble(values[3]) > 0)
                        {
                            this.chart1.Series[0].Points.AddXY(values[1], values[3]);
                            this.chart1.Series[2].Points.AddXY(values[1], values[4]);
                        }
                        j++;
                    }
                    j++;
                }
            }
            if (min != double.MaxValue && max != double.MinValue)
            {
                textBox6.Text ="MIN:" + min + " " + "MAX:" + max;
            }
            //Avr();
            mouseMoveFlag = true;

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
            if (errorFlag)
            {
                errorFlag = false;
                plotFlag = false;
                timer1.Enabled = false;
                button5.Text = "Live Plot";
                return;
            }
            DateTime timeNow = DateTime.Now;
            if (bufferLive[0] == null)
            {
                return;
            }
            if (_countBuffer == 0)
            {
                return;
            }
            else
            {
                string[] values = bufferLive[_countBuffer - 1].Split('\t');
                chart2.Series[0].Points.AddXY(timeNow, values[1]);
            }
            _countSeconds++;
            if (_countSeconds == 600)
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
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
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
            if (msgs>max)
            {
                max = msgs;
            }
            if (msgs<min)
            {
                min = msgs;
            }
        }

        private void button1_Click(object sender, EventArgs e) //Clear all data
        {
            File.WriteAllText(defpath, string.Empty);
            min = double.MaxValue;
            max = double.MinValue;
            mouseMoveFlag = false;
        }

        private void button8_Click(object sender, EventArgs e) //Full Screen
        {
            Form2 formF = new Form2();
            formF.WindowState = FormWindowState.Maximized;
            //formF.chart1.Dock = DockStyle.Fill;
            formF.Show();
            
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)  //Координаты
        {
            if (mouseMoveFlag)
            {
                chart1.Invalidate();
                ChartArea chartArea = chart1.ChartAreas[0];
                double xValue = chartArea.AxisX.PixelPositionToValue(e.Location.X);
                double yValue = chartArea.AxisY.PixelPositionToValue(e.Location.Y);
                textBox7.Text = $"X: {xValue.ToString("0")}, Y: {yValue.ToString("0")}";
            }
        }

        private void button9_Click(object sender, EventArgs e) //Zoom
        {
            int y1 = Convert.ToInt32(textBox12.Text);
            int y2 = Convert.ToInt32(textBox13.Text);
            int x1 = Convert.ToInt32(textBox10.Text);
            int x2 = Convert.ToInt32(textBox11.Text);
            chart1.ChartAreas[0].AxisX.ScaleView.Zoom(x1, x2);
            chart1.ChartAreas[0].AxisY.ScaleView.Zoom(y1, y2);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (filterFlag)
            {
                filterFlag = false;
                button11.Text = "Filter: OFF";
            }
            else
            {
                filterFlag = true;
                button11.Text = "Filter: ON";
            }
        }
    }
}








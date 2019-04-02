using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Globalization;

namespace Client
{
    public partial class Client : Form
    {
        public delegate void UpdateListBoxCallBack(string s);
        public delegate void UpdateLabelCallBack(string s);
        private Stream stmReader = null;
        private NetworkStream nwkStream = null;
        private Stream stmWriter = null;
        private TcpClient tcpClient = null;
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        bool check = false;
        string key = "x";
        int count = 1;
        public Client()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            ran = new Random();
            FTPort = 2222;
            label3.Text = GetMAC();
            label4.Text = "Xin mời chọn thư mục lưu dữ liệu trước khi kết nối với Server";
            button2.Visible = false;
        }
        IPEndPoint Ipe;        
        Socket client;
        Random ran;
        int FTPort;
        
        void Close()
        {
            client.Close();
        }

        void Send()
        {
            client.Send(Serialize(GetMAC()));
        }
        void Receive()
        {
            try
            {

                byte[] data = new byte[8192 * 5000];
                client.Receive(data);
                string message = (string)Deserialize(data);
                key = message;
            }
            catch
            {
                Close();
            }
        }
     
        byte[] Serialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            return stream.ToArray();
        }

        object Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);
        }

        private void Client_FormClosed(object sender, FormClosingEventArgs e)
        {
            try
            {
                DialogResult dlr = MessageBox.Show("Bạn muốn thoát chương trình?",
     "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dlr == DialogResult.Yes)
                {
                    if (check==true) {
                        client.Send(Serialize(""));
                        Close();
                    }
                    e.Cancel = false;
                }
                else e.Cancel = true;
            }
            finally {
                
            }
                
        }
        public string GetMAC()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            string MAC = string.Empty;
            foreach (NetworkInterface adapter in nics)
            {
                if (MAC == string.Empty)
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    MAC = adapter.GetPhysicalAddress().ToString();
                }
            }
            return MAC;
        }

        public void SaveFile()
        {
            try
            {
                nwkStream = tcpClient.GetStream();
                stmReader = nwkStream;
                int day = DateTime.Now.Day;
                string sDay = day.ToString();
                if (day < 10)
                {
                    sDay = "0" + sDay;
                }
                int month = DateTime.Now.Month;
                string sMonth = month.ToString();
                if (month < 10) {
                    sMonth = "0" + sMonth;        
                }

                int year = DateTime.Now.Year;
                int hour = DateTime.Now.Hour;
                int minute = DateTime.Now.Minute;
                int second = DateTime.Now.Second;
                
                string zip = label4.Text+"\\backup_rcv_" + sDay + "" + sMonth + "" + year + "_" + hour + "h" + minute + "m" + second + "s_"+ ".zip";
                stmWriter = File.OpenWrite(zip);
                byte[] buff = new byte[8192];
                int len = 0;
                set("Receiving");

                while ((len = stmReader.Read(buff, 0, 8192)) > 0)
                {
                    
                    stmWriter.Write(buff, 0, len);
                    stmWriter.Flush();
                    set("Đang chạy");
                }
                nwkStream.Close();
                stmWriter.Close();
                stmReader.Close();
                set("Đã nhận file thành công!");
                
                textBox2.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   Nhận file thành công");
                textBox2.AppendText(Environment.NewLine);
                count++;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            finally
            {
                nwkStream.Close();
                stmWriter.Close();
                stmReader.Close();
            }           
        }
        void clearFile()
        {
            string sourceDir = label4.Text;
            string[] fileEntries = Directory.GetFiles(sourceDir);
            string[] backupFiles = null;

            DateTime dateNow = DateTime.Now;
            Console.WriteLine(dateNow);
            foreach (string fileName in fileEntries)
            {

                //Console.WriteLine(fileName);
                string[] nameElements = fileName.Split('_');
                if (nameElements[0].EndsWith("backup"))
                {
                    //Console.WriteLine(nameElements[1]);
                    DateTime dateFile = DateTime.ParseExact(nameElements[1], "ddMMyyyy", CultureInfo.InvariantCulture);
                    TimeSpan ts = dateNow - dateFile;
                    int dayCount = (int)ts.TotalDays;
                    if (dayCount > 3)
                    {
                        File.Delete(fileName);
                    }
                }
            }
        }
        void set(string s)
        {

            if (InvokeRequired)
            {
                object[] pList = { s };
                lbMessage.BeginInvoke(new UpdateListBoxCallBack(OnUpdateLabel), pList);
            }
            else
            {
                OnUpdateLabel(s);
            }
        }
        private void OnUpdateLabel(String s)
        {
            lbMessage.Text = s;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowNewFolderButton = false;
            path.Description = "Mời chọn thư mục để lưu file";
            if (path.ShowDialog() == DialogResult.OK)
            {
                label4.Text = path.SelectedPath;
            }
            textBox2.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   Chọn <" +label4.Text + "> làm nơi lưu trữ file");
            textBox2.AppendText(Environment.NewLine);
            if (label4.Text == "Xin mời chọn thư mục lưu dữ liệu trước khi kết nối với Server")
            {
                MessageBox.Show("Chọn thư mục", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                button2.Visible = true;
            }
        }
        private void nhanfile()
        {
        	try
            {
            	IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(textBox1.Text),int.Parse(FTPort.ToString()));
                tcpClient = new TcpClient();
                tcpClient.Connect(ipe);
                StreamReader sr = new StreamReader(tcpClient.GetStream());
                StreamWriter sw = new StreamWriter(tcpClient.GetStream());
                string duongdan = sr.ReadLine();
                lbMessage.Text = "Server gui file : " + duongdan;
                SaveFile();
                Thread nhan = new Thread(new ThreadStart(nhanfile));
                nhan.IsBackground = true;
                nhan.Start();
            }
            
            catch
            {
                Thread nhan = new Thread(new ThreadStart(nhanfile));
                nhan.IsBackground = true;
                nhan.Start();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                Ipe = new IPEndPoint(IPAddress.Parse(textBox1.Text.ToString()), int.Parse(3000.ToString()));
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    
                    client.Connect(Ipe);
                    Thread listen = new Thread(Send);
                    listen.IsBackground = true;
                    listen.Start();
                    Receive();

                    if (string.Compare(key, "1", true) == 0)
                    {
                        textBox2.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   Đã kết nối đến server");
                        textBox2.AppendText(Environment.NewLine);
                        check = true;
                        Thread nhan = new Thread(new ThreadStart(nhanfile));
                        nhan.IsBackground = true;
                        nhan.Start();

                    }
                    else
                    {
                        MessageBox.Show("Access Denied!!!", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch
                {
                    MessageBox.Show("Không thể kết nối tới Server", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Mời nhập đúng IP Server!!!", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
       
      

        private void Client_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            clearFile();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            clearFile();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}

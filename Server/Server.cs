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
using System.IO.Ports;
using System.Xml;
using System.IO.Compression;
using System.Globalization;
namespace Server
{

    public partial class Server : Form
    {        
        public delegate void Updatepgdstatus(int s, int p);
        public delegate void UpdateListBoxCallBack(string s);
        public delegate void UpdateLabelCallBack(string s);
        private NetworkStream nwkStream = null;
        private Stream stmReader = null;
        private Stream stmWriter = null;
        private Socket socketForClient = null;
        private TcpListener tcpListener = null;
        IPEndPoint Ipe;
        Socket server;
        List<Socket> ClientList;
        NetworkStream ns;
        StreamReader sr;
        StreamWriter sw;
        Socket Client;
        Thread t1;
        //Thread t2;
        string myIP = "";
        Random ran ;
        public static string CMAC= "null";
        string Client_Mac;
        string key;
        string zipA, zipB = "";
        string MPort;
        
        Boolean backuped = false;
        string inputData = String.Empty;
        delegate void SetTextCallback(string text);
        int AutoCount = 0;
        Boolean clientConnDisplay = false;
        int day2Save = 3;
        string dir = @"D:\";
        //string phone = "";
        //int timercount = 30;
        Boolean day2savechange = false;
        //int firstSight = 0;
        public Server()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceive);
            string[] BaudRate = {"9600"};
            comboBox2.Items.AddRange(BaudRate);
            radioButton1.CheckedChanged += new EventHandler(Radio_show_CheckedChanged);
            radioButton2.CheckedChanged += new EventHandler(Radio_show_CheckedChanged);
            radioButton3.CheckedChanged += new EventHandler(Radio_show_CheckedChanged);
            radioButton1.Visible = false;
            radioButton2.Visible = false;
            radioButton3.Visible = false;
            button2.Visible = false;
            button1.Visible = false;

            Connect();
            label3.Text = "";
            label7.Text = "Choose backup file";
            label9.Text = "";
            label12.Text = "Choose folder OneDrive";
            label5.Text = "Username: Guest";
            label13.Text = "Admin's number: ##########";
            label8.Text = "Client's MAC:" + CMAC;
            //label10.Text = "###";
        }
        private void Radio_show_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton.Checked == true)
            { 
                switch (radioButton.Name)
                {
                    case "radioButton1":
                        timer2.Interval = 1800000;
                        if (!timer2.Enabled)
                        {
                            timer2.Start();
                        }
                        
                        //MessageBox.Show("Thiết lập auto backup mỗi 30 phút");
                        textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   Thay đổi chu kỳ backup thành 30 phút");
                        textBox1.AppendText(Environment.NewLine);
                        break;
                    case "radioButton2":
                        timer2.Interval = 3600000;
                        if (!timer2.Enabled)
                        {
                            timer2.Start();
                        }
                        //MessageBox.Show("Thiết lập auto backup mỗi 1 giờ");
                        textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   Thay đổi chu kỳ backup thành 1 giờ");
                        textBox1.AppendText(Environment.NewLine);
                        break;
                    case "radioButton3":
                        timer2.Interval = 7200000;
                        if (!timer2.Enabled)
                        {
                            timer2.Start();
                        }
                        //MessageBox.Show("Thiết lập auto backup mỗi 2 giờ");
                        textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   Thay đổi chu kỳ backup thành 2 giờ");
                        textBox1.AppendText(Environment.NewLine);
                        break;
                }
            }
        }

        private void Server_FormClosed(object sender, FormClosingEventArgs e)
        {
            DialogResult dlr = MessageBox.Show("Bạn muốn thoát chương trình?",
     "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        	if (dlr == DialogResult.Yes) 
            {e.Cancel = false;           
                Close();}
        	else e.Cancel=true;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowNewFolderButton = false;
            path.Description = "Mời chọn thư mục để backup";
            if (path.ShowDialog() == DialogResult.OK)
            {
                label7.Text = path.SelectedPath;
            }
            textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   Chọn <" + label7.Text + "> làm thư mục chính");
            textBox1.AppendText(Environment.NewLine);
        }

        void Connect()
        {
            ran = new Random();
            MPort = 3000.ToString();
            ClientList = new List<Socket>();
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress Address in host.AddressList)
            {
                if (Address.AddressFamily.ToString() == "InterNetwork")
                {
                    myIP = Address.ToString();
                }

            }
            Ipe = new IPEndPoint(IPAddress.Parse(myIP),int.Parse(MPort));
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(Ipe);
        }
        void Close()
        {
            server.Close();
        }

        void Send(Socket Client)
        {
            Client.Send(Serialize(Check_Mac()));
        }

        void Receive(object obj)
        {
            Client = obj as Socket;
            try
            {
                byte[] data = new byte[1024 * 5000];
                Client.Receive(data);
                string message = (string)Deserialize(data);
                //label2.Text = message + "xxxx";
                Client_Mac = message;
            }
            catch (Exception)
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

        public string Check_Mac()
        {
            if (string.Compare(CMAC, Client_Mac, true) == 0)
                key = "1";
            else
                key = "0";
            return key;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!Password.stt)
            {
                Password pf = new Password();
                pf.ShowDialog(this);
            }
            
            String username = "Guest";
            if (Password.stt) {
                username = Password.username;
                CMAC = textBox2.Text.Trim();
                textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   " + username.ToUpper() + " đã đăng nhập");
                textBox1.AppendText(Environment.NewLine);
                textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   Địa chỉ MAC client: " + CMAC);
                textBox1.AppendText(Environment.NewLine);
            }
            
            label5.Text = "Username: " + username;
            label8.Text = "Client's MAC: " + CMAC;
            
            Thread Listen = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Client = server.Accept();
                        ClientList.Add(Client);
                        NetworkStream ns = new NetworkStream(Client);
                        sr = new StreamReader(ns);
                        sw = new StreamWriter(ns);

                        Thread Recv = new Thread(Receive);
                        Recv.IsBackground = true;
                        Recv.Start(Client);
                        Receive(Client);
                        Send(Client);
                                          
                    }
                }                   
                catch
                {
                    Ipe = new IPEndPoint(IPAddress.Any, int.Parse(MPort));
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    ns = new NetworkStream(Client);
                }
            });
            Listen.IsBackground = true;
            Listen.Start();
            IP.Text = "IP Server: " + myIP;
            Port.Text ="Port: " + MPort;
            
            textBox2.Visible = false;
            label6.Visible = false;
        }
        void clearFile(int day2Save)
        {
            if (day2Save != 0)
            {
                string[] fileEntries = Directory.GetFiles(dir);
                //string[] backupFiles = null;

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
                        if (dayCount > day2Save)
                        {
                            File.Delete(fileName);
                        }
                    }
                }
            }
        }
        void Main_Process()
        {
            label1.Text = "Server send file: ";
            string line_A = label7.Text;
            string line_B = label9.Text;

            string filepath = label7.Text + "\\log.txt";
            FileStream fs = new FileStream(filepath, FileMode.Create);
            StreamWriter sWriter = new StreamWriter(fs, Encoding.UTF8);
            string[] folder = System.IO.Directory.GetFiles(line_A);
            DayOfWeek dayweek = DateTime.Now.DayOfWeek;
            int day = DateTime.Now.Day;
            string sDay = day.ToString();
            if (day < 10)
            {
                sDay = "0" + sDay;
            }

            int month = DateTime.Now.Month;
            string sMonth = month.ToString();
            if (month < 10)
            {
                sMonth = "0" + sMonth;
            }

            int year = DateTime.Now.Year;
            int hour = DateTime.Now.Hour;
            int minute = DateTime.Now.Minute;
            int second = DateTime.Now.Second;
            sWriter.WriteLine("THỜI GIAN BACKUP: " + dayweek + ", " + sDay + "/" + sMonth + "/" + year + "  " + hour + ":" + minute + ":" + second);
            sWriter.WriteLine("Người dùng: " + Password.username);
            int dem = 0;
            foreach (string fileName in folder)
            {
                dem++;
                sWriter.WriteLine(dem + "/ " + Path.GetFileName(fileName).Trim() + "\n");
            }
            sWriter.Flush();
            fs.Close();

            zipA = dir+ "\\backup_" + sDay + "" + sMonth + "" + year + "_" + hour + "h" + minute + "m" + second + "s" + ".zip";
           
            ZipFile.CreateFromDirectory(line_A, zipA);
            lbLink.Text = zipA;
            tbLink.Text = zipA;

            t1 = new Thread(new ThreadStart(Send_File_Type_A));
            t1.Start();
            /*if (!label9.Text.Equals("")) { 
                zipB = dir+"\\backup_" + sDay + "" + sMonth + "" + year + "_" + hour + "h" + minute + "m" + second + "s_type_B" + ".zip";
                ZipFile.CreateFromDirectory(line_B, zipB);
                lbLink.Text = zipB;
                tbLink.Text = zipB;
                
                t2 = new Thread(new ThreadStart(Send_File_Type_B));
                t2.Start();
            }*/
            clearFile(day2Save);
        }

        private void Send_File_Type_B()
        {
            long sendTime = 0;
            try
            {
                set("Listening");
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, Convert.ToInt32(2222));
                tcpListener = new TcpListener(ip);
                tcpListener.Start();
                socketForClient = tcpListener.AcceptSocket();
                if (socketForClient.Connected)
                {
                    NetworkStream ns1 = new NetworkStream(socketForClient);
                    StreamReader sr1 = new StreamReader(ns1);
                    StreamWriter sw1 = new StreamWriter(ns1);
                    string filename = zipB;
                    nwkStream = new NetworkStream(socketForClient);
                    stmReader = File.OpenRead(tbLink.Text);
                    stmWriter = nwkStream;
                    FileInfo flInfo = new FileInfo(tbLink.Text);
                    int size = Convert.ToInt32(flInfo.Length);

                    byte[] buff = new byte[8192];
                    int len = 0;
                    int progress = 0;
                    set("Starting");
                    long timeStart = DateTime.Now.Ticks;
                    sw1.WriteLine(filename);
                    sw1.Flush();
                    while ((len = stmReader.Read(buff, 0, 8192)) != 0)
                    {
                        progress += len;
                        float progressMB = progress / (1024 * 1024);
                        float sizeMB = size / (1024 * 1024);
                        set2(size, progress);
                        set(progressMB.ToString("0.00") + " MB of " + sizeMB.ToString("0.00") + " MB");
                        stmWriter.Write(buff, 0, len);
                        stmWriter.Flush();
                    }
                    long timeStop = DateTime.Now.Ticks;
                    sendTime = (timeStop - timeStart) / TimeSpan.TicksPerSecond;

                }

                set("Thời gian gửi: " + sendTime + "s");

                textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")+ "   Gửi thành công file: " + zipB + "\n");
                textBox1.AppendText(Environment.NewLine);
                

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
            finally
            {
                if (socketForClient != null)
                {
                    socketForClient.Close();
                    nwkStream.Close();
                    stmWriter.Close();
                    stmReader.Close();
                    tcpListener.Stop();
                }
            }
        }

        private void Send_File_Type_A()
        {
            long sendTime = 0;    
            try
            {
                set("Listening");
                IPEndPoint ip = new IPEndPoint(IPAddress.Any,Convert.ToInt32(2222));
                tcpListener = new TcpListener(ip);
                tcpListener.Start();
                socketForClient = tcpListener.AcceptSocket();
                if (socketForClient.Connected)
                {
                    NetworkStream ns1 = new NetworkStream(socketForClient);
                    StreamReader sr1 = new StreamReader(ns1);
                    StreamWriter sw1 = new StreamWriter(ns1);
                    string filename = zipA;
                    nwkStream = new NetworkStream(socketForClient);
                    stmReader = File.OpenRead(tbLink.Text);
                    stmWriter = nwkStream;
                    FileInfo flInfo = new FileInfo(tbLink.Text);
                    int size = Convert.ToInt32(flInfo.Length);
                    
                    byte[] buff = new byte[8192];
                    int len = 0;
                    int progress = 0;
                    set("Starting");
                    long timeStart = DateTime.Now.Ticks;
                    sw1.WriteLine(filename);
                    sw1.Flush();
                    while ((len = stmReader.Read(buff, 0, 8192)) != 0)
                    {
                        progress += len;
                        float progressMB = progress / (1024*1024);
                        float sizeMB = size / (1024*1024);
                        set2(size, progress);
                        set(progressMB.ToString("0.00") + " MB of " + sizeMB.ToString("0.00") + " MB");
                        stmWriter.Write(buff, 0, len);
                        stmWriter.Flush();
                    }
                    long timeStop = DateTime.Now.Ticks;
                    sendTime = (timeStop - timeStart) / TimeSpan.TicksPerSecond;
                    
                }

                set("Thời gian gửi: " + sendTime + "s");
                textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")+ "   Gửi thành công file: " + zipA + "\n");
                textBox1.AppendText(Environment.NewLine);
                

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
            finally
            {
                if (socketForClient != null)
                {
                    socketForClient.Close();
                    nwkStream.Close();
                    stmWriter.Close();
                    stmReader.Close();
                    tcpListener.Stop();
                }
            }
        }
 
        void Button2Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                backuped = false;
                serialPort1.PortName = comboBox1.Text;
                serialPort1.BaudRate = 9600;
                serialPort1.Parity = Parity.None;
                serialPort1.StopBits = StopBits.One;
                serialPort1.DataBits = 8;
                serialPort1.Handshake = Handshake.None;
                serialPort1.ParityReplace = 0;
                serialPort1.Open();
                textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   Kết nối với cổng " + comboBox1.Text+ " thành công");
                textBox1.AppendText(Environment.NewLine);
            }
            
        }
        
        void Button1Click(object sender, EventArgs e)
        {
        	serialPort1.Close();
        }
        
        void ServerLoad(object sender, EventArgs e)
        {
 			comboBox1.DataSource = SerialPort.GetPortNames();
            comboBox2.SelectedIndex = 0;        	
        }
        void Timer1Tick(object sender, System.EventArgs e)
        {
            if (Client_Mac == CMAC)
            {
                
                label4.Text = "Client đã kết nối";
                if (!clientConnDisplay)
                {
                    textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "    Client kết nối thành công");
                    textBox1.AppendText(Environment.NewLine);
                }
                
                clientConnDisplay = true;
                radioButton1.Visible = true;
                radioButton2.Visible = true;
                radioButton3.Visible = true;
                btn3.Visible = true;
            } else
            {
                label4.Text = "Client chưa kết nối";
                if (clientConnDisplay)
                {
                    textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "    Client ngắt kết nối");
                    textBox1.AppendText(Environment.NewLine);
                }
                clientConnDisplay = false;
                radioButton1.Visible = false;
                radioButton2.Visible = false;
                radioButton3.Visible = false;
                btn3.Visible = false;
                timer2.Stop();
            }
            if (!serialPort1.IsOpen)
            {
                label5.Text = ("MCU chưa kết nối");
            }
            else if (serialPort1.IsOpen)
            {

                label5.Text = ("MCU đã kết nối");

            }
            if (serialPort1.IsOpen)
            {
                button1.Visible = true;
                button2.Visible = false;
            } else
            {
                button1.Visible = false;
                button2.Visible = true;
            }
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            if ((label7.Text.Equals("Choose backup file"))  )
            {
                MessageBox.Show("Chưa chọn thư mục. Vui lòng chọn thư mục", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    Main_Process();
                    AutoCount++;
                    label3.Text = "Backup " + AutoCount + " times";
                }
                finally
                {
                    Close();
                }
            }      
        }
        private void DataReceive(object obj, SerialDataReceivedEventArgs e)
        {
            
            Thread.Sleep(1000);
            inputData = serialPort1.ReadExisting();
            //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   " + inputData + "  " + inputData.LastIndexOf('#'));
            //textBox1.AppendText(Environment.NewLine);
            
            if (inputData.LastIndexOf("*") == 0 && inputData.LastIndexOf('#') > 0)
            {
                textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   Có dữ liệu điều khiển");
                textBox1.AppendText(Environment.NewLine);
                //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   "+inputData);
                //textBox1.AppendText(Environment.NewLine);
                String command = inputData.Substring(1, inputData.Length-2);
                
                string[] commandList = command.Split(';');
                //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   " + commandList[0]);

                if (commandList[0] == "1" & backuped == false)
                {
                    textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   Có nguy cơ cháy. Thực hiện backup!!!!");
                    textBox1.AppendText(Environment.NewLine);
                    try
                    {
                        SetText("Gửi ");
                        
                        Thread main = new Thread(new ThreadStart(Main_Process));
                        main.Priority = ThreadPriority.Highest;
                        main.Start();
                        AutoCount++;
                    }
                    finally
                    {
                        serialPort1.Close();
                        Close();
                    }

                }
                label13.Text = "Admin's phone: " + commandList[3];
                if(commandList[1] == "30" && radioButton1.Checked == false)
                {
                    radioButton1.Checked = true;
                }
                else
                {
                    if(commandList[1] == "60" && radioButton2.Checked == false)
                    {
                        radioButton2.Checked = true;
                    }
                    else
                    {
                        if(commandList[1] == "120" && radioButton3.Checked == false)
                        {
                            radioButton3.Checked = true;
                        }
                    }
                }
                try
                {
                    if (commandList[2] != day2Save.ToString())
                    {
                        day2savechange = true;
                        if (commandList[2] == "0")
                        {
                            day2Save = 0;
                        }
                        if (commandList[2] == "3")
                        {
                            day2Save = 3;
                        }
                        if (commandList[2] == "5")
                        {
                            day2Save = 5;
                        }
                    }
                    else
                    {
                        day2savechange = false;
                    }
                } finally
                {
                    if (day2savechange)
                    {
                        if (day2Save == 0)
                        {
                            textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   Hủy tự động xóa dữ liệu");
                            textBox1.AppendText(Environment.NewLine);
                        }
                        else
                        {
                            textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   Tự động xóa dữ liệu sau " + day2Save + " ngày");
                            textBox1.AppendText(Environment.NewLine);
                        }
                    }
                }
            {

            }
                
                
            }
            
            
        }

        private void SetText(string text)
        {
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else {
            	this.textBox1.Text += text;
            	if(text=="0") label4.Text="Dừng ";
                if(text=="1") label4.Text="Gửi ";
            }
        }

        private void OnUpdateLabel(String s)
        {
            lbProgressbar.Text = s;
        }
        void set(string s)
        {
            if (InvokeRequired)
            {
                object[] pList = { s };
                lbProgressbar.BeginInvoke(new UpdateListBoxCallBack(OnUpdateLabel), pList);
            }
            else
            {
                OnUpdateLabel(s);
            }
        }
        void set2(int s, int p)
        {
            if (InvokeRequired)
            {
                object[] pList = { s, p };
                pgbStatus.BeginInvoke(new Updatepgdstatus(OnUpdatepgdstatus), pList);
            }
            else
            {
                OnUpdatepgdstatus(s, p);
            }
        }
        private void OnUpdatepgdstatus(int s, int p)
        {
            pgbStatus.Maximum = s;
            pgbStatus.Value = p;
        }

        

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void pgbStatus_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        

        private void btn3_Click(object sender, EventArgs e)
        {
            textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   Thực hiện backup ngay lập tức");
            textBox1.AppendText(Environment.NewLine);
            Thread main = new Thread(new ThreadStart(Main_Process));
            main.Priority = ThreadPriority.Highest;
            main.Start();
            AutoCount++;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }


        public void getDataFromJAVA()
        {
            byte[] data = new byte[1024];
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 8888);
            Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.Bind(endpoint);
            EndPoint remote = (EndPoint)endpoint;
            int recv = sck.ReceiveFrom(data, ref remote);
            //label10.Text = Encoding.ASCII.GetString(data, 0, recv);
            sck.Close();
            
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            Thread getData = new Thread(new ThreadStart(getDataFromJAVA));
            getData.Priority = ThreadPriority.AboveNormal;
            getData.Start();
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowNewFolderButton = false;
            path.Description = "Mời chọn thư mục để backup";
            if (path.ShowDialog() == DialogResult.OK)
            {
                label12.Text = path.SelectedPath;
                dir = label12.Text;
            }
            textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "   Chọn <" + label12.Text + "> làm thư mục lưu trữ");
            textBox1.AppendText(Environment.NewLine);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        

        private void button5_Click(object sender, EventArgs e)
        {
            
        }
    }
}

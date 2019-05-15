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
        string time = "";
        string cmd = "";
        //Thread t2;
        string myIP = "";
        DateTime timeAccess = new DateTime(2000, 1, 1, 0, 0, 0);
        Random ran ;
        public static string CMAC= "null";
        string Client_Mac;
        string key;
        string zipA, zipB = "";
        string zipFile = "";
        string MPort;
        float sizeMB = 0;
        Boolean backuped = false;
        string inputData = String.Empty;
        delegate void SetTextCallback(string text);
        int AutoCount = 0;
        Boolean clientConnDisplay = false;
        int day2Save = 3;
        string dirFolder = @"D:\";
        string dirOneDrive = "";
        string dir2SaveOffline = "";
        string dir2SaveOneDrive = "";
        string commandFromUser = "";
        int countFailRequest = 0;
        Boolean clientConnect = false;
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
            label7.Text = "Chọn thư mục sao lưu";
            label9.Text = "";
            label12.Text = "Chọn thư mục đồng bộ OneDrive";
            label5.Text = "Username: Guest";
            label13.Text = "SĐT liên lạc: ##########";
            label8.Text = "Địa chỉ MAC :" + CMAC;
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
                        time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                        cmd = "   Thay đổi chu kỳ sao lưu thành 30 phút";
                        textBox1.AppendText(time + cmd);
                        sendData(time, cmd);
                        textBox1.AppendText(Environment.NewLine);
                        break;
                    case "radioButton2":
                        timer2.Interval = 3600000;
                        if (!timer2.Enabled)
                        {
                            timer2.Start();
                        }
                        //MessageBox.Show("Thiết lập auto backup mỗi 1 giờ");
                        time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                        cmd = "   Thay đổi chu kỳ sao lưu thành 1 giờ";
                        textBox1.AppendText(time + cmd);
                        sendData(time, cmd);
                        //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   Thay đổi chu kỳ backup thành 1 giờ");
                        textBox1.AppendText(Environment.NewLine);
                        break;
                    case "radioButton3":
                        timer2.Interval = 7200000;
                        if (!timer2.Enabled)
                        {
                            timer2.Start();
                        }
                        //MessageBox.Show("Thiết lập auto backup mỗi 2 giờ");
                        time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                        cmd = "   Thay đổi chu kỳ sao lưu thành 2 giờ";
                        textBox1.AppendText(time + cmd);
                        sendData(time, cmd);
                        //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   Thay đổi chu kỳ backup thành 2 giờ");
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
            path.Description = "Mời chọn thư mục để sao lưu";
            if (path.ShowDialog() == DialogResult.OK)
            {
                label7.Text = path.SelectedPath;
                dirFolder = label7.Text;
            }
            time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            cmd = "   Chọn <" + label7.Text + "> làm thư mục sao lưu";
            textBox1.AppendText(time + cmd);
            //sendData(time, cmd);
            //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   Chọn <" + label7.Text + "> làm thư mục chính");
            textBox1.AppendText(Environment.NewLine);
            button4.Visible = false;
            button5.Visible = true;
            label7.Visible = false;
            label14.Visible = true;
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
        public int sendData(string time,string log)
        {
            int count = 0;
            while (true)
            {
                if(count > 5)
                {
                    return 1;
                }
                try
                {
                    string commandLink = "http://vvtsmart.com/backup/updateLog.php?time=" + time+"&log="+log;
                    WebClient client = new WebClient();
                    String downloadedString = client.DownloadString(commandLink);
                    if (downloadedString == "Successful!!!")
                    {
                        return 0;
                    }
                }
                catch
                {
                    count += 1;
                }
            }
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
                time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                cmd =  "   " + username.ToUpper() + " đã đăng nhập";
                textBox1.AppendText(time+cmd);
                sendData(time,cmd);
                textBox1.AppendText(Environment.NewLine);

                time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                cmd = "   Địa chỉ MAC client: " + CMAC;
                textBox1.AppendText(time + cmd);
                sendData(time, cmd);
                //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   Địa chỉ MAC client: " + CMAC);
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
                string[] fileEntries = Directory.GetFiles(dirFolder);
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
                            time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            cmd = "   Xóa file:" + fileName;
                            textBox1.AppendText(time + cmd);
                            sendData(time, cmd);
                            textBox1.AppendText(Environment.NewLine);
                        }
                    }
                }
            }
        }
       
        void Main_Process()
        {
            sendToOneDriveFolder();
            label1.Text = "Gửi file: ";
            string line_to_file = label7.Text;
            

            string filepath = label7.Text + "\\log.txt";
            FileStream fs = new FileStream(filepath, FileMode.Create);
            StreamWriter sWriter = new StreamWriter(fs, Encoding.UTF8);
            string[] folder = System.IO.Directory.GetFiles(line_to_file);
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

            List<String> fileChanged = new List<string>();

            int index = 0;
            
            foreach (string file in folder)
            {

                DateTime modification = File.GetLastWriteTime(file);
                int result = DateTime.Compare(timeAccess, modification);
                if (result == -1)
                {
                    string[] fileEle = file.Split('\\');
                    String fileName = fileEle[fileEle.Length - 1];
                    fileChanged.Add(fileName);

                }
                //AppendText(result.ToString());
                //textBox1.AppendText(Environment.NewLine);


            }
            int countFile = 0;
            foreach (string fileName in fileChanged)
            {
                countFile += 1;
            }
            time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            cmd = "   Sao lưu " + countFile + " file";
            textBox1.AppendText(time + cmd);
            sendData(time, cmd);
            textBox1.AppendText(Environment.NewLine);
            string root = @"D:\cache";

            

            if (!Directory.Exists(root))
            {

                Directory.CreateDirectory(root);


            }
            else
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(root);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }
            foreach (string fileName in fileChanged)
            {
                string sourceFile = System.IO.Path.Combine(label7.Text, fileName);
                string destFile = System.IO.Path.Combine(root, fileName);
                System.IO.File.Copy(sourceFile, destFile, true);
            }


            timeAccess = DateTime.Now;
            zipFile = dir2SaveOffline + "\\backup_" + sDay + "" + sMonth + "" + year + "_" + hour + "h" + minute + "m" + second + "s" + ".zip";
            ZipFile.CreateFromDirectory(root, zipFile);
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
                        sizeMB = size / (1024 * 1024);
                        set2(size, progress);
                        set(progressMB.ToString("0.00") + " MB of " + sizeMB.ToString("0.00") + " MB");
                        stmWriter.Write(buff, 0, len);
                        stmWriter.Flush();
                    }
                    long timeStop = DateTime.Now.Ticks;
                    sendTime = (timeStop - timeStart) / TimeSpan.TicksPerSecond;

                }

                set("Thời gian gửi: " + sendTime + "s");

                textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")+ "   Gửi thành công file: " + zipB + "\n");
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

        public void sendToOneDriveFolder()
        {
            string lineOneDrive = label12.Text;
            

            string filepath = label12.Text + "\\log.txt";
            FileStream fs = new FileStream(filepath, FileMode.Create);
            StreamWriter sWriter = new StreamWriter(fs, Encoding.UTF8);
            string[] folders = System.IO.Directory.GetFiles(dirOneDrive);
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
            foreach (string fileName in folders)
            {
                dem++;
                sWriter.WriteLine(dem + "/ " + Path.GetFileName(fileName).Trim() + "\n");
            }
            sWriter.Flush();
            fs.Close();
            string zipA_path = "\\backup_" + sDay + "" + sMonth + "" + year + "_" + hour + "h" + minute + "m" + second + "s" + ".zip";
            zipA = dir2SaveOneDrive + zipA_path;

            ZipFile.CreateFromDirectory(dirOneDrive, zipA);
            time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            cmd = "   Lưu trữ file " + zipA_path + " vào OneDrive thành công";
            textBox1.AppendText(time + cmd);
            sendData(time, cmd);
            //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   Kết nối với cổng " + comboBox1.Text + " thành công");
            textBox1.AppendText(Environment.NewLine);

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
                    string filename = zipFile;
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
                time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                cmd = "   Gửi thành công file: " + zipA;
                textBox1.AppendText(time + cmd);
                sendData(time, cmd);
                //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")+ "   Gửi thành công file: " + zipA);
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
                if(comboBox1.Text == "")
                {
                    MessageBox.Show("Vui lòng chọn cổng COM!!!");
                }
                else
                {
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.BaudRate = 9600;
                    serialPort1.Parity = Parity.None;
                    serialPort1.StopBits = StopBits.One;
                    serialPort1.DataBits = 8;
                    serialPort1.Handshake = Handshake.None;
                    serialPort1.ParityReplace = 0;
                    serialPort1.Open();
                    time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    cmd = "   Kết nối với cổng " + comboBox1.Text + " thành công";
                    textBox1.AppendText(time + cmd);
                    sendData(time, cmd);
                    //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   Kết nối với cổng " + comboBox1.Text + " thành công");
                    textBox1.AppendText(Environment.NewLine);
                }
                timer4.Enabled = false;
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
                clientConnect = true;
                if (!clientConnDisplay)
                {
                    time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    cmd = "    Client kết nối thành công";
                    textBox1.AppendText(time + cmd);
                    sendData(time, cmd);
                    //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "    Client kết nối thành công");
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
                    time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    cmd = "    Client ngắt kết nối";
                    textBox1.AppendText(time + cmd);
                    sendData(time, cmd);
                    //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "    Client ngắt kết nối");
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
            if ((label7.Text.Equals("Chọn thư mục sao lưu"))  )
            {
                MessageBox.Show("Chưa chọn thư mục. Vui lòng chọn thư mục", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    cmd = "   Thực hiện backup theo chu kỳ";
                    textBox1.AppendText(time + cmd);
                    sendData(time, cmd);

                    //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "");
                    textBox1.AppendText(Environment.NewLine);
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
            //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   " + inputData + "  " + inputData.LastIndexOf('#'));
            //textBox1.AppendText(Environment.NewLine);
            
            if (inputData.LastIndexOf("*") == 0 && inputData.LastIndexOf('#') > 0)
            {
                time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                cmd = "   Có dữ liệu điều khiển";
                textBox1.AppendText(time + cmd);
                sendData(time, cmd);

                //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "");
                textBox1.AppendText(Environment.NewLine);
                //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   "+inputData);
                //textBox1.AppendText(Environment.NewLine);
                String command = inputData.Substring(1, inputData.Length-2);
                
                string[] commandList = command.Split(';');
                //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   " + commandList[0]);

                if (commandList[0] == "1" & backuped == false)
                {
                    time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    cmd = "   Có nguy cơ cháy. Thực hiện backup!!!!";
                    textBox1.AppendText(time + cmd);
                    sendData(time, cmd);
                    //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "");
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
                label13.Text = "SĐT liên lạc: " + commandList[3];
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
                            time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            cmd = "   Hủy tự động xóa dữ liệu";
                            textBox1.AppendText(time + cmd);
                            sendData(time, cmd);
                            //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   Hủy tự động xóa dữ liệu");
                            textBox1.AppendText(Environment.NewLine);
                        }
                        else
                        {
                            time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            cmd = "   Tự động xóa dữ liệu sau " + day2Save + " ngày";
                            textBox1.AppendText(time + cmd);
                            sendData(time, cmd);
                            //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   Tự động xóa dữ liệu sau " + day2Save + " ngày");
                            textBox1.AppendText(Environment.NewLine);
                        }
                    }
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
            time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            cmd = "   Thực hiện backup ngay lập tức";
            textBox1.AppendText(time + cmd);
            sendData(time, cmd);

            //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   Thực hiện backup ngay lập tức");
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
                dirOneDrive = label12.Text;
            }

            time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            cmd = "   Chọn <" + label12.Text + "> làm thư mục đồng bộ OneDrive";
            textBox1.AppendText(time + cmd);
            //sendData(time, cmd);

            //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   Chọn <" + label12.Text + "> làm thư mục lưu trữ");
            textBox1.AppendText(Environment.NewLine);
            button7.Visible = true;
            button6.Visible = false;
            label12.Visible = false;
            label15.Visible = true;
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

        private void button5_Click_1(object sender, EventArgs e)
        {
           
        }

        private void sssssToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ghiFileTheoDõiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filePath = "D:\\";
            string fileName = "log_" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + ".txt";
            string fullName = filePath + fileName;
            File.WriteAllText(fullName, textBox1.Text);
            MessageBox.Show("Ghi thành công file " + fullName);
        }

        private void sửDựngMặcĐịnhToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox2.Text = "0C9D9256AC52";
            label12.Text = "";
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void hướngDẫnSửDụngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("B1: CHọn file cần backup ở mục <Chọn file>\nB2: Chọn thư mục đồng bộ với OneDrive ở <OneDrive>" +
                "\nB3:Nhập địa chỉ MAC của server phụ\nB4:Nhấn bắt đầu và điền tên đăng nhập + mật khẩu" +
                "\nB5:Chờ máy chủ phụ kết nối\nB6:Thay đổi chu kỳ backup nếu cần" +
                "\nB7:Chọn cổng COM và baudrate để kết nối với hệ thống cảm biến" +
                "\n:<Lưu ý: Có thể nhấn BackUp để backup bằng tay>");
        }

        private void liênHệToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Đề tài: Ứng dụng IOT trong backup dữ liệu cho các phòng máy server" +
                            "\nGiáo viên hướng dẫn: Ngô Minh Trí" +
                            "\n                                      Vũ Vân Thanh" +
                            "\nSinh viên thực hiện: Nguyễn Văn Nhật Quang");
        }

        private void aboutMeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Tên: Nguyễn Văn Nhật Quang\nLớp: 14DT2 Điện tử - viễn thông\nE-mail:nhatquang.nv14@gmail.com\nSĐT:0976742314\nFB:<Đường Hạo> facebook.com/smstarfall");
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void nộiDungĐềTàiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Xây dựng hệ thống ứng dụng IOT để backup dữ liệu. Hệ thống IOT được xây dựng để người dùng theo dõi các thông số trong phòng máy đồng thời có thể phát " +
                "hiện và cảnh báo cháy trong trường hợp khẩn cấp. Phần mềm thực hiện việc backup dữ liệu theo chu kỳ và đồng bộ với onedrive, trong trường hợp có cháy hoặc các trường hợp khẩn cấp " +
                "hệ thống sẽ tự động thực hiện công việc backup để tránh các rủi ro với dữ liệu. Ngoài ra người dùng có thể điều khiển phần mềm từ xa qua website");
        }

        private void đếnTrangĐiềuKhiểnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://vvtsmart.com/backup");
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void appTheoDõiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Minitoring_App ma = new Minitoring_App();
            ma.Show(this);
        }

        private void theoDõiServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ServerManager sm = new ServerManager();
            sm.Show(this);
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            comboBox1.DataSource = SerialPort.GetPortNames();
        }

        private void button5_Click_2(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowNewFolderButton = false;
            path.Description = "Mời chọn thư mục để lưu trữ";
            if (path.ShowDialog() == DialogResult.OK)
            {
                label14.Text = path.SelectedPath;
                dir2SaveOffline = label14.Text;
            }
            time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            cmd = "   Chọn <" + label14.Text + "> làm địa chỉ lưu trữ";
            textBox1.AppendText(time + cmd);
            //sendData(time, cmd);
            //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   Chọn <" + label7.Text + "> làm thư mục chính");
            textBox1.AppendText(Environment.NewLine);
            button4.Visible = true;
            button5.Visible = false;
            label14.Visible = false;
            label7.Visible = true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowNewFolderButton = false;
            path.Description = "Mời chọn thư mục lưu trữ OneDrive";
            if (path.ShowDialog() == DialogResult.OK)
            {
                label15.Text = path.SelectedPath;
                dir2SaveOneDrive = label15.Text;
            }

            time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            cmd = "   Chọn <" + label15.Text + "> làm thư mục lưu trữ OneDrive";
            textBox1.AppendText(time + cmd);
            //sendData(time, cmd);

            //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   Chọn <" + label12.Text + "> làm thư mục lưu trữ");
            textBox1.AppendText(Environment.NewLine);
            button6.Visible = true;
            button7.Visible = false;
            label15.Visible = false;
            label12.Visible = true;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string line_A = label12.Text;


            string filepath = label12.Text + "\\log.txt";
            FileStream fs = new FileStream(filepath, FileMode.Create);
            StreamWriter sWriter = new StreamWriter(fs, Encoding.UTF8);
            string[] folder = System.IO.Directory.GetFiles(dirOneDrive);
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
            string zipA_path = "\\backup_" + sDay + "" + sMonth + "" + year + "_" + hour + "h" + minute + "m" + second + "s" + ".zip";
            zipA = dir2SaveOneDrive + zipA_path;

            ZipFile.CreateFromDirectory(dirOneDrive, zipA);
            time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            cmd = "   Lưu trữ file " + zipA_path + " vào OneDrive thành công";
            textBox1.AppendText(time + cmd);
            sendData(time, cmd);
            //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   Kết nối với cổng " + comboBox1.Text + " thành công");
            textBox1.AppendText(Environment.NewLine);


        }

        private void button9_Click(object sender, EventArgs e)
        {
            timer5.Enabled = true;
        }

        private void timer5_Tick(object sender, EventArgs e)
        {

            if (countFailRequest < 5)
            {
                try
                {
                    string link = "http://vvtsmart.com/backup/updateData.php";
                    WebClient client = new WebClient();
                    String downloadedString = client.DownloadString(link);
                    if (!downloadedString.Equals(commandFromUser))
                    {
                        commandFromUser = downloadedString;
                        excecuteChange(commandFromUser);
                        textBox1.AppendText("Nhận được dữ liệu điều khiển mới");
                        textBox1.AppendText(Environment.NewLine);
                    }
                    countFailRequest = 0;
                }
                catch
                {
                    countFailRequest += 1;
                }
            }
            else
            {
                textBox1.AppendText("Kiểm tra kết nối Internet");
                textBox1.AppendText(Environment.NewLine);
                countFailRequest = 0;
            }
        }

        private void excecuteChange(string inputData)
        {
            if (clientConnect)
            {
                if (inputData.LastIndexOf("*") == 0 && inputData.LastIndexOf('#') > 0)
                {

                    //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "");

                    //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   "+inputData);
                    //textBox1.AppendText(Environment.NewLine);
                    String command = inputData.Substring(1, inputData.Length - 2);

                    string[] commandList = command.Split(';');
                    //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   " + commandList[0]);

                    if (commandList[0] == "1" & backuped == false)
                    {
                        time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                        cmd = "   Có nguy cơ cháy. Thực hiện backup!!!!";
                        textBox1.AppendText(time + cmd);
                        sendData(time, cmd);
                        //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "");
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
                    label13.Text = "SĐT liên lạc: " + commandList[3];
                    if (commandList[1] == "30" && radioButton1.Checked == false)
                    {
                        radioButton1.Checked = true;
                    }
                    else
                    {
                        if (commandList[1] == "60" && radioButton2.Checked == false)
                        {
                            radioButton2.Checked = true;
                        }
                        else
                        {
                            if (commandList[1] == "120" && radioButton3.Checked == false)
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
                    }
                    finally
                    {
                        if (day2savechange)
                        {
                            if (day2Save == 0)
                            {
                                time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                                cmd = "   Hủy tự động xóa dữ liệu";
                                textBox1.AppendText(time + cmd);
                                sendData(time, cmd);
                                //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   Hủy tự động xóa dữ liệu");
                                textBox1.AppendText(Environment.NewLine);
                            }
                            else
                            {
                                time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                                cmd = "   Tự động xóa dữ liệu sau " + day2Save + " ngày";
                                textBox1.AppendText(time + cmd);
                                sendData(time, cmd);
                                //textBox1.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "   Tự động xóa dữ liệu sau " + day2Save + " ngày");
                                textBox1.AppendText(Environment.NewLine);
                            }
                        }
                    }



                }
            }
            else
            {
                commandFromUser = "";
                textBox1.AppendText("Thay đổi thông tin thất bại!! Kiểm tra kết nối đến client");
                textBox1.AppendText(Environment.NewLine);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            
        }
    }
}

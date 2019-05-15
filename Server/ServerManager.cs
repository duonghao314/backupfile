using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Collections;
using System.Diagnostics;
using System.Net;
using System.IO;

namespace Server
{
    public partial class ServerManager : Form
    {
        string tempS = "";
        float? tempBefore = 0;
        string computerName = Environment.MachineName;
        string username = Environment.UserName;
        PerformanceCounter perCPU = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");
        PerformanceCounter perMem = new PerformanceCounter("Memory", "Available MBytes");
        PerformanceCounter perSystem = new PerformanceCounter("System", "System Up Time");

        //PerformanceCounter freq = new PerformanceCounter("Processor Performance", "Processor Frequency", "PPM_Processor_0");

        public ServerManager()
        {
            InitializeComponent();
            label2.Text = "\"" + computerName + "\"";
            label3.Text = "\""+username +"\"";
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label4.Text = ((int)(perCPU.NextValue())).ToString();
            label5.Text = perMem.NextValue().ToString();
            label6.Text = perSystem.NextValue().ToString();
            //label22.Text = freq.NextValue().ToString();
            ManagementObject disk = new  ManagementObject("win32_logicaldisk.deviceid=\"d:\"");
            disk.Get();

            label6.Text = ((ulong)disk["Size"] / 1024 / 1024 / 1024).ToString();
            label15.Text = ((ulong)disk["FreeSpace"] / 1024 / 1024 / 1024).ToString();
            tempBefore = GetSystemInfo(tempBefore);
            


        }

        private void ServerManager_Load(object sender, EventArgs e)
        {

        }

        private void label18_Click(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                string commandLink = "http://vvtsmart.com/backup/updateCompData.php?cn=" + computerName + "&cpu=" + label4.Text +
                    "&am=" + label5.Text + "&fs=" + label15.Text + "&temp=" + label19.Text;
                WebClient client = new WebClient();
                String downloadedString = client.DownloadString(commandLink);
                if (downloadedString == "1")
                {
                    label21.Text = "Data Sent at " + DateTime.Now.ToString("hh:mm:ss");
                }
            } 
            catch
            {
                label21.Text = "Connection Error";
            }
        }
        public float? GetSystemInfo(float? tempBefore)
        {
            string temp = "";
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            computer.Open();
            computer.Accept(updateVisitor);
            float? max = 0;
            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                {
                    
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        
                        float? tempI = computer.Hardware[i].Sensors[j].Value;
                        if(tempI > max)
                        {
                            max = tempI;
                        }
                        
                    }
                }
            }
            if(max < tempBefore - 20)
            {
                max = tempBefore;
            }
            temp = max.ToString();
            if (temp.Length > 5)
            {
                temp = temp.Substring(0, 5);
            }
            label19.Text = temp;
            computer.Close();
            return max;
        }

        private void label21_Click(object sender, EventArgs e)
        {

        }
    }
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
    
}

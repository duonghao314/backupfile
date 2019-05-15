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
using System.IO;



namespace Server
{

    public partial class Minitoring_App : Form
    {
        int stt = 0;
        
        public Minitoring_App()
        {
            InitializeComponent();
        }

        private void Minitoring_App_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            WebClient client = new WebClient();
            String downloadedString = client.DownloadString("http://vvtsmart.com/backup/appRequest.php");
            downloadedString = downloadedString.Substring(1, downloadedString.Length - 2);
            string[] elements = downloadedString.Split(';');
            label15.Text = elements[0];
            label16.Text = elements[1];
            label17.Text = elements[2];
            label10.Text = elements[3];
            if(elements[4] == "0")
            {
                label12.Text = elements[4] + " - NORMAL";
            }
            else
            {
                label12.Text = elements[4] + " - DANGER";
            }
            
            label14.Text = downloadedString;

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            
            string day = DateTime.Now.ToString("dd-MM-yyyy");
            label2.Text = day;
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if(stt == 0)
            {
                string time = DateTime.Now.ToString("hh:mm:ss");
                stt = 1;
                label3.Text = time;
            }
            else
            {
                string time = DateTime.Now.ToString("hh mm ss");
                stt = 0;
                label3.Text = time;
            }
            
            
        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            WebClient client = new WebClient();
            String downloadedString = client.DownloadString("http://vvtsmart.com/backup/appRequest.php");
            downloadedString = downloadedString.Substring(1, downloadedString.Length - 2);
            string[] elements = downloadedString.Split(';');
            label15.Text = elements[0];
            label16.Text = elements[1];
            label17.Text = elements[2];
            label10.Text = elements[3];
            if (elements[4] == "0")
            {
                label12.Text = elements[4] + " - NORMAL";
            }
            else
            {
                label12.Text = elements[4] + " - DANGER";
            }
            timer4.Enabled = false;
        }
    }
    
    
}

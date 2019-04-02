using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    
    public partial class Password : Form
    {
        public static string username = "";
        public static string password = "";
        public static Boolean stt = false;
        public Password()
        {
            InitializeComponent();
            textBox1.PasswordChar = '*';

        }

        private void Password_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            username = textBox2.Text;
            password = textBox1.Text;
            
            if ((password.Equals("123456") && username.Equals("admin"))|| (password.Equals("123456") && username.Equals("mod")))
            {
                stt = true;
                
                this.Close();
                
            }
        }
       

        private void Password_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!stt)
            {
                DialogResult dlr = MessageBox.Show("Bạn không thể nhập MAC!!",
         "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                Server.CMAC = "null";
                if (dlr == DialogResult.Cancel)
                {
                    e.Cancel = false;
                    Close();
                }
                else e.Cancel = false;
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}

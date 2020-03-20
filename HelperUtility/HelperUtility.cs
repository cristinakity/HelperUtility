using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HelperUtility
{
    public partial class HelperUtility : Form
    {
        Keys stopKey;
        Keys startKey;
        int seconds;
        const string info = "Ctrl + Alt + ";
        bool isStarted = false;
        public HelperUtility(Keys start,Keys stop, int seconds)
        {
            this.seconds = seconds;
            this.stopKey = stop;
            this.startKey = start;
            InitializeComponent();
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
        }

        public void Start()
        {
            if (!isStarted)
            {
                isStarted = true;
                this.btnStart.Text = "Stop";
                this.btnStart.BackColor = Color.LightSalmon;
                Program.Start(false);
            }
        }
        
        public void Stop()
        {
            if (isStarted)
            {
                isStarted = false;
                this.btnStart.Text = "Start";
                this.btnStart.BackColor = Color.LightGreen;
                Program.Stop(false);
                this.notifyIcon1.ShowBalloonTip(100, "Utility", "Stoped..", ToolTipIcon.Info);
            }
        }

        private void HelperUtility_Resize(object sender, EventArgs e)
        {
            if(this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                //Debug.WriteLine("Pantalla oculta");
                //Console.WriteLine("Pantalla oculta");
                //notifyIcon1.ShowBalloonTip(500,"hola","Algo",ToolTipIcon.Info);
                notifyIcon1.Visible = true;
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Console.WriteLine("Right Click.");
                this.contextMenuStrip1.Show();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void HelperUtility_Load(object sender, EventArgs e)
        {
            UpdateKeyInfo();
        }

        private void UpdateKeyInfo()
        {
            this.txtSeconds.Text = this.seconds == 0 ? "" : this.seconds.ToString();

            this.infoStart.Text = info + startKey;
            this.InfoStop.Text = info + stopKey;

            this.txtStartKey.Text = startKey.ToString();
            this.txtStopKey.Text = stopKey.ToString();

            Program.startKey = startKey;
            Program.stopKey = stopKey;
            Program.seconds = seconds;

            Program.SetAppConfigKeys();
        }

      

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        
        private void txtStartKey_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
            else
            {
                Keys k = (Keys)char.ToUpper(e.KeyChar);
                this.startKey = k;
                UpdateKeyInfo();
            }
        }

        private void txtStopKey_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
            else
            {
                Keys k = (Keys)char.ToUpper(e.KeyChar);
                this.stopKey = k;
                UpdateKeyInfo();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if(!isStarted)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        private void txtSeconds_TextChanged(object sender, EventArgs e)
        {
            Int32.TryParse(this.txtSeconds.Text,out seconds);
            UpdateKeyInfo();
        }
        
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stop();
        }
    }
}

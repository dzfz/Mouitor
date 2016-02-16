using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KlandMouitor
{
    public partial class Form1 : Form
    {
        public bool isAutoRun = false;

        public Form1(string[] args)
        {
            InitializeComponent();
            if (args != null && args.Length > 0)
            {
                if ("/start".Equals(args[0]))
                {
                    isAutoRun = true;

                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            this.textBoxServiceName.Focus();
            this.notifyIcon.Text = "服务监控";
            //ServiceUtils.getAllService();

            if (isAutoRun)
            {
                string[] configArr = TimerUtils.readConfigFromFile();
                if (configArr != null && configArr.Length == 3)
                {
                    this.textBoxServiceName.Text = configArr[0];
                    this.textBoxTime.Text = configArr[1];
                    this.comboBoxTimeType.Text = configArr[2];
                    this.buttonStart.PerformClick();
                }
            }


        }

        private string checkServiceName()
        {
            string serviceName = "";
            string serviceNameStr = this.textBoxServiceName.Text;
            if (serviceNameStr != null && serviceNameStr.Length > 0)
            {
                serviceName = serviceNameStr;
            }
            else
            {
                MessageBox.Show("服务名称不能为空！");
            }
            return serviceName;
        }

        private int checkTime()
        {
            int time = 10;
            string timeStr = this.textBoxTime.Text;
            if (timeStr != null && timeStr.Length > 0)
            {
                try
                {
                    time = int.Parse(timeStr);
                }
                catch (Exception ex)
                {
                    this.textBoxTime.Text = "";
                    time = 0;
                    TimerUtils.writeLog(ex.ToString());
                }
            }
            else
            {
                MessageBox.Show("监控频率不能为空！");
            }
            return time;
        }

        public void startMouitor()
        {
            this.buttonStart.Text = "监控中";
            this.buttonStart.Enabled = false;
            this.textBoxServiceName.Enabled = false;
            this.textBoxTime.Enabled = false;
            this.comboBoxTimeType.Enabled = false;
        }

        public void stopMouitor()
        {
            this.buttonStart.Text = "开始";
            this.buttonStart.Enabled = true;
            this.textBoxServiceName.Enabled = true;
            this.textBoxTime.Enabled = true;
            this.comboBoxTimeType.Enabled = true;
        }

        public void setMouitorCount(long count)
        {
            this.labelCount.Text = count.ToString();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            TimerUtils.stopTimer();
            stopMouitor();
            TimerUtils.writeLog("定时器已取消");
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            int time = checkTime();
            if (time < 1)
            {
                time = 10;
            }
            string serviceName = checkServiceName();
            if (serviceName != null && serviceName.Length > 0)
            {
                string type = this.comboBoxTimeType.Text;
                TimerUtils.initTimer(this,serviceName, time, type);
                int flag = TimerUtils.startTimer();
                if (flag != -1)
                {
                    startMouitor();
                }

            }

        }

        private void textBoxTime_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b' && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)    //最小化到系统托盘
            {
                notifyIcon.Visible = true;    //显示托盘图标
                this.Hide();    //隐藏窗口
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //注意判断关闭事件Reason来源于窗体按钮，否则用菜单退出时无法退出!
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult result = MessageBox.Show("退出监控请点\"确定\"，最小化到托盘请点\"取消\"", "你确定要关闭吗！", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (result == DialogResult.OK)
                {
                    e.Cancel = false;  //点击OK
                }
                else
                {
                    e.Cancel = true;    //取消"关闭窗口"事件
                    this.WindowState = FormWindowState.Minimized;    //使关闭时窗口向右下角缩小的效果
                    notifyIcon.Visible = true;
                    this.Hide();
                    return;
                }

            }
        }


        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.Visible)
            {
                this.WindowState = FormWindowState.Minimized;
                this.notifyIcon.Visible = true;
                this.Hide();
            }
            else
            {
                this.Visible = true;
                this.WindowState = FormWindowState.Normal;
                this.Activate();
            }
        }

    }
}

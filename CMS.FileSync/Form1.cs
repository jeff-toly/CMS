using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMS.FileSync
{
    public partial class Form1 : Form
    {
        private readonly int maxBoxCount = 10000;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            Run(() =>
            {
                ListenToPort();
            });
        }
        
        private void ListenToPort()
        {
            int port = 5050;
            IPAddress localAddr = IPAddress.Parse("192.168.1.105");
            TcpListener tcpListener = new TcpListener(localAddr, port);
            try
            {
                tcpListener.Start();
                ListBoxAdd("Tcp启动成功：" + DateTime.Now.ToString());
                #region 端口监听
                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    if (tcpClient.Connected)
                    {
                        string fileName = "";
                        ListBoxAdd("Tcp连接成功：" + DateTime.Now.ToString());
                        NetworkStream stream = tcpClient.GetStream();
                        Byte[] bytes = new Byte[8];
                        string data = null;
                        List<Byte> contentList = new List<Byte>();
                        do
                        {
                            stream.Read(bytes, 0, 8);
                            contentList.AddRange(bytes);
                            string curData = Encoding.ASCII.GetString(bytes);
                            data += curData;
                        }
                        while (stream.DataAvailable);
                        //数据校验
                        Byte[] checkContent = new Byte[8] { 1, 2, 3, 4, 5, 6, 7, 8, };
                        Byte[] checkName = new Byte[8] { 1, 3, 5, 7, 2, 4, 6, 8, };
                        string strCheckContent = Encoding.ASCII.GetString(checkContent);
                        string strCheckName = Encoding.ASCII.GetString(checkName);
                        if (data.Contains(strCheckContent))
                        {
                            int checkContentIndex = data.IndexOf(strCheckContent);
                            int checkNameIndex = data.IndexOf(strCheckName);
                            fileName = data.Substring(checkContentIndex + 8, checkNameIndex);
                            ListBoxAdd("数据校验通过，校验码为：" + checkContent.ToString());
                            contentList.RemoveRange(checkContentIndex, contentList.Count - checkContentIndex);
                        }
                        else
                        {
                            ListBoxAdd("数据校验未通过");
                            continue;
                        }
                        //数据保存
                        Byte[] fileContent = contentList.ToArray();
                        ListBoxAdd("数据总长度为" + contentList.Count);
                        string dictionary = @"D:\PortListener\";
                        if (!Directory.Exists(dictionary))
                        {
                            Directory.CreateDirectory(dictionary);
                        }
                        string path = dictionary + fileName;
                        File.WriteAllBytes(path, fileContent);
                    }
                    else
                    {
                        ListBoxAdd("Tcp连接失败");
                    }
                    //Application.DoEvents();
                }
                #endregion
            }
            catch(Exception ex)
            {
                ListBoxAdd(ex.ToString());
            }
            finally
            {
                RunOnUi(() => {
                    button1.Text = "点击重连";
                    button1.Enabled = true;
                });
                tcpListener.Stop();
            }
        }

        private void ListBoxAdd(string buffer)
        {
            RunOnUi(() => {
                while(listBox1.Items.Count >= maxBoxCount)
                {
                    listBox1.Items.RemoveAt(0);
                }
                listBox1.Items.Add(buffer);
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            });
        }

        public void Run(Action act)
        {
            new Thread(new ThreadStart(act)).Start();
        }

        public void RunOnUi(Action act)
        {
            if (!this.IsDisposed) this.BeginInvoke(act);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

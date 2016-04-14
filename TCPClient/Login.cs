using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using CommunicateServer;
using BusinessObjects;
using CCWin;

namespace TCPClient
{

    public partial class Login : CCSkinMain
    {
        public Login()
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
            clientAd = new ClientAdapter(txtIp.Text.Trim(), txtPort.Text.Trim());
        }
        //Thread threadClient = null; // 创建用于接收服务端消息的 线程；  
        //Socket sockClient = null;
        private ClientAdapter clientAd = null;
        private User currentUser = null;
        private bool isConnectToServer = false;
        ChatList newForm;
        private void LoginButton_Click(object sender, EventArgs e)
        {
            login();
        }
        Thread currentThread = Thread.CurrentThread;
        private void login()
        {
            int result = clientAd.sendLoginMessage(IDText.Text.Trim(), PWText.Text.Trim()); ;
            if (result == -1)
            {
                MessageBox.Show("TCP server is disconnected");
            }
            //string strMsg = IDText.Text.Trim() + "," + PWText.Text.Trim();
            
            //byte[] arrMsg = System.Text.Encoding.UTF8.GetBytes(strMsg);
            //byte[] arrSendMsg = new byte[arrMsg.Length + 1];
            //arrSendMsg[0] = 2; // 用来登录  
            //Buffer.BlockCopy(arrMsg, 0, arrSendMsg, 1, arrMsg.Length);
            //sockClient.Send(arrSendMsg); // 发送消息；  
            //ShowMsg(strMsg);
            PWText.Clear(); 
        }
        //private void connectToServer()
        //{
        //    IPAddress ip = IPAddress.Parse(txtIp.Text.Trim());
        //    IPEndPoint endPoint = new IPEndPoint(ip, int.Parse(txtPort.Text.Trim()));
        //    sockClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //    try
        //    {
        //        //ShowMsg("与服务器连接中……");
        //        sockClient.Connect(endPoint);

        //    }
        //    catch (SocketException se)
        //    {
        //        MessageBox.Show(se.Message);
        //        return;
        //        //this.Close();  
        //    }
        //    //ShowMsg("与服务器连接成功！！！");
        //    threadClient = new Thread(RecMsg);
        //    threadClient.IsBackground = true;
        //    threadClient.Start();  
        //}
        //void RecMsg()
        //{
        //    while (true)
        //    {
        //        // 定义一个2M的缓存区；  
        //        byte[] arrMsgRec = new byte[1024 * 1024 * 2];
        //        // 将接受到的数据存入到输入  arrMsgRec中；  
        //        int length = -1;
        //        try
        //        {
        //            length = sockClient.Receive(arrMsgRec); // 接收数据，并返回数据的长度；  
        //        }
        //        catch (SocketException se)
        //        {
        //            //ShowMsg("异常；" + se.Message);
        //            return;
        //        }
        //        catch (Exception e)
        //        {
        //            //ShowMsg("异常：" + e.Message);
        //            return;
        //        }
        //        if (arrMsgRec[0] == 0) // 表示接收到的是消息数据；  
        //        {
        //            string strMsg = System.Text.Encoding.UTF8.GetString(arrMsgRec, 1, length - 1);// 将接受到的字节数据转化成字符串；  
        //            //ShowMsg(strMsg);
        //        }
        //        if (arrMsgRec[0] == 1) // 表示接收到的是文件数据；  
        //        {

        //            try
        //            {
        //                SaveFileDialog sfd = new SaveFileDialog();

        //                if (sfd.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
        //                {// 在上边的 sfd.ShowDialog（） 的括号里边一定要加上 this 否则就不会弹出 另存为 的对话框，而弹出的是本类的其他窗口，，这个一定要注意！！！【解释：加了this的sfd.ShowDialog(this)，“另存为”窗口的指针才能被SaveFileDialog的对象调用，若不加thisSaveFileDialog 的对象调用的是本类的其他窗口了，当然不弹出“另存为”窗口。】  

        //                    string fileSavePath = sfd.FileName;// 获得文件保存的路径；  
        //                    // 创建文件流，然后根据路径创建文件；  
        //                    using (FileStream fs = new FileStream(fileSavePath, FileMode.Create))
        //                    {
        //                        fs.Write(arrMsgRec, 1, length - 1);
        //                        //ShowMsg("文件保存成功：" + fileSavePath);
        //                    }
        //                }
        //            }
        //            catch (Exception aaa)
        //            {
        //                MessageBox.Show(aaa.Message);
        //            }
        //        }
        //        if (arrMsgRec[0] == 2) // Login  
        //        {
        //            string strInfo = System.Text.Encoding.UTF8.GetString(arrMsgRec, 1, length - 1);// 将接受到的字节数据转化成字符串；
        //            string[] Infos = strInfo.Split(',');
        //            if (Infos[0] == "True")
        //            {
        //                //登录成功
        //                //name = Infos[1];
        //                Thread thr = new Thread(threadPro);
        //                thr.IsBackground = true;
        //                thr.Start();
        //            }
        //            else
        //            {
        //                MessageBox.Show("ID or Password is not correct.","Login failed");
        //            }
        //        }
                
        //    }
        //}
        private void threadPro()
        {
            MethodInvoker MethInvo = new MethodInvoker(openChatList);
            BeginInvoke(MethInvo);  
        }
        private void openChatList()
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            newForm = new ChatList(currentUser, clientAd);
            newForm.closefather += new childclose(this.closethis); //closethis()是父窗体中的一个方法
            newForm.Show();
            clientAd.LoginUserReceivedEvent -= serverAdEventHandler;

            this.Hide();
        }
        private void closethis()
        {
           
            this.Close();
        }
        private void serverAdEventHandler(object sender, string strMsg)
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            currentUser = clientAd.getUserFromJsonStr(strMsg);
            if (currentUser != null)
            {
                threadPro();
            }
            else
            {
                MessageBox.Show(strMsg,"Login Exception");
            }
            //openChatList();
        }
        private void Login_Load(object sender, EventArgs e)
        {
            //connectToServer();
            //int id = Thread.CurrentThread.ManagedThreadId;

            _connectToTCPServer();
            clientAd.LoginUserReceivedEvent += serverAdEventHandler;
        }

        private void _connectToTCPServer()
        {
            clientAd.connectToTCPServer();
            ClientEndPointText.Text = clientAd.ClientEndPoint;
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            clientAd.closeTCPConnection();
        }

        private void SetButton_Click(object sender, EventArgs e)
        {
            clientAd.ServerIP = txtIp.Text.Trim();
            clientAd.ServerPort = txtPort.Text.Trim();
            _connectToTCPServer();
        }

        private void RegisterLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.RegisterLinkLabel.Links[0].LinkData = "http://172.26.203.57:8089/Register.aspx";
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());    
        }

        private void PWText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                login();
            }
        }

      

      
        
    }
}

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
using System.Data.SqlClient;
using TCPClient;
using BusinessObjects;
using System.Web.Script.Serialization;
using Json;

namespace TCPServer
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
        }
        Thread threadWatch = null; // 负责监听客户端连接请求的 线程；  
        Socket socketWatch = null;
        int ComSerialID = 1;
        Dictionary<string, Socket> dict = new Dictionary<string, Socket>();
        Dictionary<string, Thread> dictThread = new Dictionary<string, Thread>();
        /// <summary>
        /// key is remote end point
        /// value is user ident
        /// </summary>
        Dictionary<string, User> dictUser = new Dictionary<string, User>();

        string connectString = "Data Source=CN-LIA-NB1\\SQLEXPRESS;Initial Catalog=Chat_DB;User ID=sa;Password=Roding";
        SqlConnection sqlConn;
        private List<User> userList = new List<User>();
        private JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
        private void BeginListenButton_Click(object sender, EventArgs e)
        {
            // 创建负责监听的套接字，注意其中的参数；  
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // 获得文本框中的IP对象；  
            IPAddress address = IPAddress.Parse(txtIp.Text.Trim());
            // 创建包含ip和端口号的网络节点对象；  
            IPEndPoint endPoint = new IPEndPoint(address, int.Parse(txtPort.Text.Trim()));
            try
            {
                // 将负责监听的套接字绑定到唯一的ip和端口上；  
                socketWatch.Bind(endPoint);
            }
            catch (SocketException se)
            {
                MessageBox.Show("异常：" + se.Message);
                return;
            }
            // 设置监听队列的长度；  
            socketWatch.Listen(10);
            // 创建负责监听的线程；  
            threadWatch = new Thread(WatchConnecting);
            threadWatch.IsBackground = true;
            threadWatch.Start();
            ShowMsg("服务器启动监听成功！");
        }

        /// <summary>  
        /// 监听客户端请求的方法；  
        /// </summary>  
        void WatchConnecting()
        {
            while (true)  // 持续不断的监听客户端的连接请求；  
            {
                // 开始监听客户端连接请求，Accept方法会阻断当前的线程；  
                Socket sokConnection = socketWatch.Accept(); // 一旦监听到一个客户端的请求，就返回一个与该客户端通信的 套接字；  
                // 想列表控件中添加客户端的IP信息；  
                lbOnline.Items.Add(sokConnection.RemoteEndPoint.ToString());
                // 将与客户端连接的 套接字 对象添加到集合中；  
                dict.Add(sokConnection.RemoteEndPoint.ToString(), sokConnection);
                ShowMsg("客户端连接成功！");
                Thread thr = new Thread(RecMsg);
                thr.IsBackground = true;
                thr.Start(sokConnection);
                dictThread.Add(sokConnection.RemoteEndPoint.ToString(), thr);  //  将新建的线程 添加 到线程的集合中去。  
            }
        }
        private void getIP()
        {
            string hostName = Dns.GetHostName();//本机名   
            //System.Net.IPAddress[] addressList = Dns.GetHostByName(hostName).AddressList;//会警告GetHostByName()已过期，我运行时且只返回了一个IPv4的地址   
            System.Net.IPAddress[] addressList = Dns.GetHostAddresses(hostName);//会返回所有地址，包括IPv4和IPv6   
            foreach (IPAddress ip in addressList)
            {
                txtIp.Text = ip.ToString();
                return;
            }
        }
        private void removeClient(Socket sokClient,string ex)
        {
            ShowMsg("异常：" + ex);
            
            // 从 通信套接字 集合中删除被中断连接的通信套接字；  
            dict.Remove(sokClient.RemoteEndPoint.ToString());
            // 从通信线程集合中删除被中断连接的通信线程对象；  
            dictThread.Remove(sokClient.RemoteEndPoint.ToString());
            //更新数据库,删除下线的用户
            if (dictUser.ContainsKey(sokClient.RemoteEndPoint.ToString()))
            {
                userLogoffDB(dictUser[sokClient.RemoteEndPoint.ToString()].Ident);
                dictUser[sokClient.RemoteEndPoint.ToString()].IsOnline = false;
                sendUserInfoToAll(sokClient);
            }
            dictUser.Remove(sokClient.RemoteEndPoint.ToString());
            // 从列表中移除被中断的连接IP  
            lbOnline.Items.Remove(sokClient.RemoteEndPoint.ToString());
            
        }
        void RecMsg(object sokConnectionparn)
        {
            Socket sokClient = sokConnectionparn as Socket;
            while (true)
            {
                // 定义一个2M的缓存区；  
                byte[] arrMsgRec = new byte[1024 * 1024 * 2];
                // 将接受到的数据存入到输入  arrMsgRec中；  
                int length = -1;
                try
                {
                    length = sokClient.Receive(arrMsgRec); // 接收数据，并返回数据的长度；
                    if (length <= 0)
                    {
                        removeClient(sokClient, "Length isn't non negative.");
                        break;
                    }
                }
                catch (SocketException SokEx)
                {
                    removeClient(sokClient, SokEx.Message);
                    break;
                }
                catch (Exception ex)
                {
                    removeClient(sokClient, ex.Message);

                    //ShowMsg("异常：" + e.Message);
                    //// 从 通信套接字 集合中删除被中断连接的通信套接字；  
                    //dict.Remove(sokClient.RemoteEndPoint.ToString());
                    //// 从通信线程集合中删除被中断连接的通信线程对象；  
                    //dictThread.Remove(sokClient.RemoteEndPoint.ToString());
                    ////更新数据库,删除下线的用户
                    //if (dictUser.ContainsKey(sokClient.RemoteEndPoint.ToString()))
                    //{
                    //    userLogoff(dictUser[sokClient.RemoteEndPoint.ToString()]);
                    //}
                    //dictUser.Remove(sokClient.RemoteEndPoint.ToString());
                    //// 从列表中移除被中断的连接IP  
                    //lbOnline.Items.Remove(sokClient.RemoteEndPoint.ToString());
                    break;
                }
                if (arrMsgRec[0] == 0)  // 表示接收到的是数据；  
                {
                    //the message should be sended to another chatting end
                    //the current end is sokClient.RemoteEndPoint.ToString()
                    //another end is cMessage.ChatRemoteEndPoint
                    
                    string strMsg = System.Text.Encoding.UTF8.GetString(arrMsgRec, 1, length - 1);// 将接受到的字节数据转化成字符串；  
                    ChatMessage cMessage = jsonSerializer.Deserialize<ChatMessage>(strMsg);
                    ChatMessage sMessage = new ChatMessage();
                    if (cMessage != null && sokClient != null)
                    {
                        sMessage.Message = cMessage.Message;
                        sMessage.ChatRemoteEndPoint = sokClient.RemoteEndPoint.ToString();
                        string sendMsg = jsonSerializer.Serialize(sMessage);
                        sendInfoToClient(cMessage.ChatRemoteEndPoint, 0, sendMsg);
                        ShowMsg(cMessage.Message);
                    }

                    //sendToAll(strMsg);
                }
                if (arrMsgRec[0] == 1) // 表示接收到的是文件；  
                {
                    SaveFileDialog sfd = new SaveFileDialog();

                    if (sfd.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    {// 在上边的 sfd.ShowDialog（） 的括号里边一定要加上 this 否则就不会弹出 另存为 的对话框，而弹出的是本类的其他窗口，，这个一定要注意！！！【解释：加了this的sfd.ShowDialog(this)，“另存为”窗口的指针才能被SaveFileDialog的对象调用，若不加thisSaveFileDialog 的对象调用的是本类的其他窗口了，当然不弹出“另存为”窗口。】  

                        string fileSavePath = sfd.FileName;// 获得文件保存的路径；  
                        // 创建文件流，然后根据路径创建文件；  
                        using (FileStream fs = new FileStream(fileSavePath, FileMode.Create))
                        {
                            fs.Write(arrMsgRec, 1, length - 1);
                            ShowMsg("文件保存成功：" + fileSavePath);
                        }
                    }
                }
                if (arrMsgRec[0] == 2)//登录
                {
                    string queryString = "";
                    string userString = System.Text.Encoding.UTF8.GetString(arrMsgRec, 1, length - 1);// 将接受到的字节数据转化成字符串；  
                    User userFromClient = jsonSerializer.Deserialize<User>(userString);
                    User userFromServer = null;
                    string userStr = "User ID or password is not correct.";
                    string updateString = "";
                    //string[] info = logInString.Split(',');
                    //string ID = "";
                    //string PW = "";

                    if (userFromClient != null)
                    {

                        try
                        {
                            //ID = info[0];
                            //PW = info[1];
                            connectToSQLServer();
                            SqlCommand thiscommand = sqlConn.CreateCommand();
                            queryString = "select Ident, Name,IsOnline,RemoteEndPoint from [User] where Ident = '" + userFromClient.Ident + "' and Password = '" + userFromClient.Password +"'";
                            thiscommand.CommandText = queryString;
                            SqlDataReader rd = thiscommand.ExecuteReader();
                            while (rd.Read())
                            {
                                if (rd["IsOnline"].ToString() == "True")
                                {
                                    userStr = "The user " + userFromClient.Ident + " has already login";
                                    break;
                                }
                                //get user's json string in the server
                                userFromServer = new User();
                                userFromServer.Ident = rd["Ident"].ToString();
                                userFromServer.Name = rd["Name"].ToString();
                                userFromServer.RemoteEndPoint = sokClient.RemoteEndPoint.ToString();
                                userStr = jsonSerializer.Serialize(userFromServer);
                                //set the remoteendpoint and online
                                dictUser.Add(userFromServer.RemoteEndPoint, userFromServer);
                            }
                            rd.Close();
                            //it means, log in successful
                            if (userFromServer != null)
                            {
                                updateString = "update [User] set RemoteEndPoint = '" + userFromServer.RemoteEndPoint + "', IsOnline = " + 1 + " where Ident =" + userFromClient.Ident;
                                thiscommand.CommandText = updateString;
                                thiscommand.ExecuteNonQuery();
                                userFromServer.IsOnline = true;
                                sendUserInfoToAll(sokClient);
                            }
                            sendInfoToClient(sokClient.RemoteEndPoint.ToString(), 2, userStr);
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message, "Database Connect Failed");

                        }
                    }

                }
                if (arrMsgRec[0] == 3)
                {
                    //get all user's info
                    updateUserList(sokClient.RemoteEndPoint.ToString());
                }
               if(arrMsgRec[0] == 4)
               {
                   //user login or log off

               }
            }
        }
        private void resetUserInDB()
        {
            string queryString = "select Ident from [User] where IsOnline = " + 1;
            StringBuilder updateString = new StringBuilder("update [User] set IsOnline = " + 0 + ", RemoteEndPoint = '' where Ident IN (");
            bool needToReset = false;
            string ident = "";
            try
            {
                connectToSQLServer();
                SqlCommand thiscommand = sqlConn.CreateCommand();
                thiscommand.CommandText = queryString;
                SqlDataReader rd = thiscommand.ExecuteReader();
                while (rd.Read())
                {
                    needToReset = true;
                    if (!string.IsNullOrEmpty(ident))
                    {
                        updateString.Append(",");
                       
                    }
                    ident = rd["Ident"].ToString();
                    updateString.Append("'" + ident + "'");
                    //updateString = "update [User] set IsOnline = " + 0 + ", RemoteEndPoint = '' where ID = " + ID;
                   
                   
                }
                updateString.Append(")");
                rd.Close();
                thiscommand.CommandText = updateString.ToString();
                if (needToReset)
                    thiscommand.ExecuteNonQuery();
                thiscommand.Dispose();
              
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
           
        }
        private void userLogoffDB(string Ident)
        {
            string updateString = "update [User] set RemoteEndPoint = '', IsOnline = " + 0 + " where Ident = '" + Ident + "'";
            SqlCommand thiscommand = sqlConn.CreateCommand();
            thiscommand.CommandText = updateString;
            thiscommand.ExecuteNonQuery();
        }
        private void sendUserInfoToAll(Socket sokClient)
        {
            string remoteEndPoint ="";
            string message = "";
            User theUser = null;
            if (sokClient.RemoteEndPoint != null)
            {
                remoteEndPoint = sokClient.RemoteEndPoint.ToString();
                if (!string.IsNullOrEmpty(remoteEndPoint) && dictUser.ContainsKey(remoteEndPoint))
                {
                    theUser = dictUser[remoteEndPoint];
                    message = JsonHelper.SerializeObject(theUser);
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
            byte[] arrMsg = System.Text.Encoding.UTF8.GetBytes(message); // 将要发送的字符串转换成Utf-8字节数组；  

            byte[] arrSendMsg = new byte[arrMsg.Length + 1];   
            arrSendMsg[0] = 4; // 表示有用户下线  
            Buffer.BlockCopy(arrMsg, 0, arrSendMsg, 1, arrMsg.Length);
            foreach (Socket s in dict.Values)
            {
                if (s.RemoteEndPoint != sokClient.RemoteEndPoint)
                {
                    s.Send(arrSendMsg); 
                }
                
            }
        }
        //private string getUserStatusStr(string remoteEndPoint,bool )
        //{
 
        //}
        /// <summary>
        /// try to connect to SQL server
        /// </summary>
        private void connectToSQLServer()
        {
            if (sqlConn == null)
            {
                sqlConn = new SqlConnection(connectString);
            }
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
        }
        /// <summary>
        /// get user's list and send to the client
        /// </summary>
        /// <param name="clientEndPoint"></param>
        private void updateUserList(string clientEndPoint)
        {
            string queryString = "";
            try
            {
                userList.Clear();
                connectToSQLServer();
                SqlCommand thiscommand = sqlConn.CreateCommand();
                queryString = "select Ident, Name,RemoteEndPoint from [User] where IsOnline = 1";
                thiscommand.CommandText = queryString;
                SqlDataReader rd = thiscommand.ExecuteReader();
                while (rd.Read())
                {
                    User theUser = new User();
                    theUser.Ident = rd["Ident"].ToString();
                    theUser.Name = rd["Name"].ToString();
                    theUser.RemoteEndPoint = rd["RemoteEndPoint"].ToString();
                    userList.Add(theUser);
                }
                rd.Close();
                string userListStr = jsonSerializer.Serialize(userList);
                sendInfoToClient(clientEndPoint, 3, userListStr);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "Database Connect Failed");

            }
        }
        private void sendInfoToClient(string strKey, byte type, string message)
        {
            byte[] arrMsg = System.Text.Encoding.UTF8.GetBytes(message); // 将要发送的字符串转换成Utf-8字节数组；  

            byte[] arrSendMsg = new byte[arrMsg.Length + 1]; // 上次写的时候把这一段给弄掉了，实在是抱歉哈~ 用来标识发送是数据而不是文件，如果没有这一段的客户端就接收不到消息了~~~  
            arrSendMsg[0] = type; // 表示登录消息  
            Buffer.BlockCopy(arrMsg, 0, arrSendMsg, 1, arrMsg.Length);
            strKey = strKey.Trim();
            if (!string.IsNullOrEmpty(strKey))
            {
                dict[strKey].Send(arrSendMsg);// 解决了 sokConnection是局部变量，不能再本函数中引用的问题；  
            }
        }
        void ShowMsg(string str)
        {
            txtMsg.AppendText(str + "\r\n");
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string strMsg = "服务器" + "\r\n" + "   -->" + txtMsgSend.Text.Trim() + "\r\n";
            byte[] arrMsg = System.Text.Encoding.UTF8.GetBytes(strMsg); // 将要发送的字符串转换成Utf-8字节数组；  
            byte[] arrSendMsg = new byte[arrMsg.Length + 1];
            arrSendMsg[0] = 0; // 表示发送的是消息数据  
            Buffer.BlockCopy(arrMsg, 0, arrSendMsg, 1, arrMsg.Length);
            string strKey = "";
            strKey = lbOnline.Text.Trim();
            if (string.IsNullOrEmpty(strKey))   // 判断是不是选择了发送的对象；  
            {
                MessageBox.Show("请选择你要发送的好友！！！");
            }
            else
            {
                dict[strKey].Send(arrSendMsg);// 解决了 sokConnection是局部变量，不能再本函数中引用的问题；  
                ShowMsg(strMsg);
                txtMsgSend.Clear();
            }
        }

        private void sendToAll(string message)
        {
            byte[] arrMsg = System.Text.Encoding.UTF8.GetBytes(message); // 将要发送的字符串转换成Utf-8字节数组；  

            byte[] arrSendMsg = new byte[arrMsg.Length + 1]; // 上次写的时候把这一段给弄掉了，实在是抱歉哈~ 用来标识发送是数据而不是文件，如果没有这一段的客户端就接收不到消息了~~~  
            arrSendMsg[0] = 0; // 表示发送的是消息数据  
            Buffer.BlockCopy(arrMsg, 0, arrSendMsg, 1, arrMsg.Length);
            foreach (Socket s in dict.Values)
            {
                s.Send(arrSendMsg);
            }
        }

        private void btnSendToAll_Click(object sender, EventArgs e)
        {
            string strMsg = "服务器" + "\r\n" + "   -->" + txtMsgSend.Text.Trim() + "\r\n";
            sendToAll(strMsg);
            ShowMsg(strMsg);
            txtMsgSend.Clear();
            ShowMsg(" 群发完毕～～～");
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = "D:\\";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtSelectFile.Text = ofd.FileName;
            }
        }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSelectFile.Text))
            {
                MessageBox.Show("请选择你要发送的文件！！！");
            }
            else
            {
                // 用文件流打开用户要发送的文件；  
                using (FileStream fs = new FileStream(txtSelectFile.Text, FileMode.Open))
                {
                    string fileName = System.IO.Path.GetFileName(txtSelectFile.Text);
                    string fileExtension = System.IO.Path.GetExtension(txtSelectFile.Text);
                    string strMsg = "我给你发送的文件为： " + fileName + fileExtension + "\r\n";
                    byte[] arrMsg = System.Text.Encoding.UTF8.GetBytes(strMsg); // 将要发送的字符串转换成Utf-8字节数组；  
                    byte[] arrSendMsg = new byte[arrMsg.Length + 1];
                    arrSendMsg[0] = 0; // 表示发送的是消息数据  
                    Buffer.BlockCopy(arrMsg, 0, arrSendMsg, 1, arrMsg.Length);
                    bool fff = true;
                    string strKey = "";
                    strKey = lbOnline.Text.Trim();
                    if (string.IsNullOrEmpty(strKey))   // 判断是不是选择了发送的对象；  
                    {
                        MessageBox.Show("请选择你要发送的好友！！！");
                    }
                    else
                    {
                        dict[strKey].Send(arrSendMsg);// 解决了 sokConnection是局部变量，不能再本函数中引用的问题；  
                        byte[] arrFile = new byte[1024 * 1024 * 2];
                        int length = fs.Read(arrFile, 0, arrFile.Length);  // 将文件中的数据读到arrFile数组中；  
                        byte[] arrFileSend = new byte[length + 1];
                        arrFileSend[0] = 1; // 用来表示发送的是文件数据；  
                        Buffer.BlockCopy(arrFile, 0, arrFileSend, 1, length);
                        // 还有一个 CopyTo的方法，但是在这里不适合； 当然还可以用for循环自己转化；  
                        //  sockClient.Send(arrFileSend);// 发送数据到服务端；  
                        dict[strKey].Send(arrFileSend);// 解决了 sokConnection是局部变量，不能再本函数中引用的问题；  
                        txtSelectFile.Clear();
                    }
                }
            }
            txtSelectFile.Clear();
        }

        private void Server_Load(object sender, EventArgs e)
        {
            txtIp.Text = GetLocalIp();
            resetUserInDB();

        }
        static string GetLocalIp()
        {
            string hostname = Dns.GetHostName();//得到本机名   
            //IPHostEntry localhost = Dns.GetHostByName(hostname);//方法已过期，只得到IPv4的地址   
            IPHostEntry localhost = Dns.GetHostEntry(hostname);
            foreach (IPAddress localAddr in localhost.AddressList)
            {
                if (localAddr.AddressFamily == AddressFamily.InterNetwork)
                {
                    return localAddr.ToString();
                }
            }
            return localhost.AddressList[0].ToString();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Login client = new Login();
            client.Show();
        }

        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (User curUser in dictUser.Values)
            {
                userLogoffDB(curUser.Ident);
            }
        }
    }
}

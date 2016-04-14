using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BusinessObjects;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using Json;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace CommunicateServer
{
    public enum ClientUpdateType
    {
        UpdateUserList = 0,
        changeItemColor = 1,
        UserStateChange = 2
    }
    public delegate void ParameterizedThreadStart(object sender, string msg);
    public class ClientAdapter
    {
        #region "field"
        #endregion
        private Thread threadClient = null; // 创建用于接收服务端消息的 线程；  
        private Socket sockClient = null;
        private Form _form;
        // private User theUser = null;

        private Dictionary<string, User> userList = new Dictionary<string, User>();
        /// <summary>
        /// key is remote end point
        /// value is dialog open status
        /// </summary>
        private Dictionary<string, bool> usersChatOpenStatus = new Dictionary<string, bool>();
        /// <summary>
        /// key is remote end point
        /// value is ChatClient form
        /// </summary>
        private Dictionary<string, Form> formList = new Dictionary<string, Form>();


        public event ParameterizedThreadStart LoginUserReceivedEvent;
        public event ParameterizedThreadStart UserListReceivedEvent;
        public event ParameterizedThreadStart ChatMsgReceivedEvent;
        public event ParameterizedThreadStart UserLogInfoEvent;
        private JavaScriptSerializer Serializer = new JavaScriptSerializer();

        #region "Constructor"
        public ClientAdapter(string serverIP, string serverPort)
        {
            _isConnectToTCPServer = false;
            _serverIP = serverIP;
            _serverPort = serverPort;
            userList.Clear();
            usersChatOpenStatus.Clear();
        }
        #endregion

        #region "Property"
        private bool _isConnectToTCPServer;
        public bool IsConnectToTCPServer
        {
            get { return _isConnectToTCPServer; }

        }

        private string _clientEndPoint;
        /// <summary>
        /// 
        /// </summary>
        public string ClientEndPoint
        {
            get
            {
                if (sockClient != null && sockClient.LocalEndPoint != null)
                {
                    _clientEndPoint = sockClient.LocalEndPoint.ToString();
                }
                else
                {
                    _clientEndPoint = "";
                }
                return _clientEndPoint;
            }
        }


        private string _serverIP;

        public string ServerIP
        {
            get { return _serverIP; }
            set { _serverIP = value; }
        }
        private string _serverPort;

        public string ServerPort
        {
            get { return _serverPort; }
            set { _serverPort = value; }
        }


        #endregion



        #region "Public method"
        /// <summary>
        /// 
        /// </summary>
        public void closeTCPConnection()
        {
            if (sockClient != null)
            {
                sockClient.Close();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool connectToTCPServer()
        {

            IPAddress ip = IPAddress.Parse(ServerIP);
            IPEndPoint endPoint = new IPEndPoint(ip, int.Parse(ServerPort));
            sockClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                //ShowMsg("与服务器连接中……");
                sockClient.Connect(endPoint);
                _isConnectToTCPServer = true;
            }
            catch (SocketException se)
            {
                //MessageBox.Show(se.Message);
                return _isConnectToTCPServer;
                //this.Close();  
            }
            //ShowMsg("与服务器连接成功！！！");
            threadClient = new Thread(recMsg);
            threadClient.IsBackground = true;
            threadClient.Start();
            return _isConnectToTCPServer;
        }
        /// <summary>
        /// get user's name by remote end point
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <returns></returns>
        public string getUserName(string remoteEndPoint)
        {
            string userName = "";
            if (!string.IsNullOrEmpty(remoteEndPoint) && userList.ContainsKey(remoteEndPoint))
            {
                User user = userList[remoteEndPoint];
                userName = user.Name;
            }
            return userName;
        }
        /// <summary>
        /// get user's name by remote end point
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <returns></returns>
        public string getUserIdent(string remoteEndPoint)
        {
            string userIdent = "";
            if (!string.IsNullOrEmpty(remoteEndPoint) && userList.ContainsKey(remoteEndPoint))
            {
                User user = userList[remoteEndPoint];
                userIdent = user.Ident;
            }
            return userIdent;
        }
        /// <summary>
        /// get user's name by remote end point
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <returns></returns>
        public User getUser(string remoteEndPoint)
        {
            User user = null;
            if (!string.IsNullOrEmpty(remoteEndPoint) && userList.ContainsKey(remoteEndPoint))
            {
                user = userList[remoteEndPoint];
            }
            return user;
        }

        public void setUserChatStatus(string remoteEndPoint, bool status)
        {
            if (!string.IsNullOrEmpty(remoteEndPoint) && usersChatOpenStatus.ContainsKey(remoteEndPoint))
            {
                usersChatOpenStatus[remoteEndPoint] = status;
            }
            else
            {
                usersChatOpenStatus.Add(remoteEndPoint, status);
            }

        }
        public bool getUserChatStatus(string remoteEndPoint)
        {
            bool status = false;
            if (!string.IsNullOrEmpty(remoteEndPoint) && usersChatOpenStatus.ContainsKey(remoteEndPoint))
            {
                status = usersChatOpenStatus[remoteEndPoint];
            }
            return status;
        }
        /// <summary>
        /// 
        /// </summary>
        /// System.Collections.Generic.Dictionary<string, User>.ValueCollection
        /// <returns></returns>
        public List<User> getUserList()
        {
            List<User> tempList = new List<User>();

            foreach (User curUser in userList.Values)
            {
                tempList.Add(curUser);
            }
            return tempList;
        }
        public void updateUserList(string strMsg)
        {
            userList.Clear();
            usersChatOpenStatus.Clear();
            foreach (User theUser in getUserListFromJsonStr(strMsg))
            {
                if (!string.IsNullOrEmpty(theUser.RemoteEndPoint))
                {
                    if (!userList.ContainsKey(theUser.RemoteEndPoint))
                    {
                        userList.Add(theUser.RemoteEndPoint, theUser);
                        usersChatOpenStatus.Add(theUser.RemoteEndPoint, false);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public User getUserFromJsonStr(string jsonStr)
        {
            User theUser = null;
            try
            {
                theUser = Serializer.Deserialize<User>(jsonStr);
            }
            catch (Exception ex)
            {

            }

            return theUser;
        }
        public List<User> getUserListFromJsonStr(string jsonStr)
        {
            List<User> userList = new List<User>();
            try
            {
                userList = Serializer.Deserialize<List<User>>(jsonStr);
            }
            catch (Exception ex)
            {

            }
            return userList;
        }
        public ChatMessage getChatMsgFromJsonStr(string jsonStr)
        {
            ChatMessage sMsg = null;
            try
            {
                sMsg = Serializer.Deserialize<ChatMessage>(jsonStr);
            }
            catch (Exception ex)
            {

            }

            return sMsg;
        }
        public int sendMessage(string strMsg, byte type)
        {
            int result = -1;
            //string strMsg = IDText.Text.Trim() + "," + PWText.Text.Trim();
            byte[] arrMsg = System.Text.Encoding.UTF8.GetBytes(strMsg);
            byte[] arrSendMsg = new byte[arrMsg.Length + 1];
            arrSendMsg[0] = type; // 用来登录  
            Buffer.BlockCopy(arrMsg, 0, arrSendMsg, 1, arrMsg.Length);
            if (sockClient != null)
            {
                result = sockClient.Send(arrSendMsg); // 发送消息；  
            }
            return result;

        }
        /// <summary>
        /// log in TCP server from client
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="PW"></param>
        public int sendLoginMessage(string ID, string PW)
        {
            if (!IsConnectToTCPServer)
            {
                if (!connectToTCPServer())
                {
                    return -1;
                }
            }
            byte type = 2;
            User theUser = new User();
            theUser.Ident = ID;
            theUser.Password = PW;
            theUser.RemoteEndPoint = ClientEndPoint;
            string strMsg = Serializer.Serialize(theUser);
            return sendMessage(strMsg, type);
        }
        public void sendChatMessage(string chatMsg, string remoteEndPoint)
        {
            byte type = 0;
            ChatMessage cMessgae = new ChatMessage();
            cMessgae.Message = chatMsg;
            cMessgae.ChatRemoteEndPoint = remoteEndPoint;
            string strMsg = Serializer.Serialize(cMessgae);
            sendMessage(strMsg, type);

        }
        public void removeChatForm(string remoteEndPoint)
        {
            if (!string.IsNullOrEmpty(remoteEndPoint) && formList.ContainsKey(remoteEndPoint))
            {
                formList.Remove(remoteEndPoint);
            }
        }
        public Form getChatForm(string remoteEndPoint)
        {
            Form chat = null;
            if (!string.IsNullOrEmpty(remoteEndPoint) && formList.ContainsKey(remoteEndPoint))
            {
                chat = formList[remoteEndPoint];
            }
            return chat;
        }
        public void addChatForm(string remoteEndPoint, Form chat)
        {
            if (!string.IsNullOrEmpty(remoteEndPoint) && !formList.ContainsKey(remoteEndPoint))
            {
                formList.Add(remoteEndPoint, chat);
            }
        }
        #endregion


        #region "Private method"
        /// <summary>
        /// call back after message is recevied from TCP server
        /// </summary>
        private void recMsg()
        {
            while (true)
            {
                // 定义一个2M的缓存区；  
                byte[] arrMsgRec = new byte[1024 * 1024 * 2];
                // 将接受到的数据存入到输入  arrMsgRec中；  
                int length = -1;
                try
                {
                    length = sockClient.Receive(arrMsgRec); // 接收数据，并返回数据的长度；  
                }
                catch (SocketException se)
                {
                    //ShowMsg("异常；" + se.Message);
                    return;
                }
                catch (Exception e)
                {
                    //ShowMsg("异常：" + e.Message);
                    return;
                }
                if (arrMsgRec[0] == 0) // 表示接收到的是消息数据；  
                {
                    string strMsg = System.Text.Encoding.UTF8.GetString(arrMsgRec, 1, length - 1);// 将接受到的字节数据转化成字符串；  
                    ChatMsgReceivedEvent(sockClient, strMsg);
                    //ShowMsg(strMsg);
                }
                if (arrMsgRec[0] == 1) // 表示接收到的是文件数据；  
                {

                    try
                    {
                        //SaveFileDialog sfd = new SaveFileDialog();

                        //if (sfd.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                        //{// 在上边的 sfd.ShowDialog（） 的括号里边一定要加上 this 否则就不会弹出 另存为 的对话框，而弹出的是本类的其他窗口，，这个一定要注意！！！【解释：加了this的sfd.ShowDialog(this)，“另存为”窗口的指针才能被SaveFileDialog的对象调用，若不加thisSaveFileDialog 的对象调用的是本类的其他窗口了，当然不弹出“另存为”窗口。】  

                        //    string fileSavePath = sfd.FileName;// 获得文件保存的路径；  
                        //    // 创建文件流，然后根据路径创建文件；  
                        //    using (FileStream fs = new FileStream(fileSavePath, FileMode.Create))
                        //    {
                        //        fs.Write(arrMsgRec, 1, length - 1);
                        //        //ShowMsg("文件保存成功：" + fileSavePath);
                        //    }
                        //}
                    }
                    catch (Exception aaa)
                    {
                        //MessageBox.Show(aaa.Message);
                    }
                }
                if (arrMsgRec[0] == 2) // Login  
                {
                    string strInfo = System.Text.Encoding.UTF8.GetString(arrMsgRec, 1, length - 1);// 将接受到的字节数据转化成字符串；
                    //User theUser = getUserFromJsonStr(strInfo);
                    LoginUserReceivedEvent(sockClient, strInfo);
                    //if (theUser != null)
                    //{
                    //    //登录成功
                    //    //name = Infos[1];
                    //    messageReceivedEvent(sockClient,strInfo);
                    //    //Thread thr = new Thread(threadPro);
                    //    //thr.IsBackground = true;
                    //    //thr.Start();
                    //}
                    //else
                    //{
                    //    MessageBox.Show("ID or Password is not correct.", "Login failed");
                    //}
                }
                if (arrMsgRec[0] == 3)
                {
                    string strInfo = System.Text.Encoding.UTF8.GetString(arrMsgRec, 1, length - 1);// 将接受到的字节数据转化成字符串；
                    UserListReceivedEvent(sockClient, strInfo);
                }
                if (arrMsgRec[0] == 4)
                {
                    try
                    {
                        string userStr = System.Text.Encoding.UTF8.GetString(arrMsgRec, 1, length - 1);// remote end point
                        User theUser = JsonHelper.DeserializeJsonToObject<User>(userStr);
                        if (theUser != null && !userList.ContainsKey(theUser.RemoteEndPoint))
                        {
                            userList.Add(theUser.RemoteEndPoint, theUser);
                            usersChatOpenStatus.Add(theUser.RemoteEndPoint, false);
                        }
                        UserLogInfoEvent(sockClient, userStr);
                    }
                    catch (Exception UserStatusEx)
                    {

                    }

                }
            }
        }
        #endregion

    }

    public class ServerAdapter
    {
        private string ip = "172.26.203.57";
        private string port = "8088";
        private string configFileName = "Config.INI";
        private string defaultPath = "C:\\Allen";
        Thread threadWatch = null; // 负责监听客户端连接请求的 线程；  
        Socket socketWatch = null;

        List<string> clientsOnLine = new List<string>();
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
        //private JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
         #region "Constructor"
        public ServerAdapter()
        {
            readConfigFile();
        }
        #endregion

        public void StartTCPService()
        {

            // 创建负责监听的套接字，注意其中的参数；  
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // 获得文本框中的IP对象；  
            IPAddress address = IPAddress.Parse(ip);
            // 创建包含ip和端口号的网络节点对象；  
            IPEndPoint endPoint = new IPEndPoint(address, int.Parse(port));
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
            resetUserInDB();
            //ShowMsg("服务器启动监听成功！");  
        }
        /// <summary>  
        /// 监听客户端请求的方法；  
        /// </summary>  
        private void WatchConnecting()
        {
            try
            {
                while (true)  // 持续不断的监听客户端的连接请求；  
                {
                    // 开始监听客户端连接请求，Accept方法会阻断当前的线程；  
                    Socket sokConnection = socketWatch.Accept(); // 一旦监听到一个客户端的请求，就返回一个与该客户端通信的 套接字；  
                    // 想列表控件中添加客户端的IP信息；  
                    clientsOnLine.Add(sokConnection.RemoteEndPoint.ToString());
                    //lbOnline.Items.Add(sokConnection.RemoteEndPoint.ToString());
                    // 将与客户端连接的 套接字 对象添加到集合中；  
                    dict.Add(sokConnection.RemoteEndPoint.ToString(), sokConnection);
                    //ShowMsg("客户端连接成功！");
                    Thread thr = new Thread(RecMsg);
                    thr.IsBackground = true;
                    thr.Start(sokConnection);
                    dictThread.Add(sokConnection.RemoteEndPoint.ToString(), thr);  //  将新建的线程 添加 到线程的集合中去。  
                }
            }
            catch (Exception ex)
            {

            }

        }
        private void removeClient(Socket sokClient, string ex)
        {
            //ShowMsg("异常：" + ex);

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
            //lbOnline.Items.Remove(sokClient.RemoteEndPoint.ToString());

        }
        private void RecMsg(object sokConnectionparn)
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
                    if (arrMsgRec[0] == 0)  // 表示接收到的是数据；  
                    {
                        //the message should be sended to another chatting end
                        //the current end is sokClient.RemoteEndPoint.ToString()
                        //another end is cMessage.ChatRemoteEndPoint

                        string strMsg = System.Text.Encoding.UTF8.GetString(arrMsgRec, 1, length - 1);// 将接受到的字节数据转化成字符串；  
                        ChatMessage cMessage = JsonHelper.DeserializeJsonToObject<ChatMessage>(strMsg);
                        ChatMessage sMessage = new ChatMessage();
                        if (cMessage != null && sokClient != null)
                        {
                            sMessage.Message = cMessage.Message;
                            sMessage.ChatRemoteEndPoint = sokClient.RemoteEndPoint.ToString();
                            string sendMsg = JsonHelper.SerializeObject(sMessage);
                            sendInfoToClient(cMessage.ChatRemoteEndPoint, 0, sendMsg);
                            //ShowMsg(cMessage.Message);
                        }

                        //sendToAll(strMsg);
                    }
                    if (arrMsgRec[0] == 1) // 表示接收到的是文件；  
                    {
                        SaveFileDialog sfd = new SaveFileDialog();

                        //if (sfd.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                        //{// 在上边的 sfd.ShowDialog（） 的括号里边一定要加上 this 否则就不会弹出 另存为 的对话框，而弹出的是本类的其他窗口，，这个一定要注意！！！【解释：加了this的sfd.ShowDialog(this)，“另存为”窗口的指针才能被SaveFileDialog的对象调用，若不加thisSaveFileDialog 的对象调用的是本类的其他窗口了，当然不弹出“另存为”窗口。】  

                        //    string fileSavePath = sfd.FileName;// 获得文件保存的路径；  
                        //    // 创建文件流，然后根据路径创建文件；  
                        //    using (FileStream fs = new FileStream(fileSavePath, FileMode.Create))
                        //    {
                        //        fs.Write(arrMsgRec, 1, length - 1);
                        //        ShowMsg("文件保存成功：" + fileSavePath);
                        //    }
                        //}
                    }
                    if (arrMsgRec[0] == 2)//登录
                    {
                        string queryString = "";
                        string userString = System.Text.Encoding.UTF8.GetString(arrMsgRec, 1, length - 1);// 将接受到的字节数据转化成字符串；  
                        User userFromClient = JsonHelper.DeserializeJsonToObject<User>(userString);
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
                                queryString = "select Ident, Name,Gender,IsOnline,RemoteEndPoint from [User] where Ident = '" + userFromClient.Ident + "' and Password = '" + userFromClient.Password + "'";
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
                                    userFromServer.Gender = bool.Parse(rd["Gender"].ToString());
                                    userStr = JsonHelper.SerializeObject(userFromServer);
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
                    if (arrMsgRec[0] == 4)
                    {
                        //user login or log off

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
            string remoteEndPoint = "";
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
        /// <summary>
        /// Read configuration file
        /// Get IP
        /// Get Port
        /// path name is stored in environment variable "MBConfigPath"
        /// file name is "Config.INI"
        /// </summary>
        private void readConfigFile()
        {
            string fullPath = "";
            string tempPath = "";
            string[] lineArray;
            //to get the full path of configuration file
            tempPath = Environment.GetEnvironmentVariable("MBConfigPath");
            if (string.IsNullOrEmpty(tempPath))
            {
                Environment.SetEnvironmentVariable("MBConfigPath", defaultPath);
                fullPath = defaultPath + "\\" + configFileName;
            }
            else
            {
                fullPath = tempPath + "\\" + configFileName;
            }
            //prepare to open the configuration file
            if (!Directory.Exists(fullPath))//if not exists, create one
            {
                Directory.CreateDirectory(fullPath);
                //need to log
            }
            else
            {
                StreamReader sr = new StreamReader(fullPath, Encoding.ASCII);
                string lineText;
                while ((lineText = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(lineText))
                    {
                        //the line is white space or empty line, just keep it

                        continue;
                    }
                    else
                    {
                        lineArray = lineText.Split('=');
                        if (lineArray.Length == 2)
                        {
                            if (!string.IsNullOrEmpty(lineArray[0]) && lineArray[0].ToUpper() == "IP")
                            {
                                ip = lineArray[1];
                            }
                            if (!string.IsNullOrEmpty(lineArray[0]) && lineArray[0].ToUpper() == "PORT")
                            {
                                port = lineArray[1];
                            }
                        }
                        else
                        {
                            //exception, need to log
                        }    
                    }
                }
                sr.Close();
            }
           
           

            
        }


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
                queryString = "select Ident, Name,Gender,RemoteEndPoint from [User] where IsOnline = 1";
                thiscommand.CommandText = queryString;
                SqlDataReader rd = thiscommand.ExecuteReader();
                while (rd.Read())
                {
                    User theUser = new User();
                    theUser.Ident = rd["Ident"].ToString();
                    theUser.Name = rd["Name"].ToString();
                    theUser.Gender = bool.Parse(rd["Gender"].ToString());
                    theUser.RemoteEndPoint = rd["RemoteEndPoint"].ToString();
                    userList.Add(theUser);
                }
                rd.Close();
                string userListStr = JsonHelper.SerializeObject(userList);
                sendInfoToClient(clientEndPoint, 3, userListStr);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "Database Connect Failed");

            }
        }
        /// <summary>
        /// common method to send info to client
        /// </summary>
        /// <param name="strKey"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
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

        /// <summary>
        /// it's for test.
        /// </summary>
        /// <param name="message"></param>
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
        private string GetLocalIp()
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
        public void StopTCPService()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            foreach (User curUser in dictUser.Values)
            {
                userLogoffDB(curUser.Ident);
            }
            socketWatch.Close();
            threadWatch.Abort();
            threadWatch = null; // 负责监听客户端连接请求的 线程；  
            socketWatch = null;
            clientsOnLine = new List<string>();
            dict = new Dictionary<string, Socket>();
            dictThread = new Dictionary<string, Thread>();
        }
    }
}

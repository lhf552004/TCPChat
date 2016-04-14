using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BusinessObjects;
using CommunicateServer;
using CCWin;

namespace TCPClient
{
    public delegate void childclose();
    public partial class ChatList : CCSkinMain
    {

        public event childclose closefather;
        public ChatList(User theUser, ClientAdapter clientAd)
        {
            InitializeComponent();
            this._currentUser = theUser;
            _clientAd = clientAd;
            if (_currentUser != null)
                this.Text = _currentUser.Name;
        }


        #region "field"
        private User _currentUser;
        private ClientAdapter _clientAd;
        private string _listViewKey;
        /// <summary>
        /// Talk with the user
        /// </summary>
        private User theUser = null;
        private ClientUpdateType _type;
        //private List<User> userList = new List<User>();
        //private List<ChatMessage> chatList = new List<ChatMessage>();
        /// <summary>
        /// key is remote end point
        /// value is chat message
        /// </summary>
        private Dictionary<string, string> chatList = new Dictionary<string, string>();
        
        #endregion

        #region "private method"
        /// <summary>
        ///Cross thread access control
        /// </summary>
        private void threadPro()
        {
            MethodInvoker MethInvo = new MethodInvoker(_updateClientForm);
            BeginInvoke(MethInvo);
        }
        private void _updateClientForm()
        {
            switch (_type)
            {
                case ClientUpdateType.UpdateUserList:
                    _updateUserList();
                    break;
                case ClientUpdateType.changeItemColor:
                    _changeItemColor();
                    break;
                case ClientUpdateType.UserStateChange:
                    _updateUserInfo();
                    break;

            }
        }
        /// <summary>
        /// update the user list in the form
        /// </summary>
        private void _updateUserList()
        {
            UsersListView.Items.Clear();
            foreach (User theUser in _clientAd.getUserList())
            {
                
                ListViewItem newItem = new ListViewItem(theUser.Name + "," + theUser.RemoteEndPoint);
                if (chatList.ContainsKey(theUser.RemoteEndPoint))
                {
                    //if the message is not null, then set the item's back color to red
                    if (!string.IsNullOrEmpty(chatList[theUser.RemoteEndPoint]))
                    {
                        newItem.BackColor = Color.Red;
                    }
                }
                UsersListView.Items.Add(newItem);
            }
            
        }
        private void _changeItemColor()
        {
            foreach (ListViewItem item in UsersListView.Items)
            {
                if (item.Text.Contains(_listViewKey))
                {
                    item.BackColor = Color.Red;
                }
            }
            //int index = -1;
            //if (UsersListView.Items.ContainsKey(_listViewKey))
            //{
            //   index= UsersListView.Items.IndexOfKey(_listViewKey);
            //   UsersListView.Items[index].BackColor = Color.Red;
            //}
        }
       
        
        //private void threadPro2()
        //{
        //    MethodInvoker MethInvo = new MethodInvoker(_changeItemColor);
        //    BeginInvoke(MethInvo);
        //}
        //private void threadPro3()
        //{
        //    MethodInvoker MethInvo = new MethodInvoker(_updateUserInfo);
        //    BeginInvoke(MethInvo);
        //}
        private void _updateUserInfo()
        {
            bool found = false;
            if (theUser == null)
                return;
            _listViewKey = theUser.Name + "," + theUser.RemoteEndPoint;
            foreach (ListViewItem item in UsersListView.Items)
            {
                if (item.Text.Contains(_listViewKey))
                {
                    found = true;
                    if (theUser.IsOnline == false)
                    {
                        item.Remove();
                    }
                    break;
                }
            }
            if (!found && theUser.IsOnline)
            {
                UsersListView.Items.Add(_listViewKey);
            }
            
        }

        private ChatClient _getChatClient(string RemoteEndPoint, string msg)
        {
            ChatClient theForm = _clientAd.getChatForm(RemoteEndPoint) as ChatClient;
            if (theForm== null)
            {
                theForm = new ChatClient(_clientAd, _currentUser, RemoteEndPoint, msg, this);
                _clientAd.addChatForm(RemoteEndPoint, theForm);
            }
            return theForm;
        }

        #endregion

        #region "Handler for control and event"
        private void ChatList_Load(object sender, EventArgs e)
        {
            
            chatList.Clear();
 
            _clientAd.UserListReceivedEvent += userListReceivedEventHandler;
            _clientAd.ChatMsgReceivedEvent += chatMsgReceivedEventHandler;
            _clientAd.UserLogInfoEvent += userLogEventHandler;
            _clientAd.sendMessage("", 3);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="remoteEndPoint"></param>
        private void userLogEventHandler(object sender, string strMsg)
        {
            theUser = Json.JsonHelper.DeserializeJsonToObject<User>(strMsg);
            _type = ClientUpdateType.UserStateChange;
            threadPro();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="strMsg"></param>
        private void chatMsgReceivedEventHandler(object sender, string strMsg)
        {
            ChatMessage cMsg = _clientAd.getChatMsgFromJsonStr(strMsg);
            string curMsg ="";
            string userName ="";
            bool isOpened = false;
            
            if (cMsg != null)
            {
                string tempMsg = cMsg.Message;
                if (!string.IsNullOrEmpty(cMsg.ChatRemoteEndPoint))
                {
                    isOpened = _clientAd.getUserChatStatus(cMsg.ChatRemoteEndPoint);
                    if (!isOpened)
                    {
                        userName = _clientAd.getUserName(cMsg.ChatRemoteEndPoint);
                        if (chatList.ContainsKey(cMsg.ChatRemoteEndPoint))
                        {
                            //已有消息，但是没有打开
                            curMsg = chatList[cMsg.ChatRemoteEndPoint];

                            curMsg += userName + ":\n" + tempMsg + "\n";
                            chatList[cMsg.ChatRemoteEndPoint] = curMsg;
                        }
                        else
                        {
                            //需创建新的会话  
                            curMsg += userName + ":\n" + tempMsg + "\n";
                            chatList.Add(cMsg.ChatRemoteEndPoint, curMsg);
                        }
                        _listViewKey = userName + "," + cMsg.ChatRemoteEndPoint;
                        _type = ClientUpdateType.changeItemColor;
                        threadPro();
                    }
                    
                }
            }
        }
        private void userListReceivedEventHandler(object sender, string strMsg)
        {
            //update the user list
            _clientAd.updateUserList(strMsg);
            //userList = _clientAd.getUserListFromJsonStr(strMsg);
            _type = ClientUpdateType.UpdateUserList;
            threadPro();

        }

        private void UsersListView_DoubleClick(object sender, EventArgs e)
        {
            ChatClient chat =null;
            string otherUserIdent = "";
            string userStr = UsersListView.SelectedItems[0].Text;
            string[] userInfo = userStr.Split(',');
            if (userInfo.Length != 2)
                return;
            string RemoteEndPoint = userInfo[1];
            string msg = "";
            if (chatList.ContainsKey(RemoteEndPoint))
            {
                msg = chatList[RemoteEndPoint];
                chatList[RemoteEndPoint] = "";
                UsersListView.SelectedItems[0].BackColor = Color.White;
            }
            //string ss = UsersListbox.SelectedValue.ToString();

            _clientAd.setUserChatStatus(RemoteEndPoint, true);

            chat = _getChatClient(RemoteEndPoint,msg);
            chat.Show();
        }
       
        private void timer1_Tick(object sender, EventArgs e)
        {
           // _clientAd.sendMessage("", 3);
        }
        #endregion

        private void ChatList_FormClosed(object sender, FormClosedEventArgs e)
        {
            //trigger the event to close father form
            _clientAd.UserListReceivedEvent -= userListReceivedEventHandler;
            _clientAd.ChatMsgReceivedEvent -= chatMsgReceivedEventHandler;
            _clientAd.UserLogInfoEvent -= userLogEventHandler;
            if (_clientAd != null)
            {
                _clientAd.closeTCPConnection();
            }
            closefather();
        }

        private void UsersListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            MessageBox.Show(e.Column.ToString());
        }

       

        

        private void ChatList_SizeChanged(object sender, EventArgs e)
        {
            //判断是否选择的是最小化按钮 
            if (WindowState == FormWindowState.Minimized)
            {
                //托盘显示图标等于托盘图标对象 
                //注意notifyIcon1是控件的名字而不是对象的名字 
               
                //隐藏任务栏区图标 
                this.ShowInTaskbar = false;
                //图标显示在托盘区 
                notifyIcon1.Visible = true;
                notifyIcon1.Text = _currentUser.Name;
            }

        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            //判断是否已经最小化于托盘 
            if (WindowState == FormWindowState.Minimized)
            {
                //还原窗体显示 
                WindowState = FormWindowState.Normal;
                //激活窗体并给予它焦点 
                this.Activate();
                //任务栏区显示图标 
                this.ShowInTaskbar = true;
                //托盘区图标隐藏 
                notifyIcon1.Visible = false;
            }
        }

       
       










    }
}

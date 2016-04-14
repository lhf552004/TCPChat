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
using System.Threading;
using CCWin;

namespace TCPClient
{
    public partial class ChatClient : CCSkinMain
    {
        public ChatClient(
            ClientAdapter theClientAd, 
            User theUser, 
            string chatRemoteEndpoint,
            string message,
            ChatList father)
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
            _clientAd = theClientAd;
            _curUser = theUser;
            _chatRemote = chatRemoteEndpoint;
            _clientAd.ChatMsgReceivedEvent += chatMsgReceivedEventHandler;
            this.txtMsg.Text = message;
            this.Text = "Talk with " + _clientAd.getUserName(_chatRemote);
            this._father = father;
            this._father.closefather += closethis;
        }
        private ChatList _father;
        private User _curUser;
        private User _anotherUser = null;
        private string _chatRemote;
        private ClientAdapter _clientAd;
        private string _currentMessage;
        private string _maleImageFileName = "TCPClient.Properties.Resources.Male.jpg";
        private string _femaleImageFileName = "TCPClient.Properties.Resources.Female.jpg";
        private void chatMsgReceivedEventHandler(object sender, string strMsg)
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            ChatMessage cMsg = _clientAd.getChatMsgFromJsonStr(strMsg);
            if (cMsg != null)
            {
                _currentMessage = _clientAd.getUserName(_chatRemote) +":\n" + cMsg.Message;
                threadPro();
            }
        }
        private void threadPro()
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            MethodInvoker MethInvo = new MethodInvoker(updateMessage);
            BeginInvoke(MethInvo);
        }
        private void updateMessage()
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            txtMsg.AppendText(_currentMessage + "\n");
            _currentMessage = "";
        }
        private void btnSendMsg_Click(object sender, EventArgs e)
        {
            _clientAd.sendChatMessage(txtSendMsg.Text, _chatRemote);
            _currentMessage = _curUser.Name + ":\n" + txtSendMsg.Text;
            updateMessage();
            txtSendMsg.Text = "";
        }
        private void closethis()
        {

            this.Close();
        }
        private void _setImage()
        {
            try
            {
                if (_curUser != null && _curUser.Gender)
                {
                    YourSelfpictureBox.Image = TCPClient.Properties.Resources.Male;
                }
                else
                {
                    YourSelfpictureBox.Image = TCPClient.Properties.Resources.Female;
                }
                // another user
                if (_anotherUser != null && _anotherUser.Gender)
                {
                    AnotherpictureBox.Image = TCPClient.Properties.Resources.Male;
                }
                else
                {
                    AnotherpictureBox.Image = TCPClient.Properties.Resources.Female;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        private void ChatClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            _clientAd.setUserChatStatus(_chatRemote, false);
            _clientAd.removeChatForm(_chatRemote);
            _clientAd.ChatMsgReceivedEvent -= chatMsgReceivedEventHandler;
            this._father.closefather -= closethis;
        }

        private void ChatClient_Load(object sender, EventArgs e)
        {
            _anotherUser = _clientAd.getUser(_chatRemote);
            _setImage();
            //YourSelfpictureBox.Image = Image.FromFile("");
        }

        private void txtSendMsg_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSendMsg_Click(null,null);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessObjects
{
    [Serializable]
    public class ChatMessage
    {
        public ChatMessage()
        { 
        }
        private string _Message;

        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }
        private string _chatRemoteEndPoint;

        public string ChatRemoteEndPoint
        {
            get { return _chatRemoteEndPoint; }
            set { _chatRemoteEndPoint = value; }
        }
    }
}

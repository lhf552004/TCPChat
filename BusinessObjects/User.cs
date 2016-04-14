using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessObjects
{
    [Serializable]
    public class User
    {
        public User()
        {

        }
        private int _id;

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _ident;

        public string Ident
        {
            get { return _ident; }
            set { _ident = value; }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _password;

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        private bool _isOnline;

        public bool IsOnline
        {
            get { return _isOnline; }
            set { _isOnline = value; }
        }
        private string _remoteEndPoint;

        public string RemoteEndPoint
        {
            get { return _remoteEndPoint; }
            set { _remoteEndPoint = value; }
        }

        private bool _gender;

        public bool Gender
        {
            get { return _gender; }
            set { _gender = value; }
        }

        //public int ID
        //{
        //}
        //public string Name
        //{
        //}
        //public string Password
        //{
        //}
        //public bool IsOnline
        //{ }
        //public string RemoteEndPoint
        //{ }
    }
}

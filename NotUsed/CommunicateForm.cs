using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Threading;

namespace TCPService
{
    public partial class CommunicateForm : Form
    {
        public CommunicateForm()
        {
            InitializeComponent();
           

            Thread thread = new Thread(new ThreadStart(SocketListen));
            thread.Start();
            IPAddress ipAddress = IPAddress.Any; //IPAddress.Parse("172.16.102.11");
            this.Text = ipAddress.ToString() + "正在监听...";

            tcpClient.Connect(IPAddress.Parse("127.0.0.1"), Int32.Parse("8989"));
        }
        protected delegate void UpdateDisplayDelegate(string text);
        private const int bufferSize = 8192;
        TcpClient tcpClient = new TcpClient();

        private void Communicate_Load(object sender, EventArgs e)
        {
            //IPAddress local = IPAddress.Any;
            //IPEndPoint iep = new IPEndPoint(local, 9000);
            //server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //server.Bind(iep);
            //server.Listen(20);

            //Thread tcpThread = new Thread(new ThreadStart(TcpListen));
            //tcpThread.Start;
        }
        private void sendMessage()
        {


            NetworkStream ns = tcpClient.GetStream();


            if (ns.CanWrite)
            {
                Byte[] sendBytes = Encoding.UTF8.GetBytes(SendMessageTextBox.Text);
                ns.Write(sendBytes, 0, sendBytes.Length);
            }
            else
            {
                MessageBox.Show("不能写入数据流", "终止", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                //Console.WriteLine("You cannot write data to this stream.");
                //tcpClient.Close();

                // Closing the tcpClient instance does not close the network stream.
                ns.Close();
                return;
            }


            ns.Close();
            //tcpClient.Close();
        }
        //private TcpClient ConnectToServer()
        //{
        //    TcpClient client = new TcpClient();
        //    IPHostEntry host = Dns.GetHostEntry(Properties.Settings.Default.Server);
        //    var address = (from h in host.AddressList
        //                   where h.AddressFamily == AddressFamily.InterNetwork
        //                   select h).First();
        //    client.Connect(
        //        address.ToString(), Properties.Settings.Default.ServerPort);
        //    return client;
        //}

        //private void buttonGetPictureList_Click(object sender, RoutedEventArgs e)
        //{
        //    //send data  
        //    TcpClient client = ConnectToServer();
        //    NetworkStream clientStream = client.GetStream();
        //    string request = "LIST";
        //    byte[] requestBuffer = Encoding.ASCII.GetBytes(request);
        //    clientStream.Write(requestBuffer, 0, requestBuffer.Length);

        //    //read response  
        //    byte[] responseBuffer = new byte[bufferSize];
        //    MemoryStream memStream = new MemoryStream();
        //    int bytesRead = 0;
        //    do
        //    {
        //        bytesRead = clientStream.Read(responseBuffer, 0, bufferSize);
        //        memStream.Write(responseBuffer, 0, bytesRead);

        //    } while (bytesRead > 0);
        //    clientStream.Close();
        //    client.Close();

        //    byte[] buffer = memStream.GetBuffer();
        //    string response = Encoding.ASCII.GetString(buffer);
        //    this.DataContext = response.Split(':');
        //}

        //private void buttonGetpicture_Click(object sender, EventArgs e)
        //{
        //    TcpClient client = ConnectToServer();

        //    NetworkStream clientStream = client.GetStream();
        //    string request = "FILE:" + this.listBoxFiles.SelectedItem.ToString();
        //    byte[] requestBuffer = Encoding.ASCII.GetBytes(request);
        //    clientStream.Write(requestBuffer, 0, requestBuffer.Length);

        //    byte[] responseBuffer = new byte[bufferSize];
        //    MemoryStream memStream = new MemoryStream();
        //    int bytesRead = 0;
        //    do
        //    {
        //        bytesRead = clientStream.Read(responseBuffer, 0, bufferSize);
        //        memStream.Write(responseBuffer, 0, bytesRead);

        //    } while (bytesRead > 0);
        //    clientStream.Close();
        //    client.Close();

        //    if (memStream.GetBuffer().Length != 0)
        //    {
        //        BitmapImage bitmapImage = new BitmapImage();
        //        memStream.Seek(0, SeekOrigin.Begin);
        //        bitmapImage.BeginInit();
        //        bitmapImage.StreamSource = memStream;
        //        bitmapImage.EndInit();
        //        pictureBox.Source = bitmapImage;
        //    }
        //}

        private void SendButton_Click(object sender, EventArgs e)
        {
            sendMessage();
        }

        //----------------------------------------

        public void Listen()
        {
            IPAddress ipAddress = IPAddress.Any; //IPAddress.Parse("172.16.102.11");




            TcpListener tcpListener = new TcpListener(ipAddress, 8989);
            tcpListener.Start();

            TcpClient tcpClient = tcpListener.AcceptTcpClient();


            NetworkStream ns = tcpClient.GetStream();

            StreamReader sr = new StreamReader(ns);

            string result = sr.ReadToEnd();

            Invoke(new UpdateDisplayDelegate(UpdateDisplay), new object[] { result });

            tcpClient.Close();
            tcpListener.Stop();
        }
        public void SocketListen()
        {
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(new IPEndPoint(IPAddress.Any, 8989));

            while (true)
            {
                listener.Listen(0);

                Socket socket = listener.Accept();
                Stream netStream = new NetworkStream(socket);
                StreamReader reader = new StreamReader(netStream);

                string result = reader.ReadToEnd();
                Invoke(new UpdateDisplayDelegate(UpdateDisplay), new object[] { result });

                //socket.Close();
                //listener.Close();
            }



        }

        public void UpdateDisplay(string text)
        {
            richTextBox1.Text = text;
        }

        private void CommunicateForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            tcpClient.Close();
        }

    }
}

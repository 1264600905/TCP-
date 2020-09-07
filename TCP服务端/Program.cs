using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Linq;

namespace TCP服务端
{
    class Program
    {
        static void Main(string[] args)
        {
            //单次访问
            //  SingleNetWork();
            //异步接收，单个客户端持续访问
            //   StartServerAsync();
            //异步接收，多个客户端持续访问
            //StartServerAsync_Multi();
            //Console.ReadKey();
            //异步接收，多个客户端持续访问,解决粘包和分包问题

        }
        static void SingleNetWork()//接收单个客户端一次访问
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //本机Ip 127.0.0.1
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 88);
            serverSocket.Bind(ipEndPoint);
            serverSocket.Listen(50);
            Socket clientSocket = serverSocket.Accept();
            //向客户端发送消息！
            string msg = "你好，客户端！";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);
            clientSocket.Send(data);

            //接收客户端的数据
            byte[] dataBuffer = new byte[1024];
            int count = clientSocket.Receive(dataBuffer);
            string msgReceive = System.Text.Encoding.UTF8.GetString(dataBuffer, 0, count);
            Console.Write(msgReceive);

            Console.ReadKey();
            clientSocket.Close();
            serverSocket.Close();
        }
        static  byte[] dataBuffer = new byte[1024];
        static void StartServerAsync()//异步接收，单个客户端持续访问
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //本机Ip 127.0.0.1
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 88);
            serverSocket.Bind(ipEndPoint);
            serverSocket.Listen(50);
            Socket clientSocket = serverSocket.Accept();
            //向客户端发送消息！
            string msg = "你好，客户端！";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);
            clientSocket.Send(data);

            clientSocket.BeginReceive(dataBuffer, 0, 1024,SocketFlags.None, ReceiveCallBack, clientSocket);
        }
        static void ReceiveCallBack(IAsyncResult ar)
        {
            Socket clientSocket = null;
            try
            {
                clientSocket = ar.AsyncState as Socket;
                int count = clientSocket.EndReceive(ar);
                string msg = Encoding.UTF8.GetString(dataBuffer, 0, count);
                Console.WriteLine("服务器接收数据:" + msg);
                clientSocket.BeginReceive(dataBuffer, 0, 1024, SocketFlags.None, ReceiveCallBack, clientSocket);
            }
            catch (Exception e)
            {
                clientSocket.Close();
            }
        }
        static void StartServerAsync_Multi()//异步接收，多个客户端持续访问
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //本机Ip 127.0.0.1
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 88);
            serverSocket.Bind(ipEndPoint);
            serverSocket.Listen(50);
            serverSocket.BeginAccept(AcceptCallBack, serverSocket);
        }
        static void AcceptCallBack(IAsyncResult ar)
        {
            Socket serverSocket = ar.AsyncState as Socket;
            Socket clientSocket = serverSocket.EndAccept(ar);
            string msg = "你好，客户端！";
            byte[] data = Encoding.UTF8.GetBytes(msg);
            clientSocket.Send(data);
            clientSocket.BeginReceive(dataBuffer, 0, 1024, SocketFlags.None, ReceiveCallBack, clientSocket);
            serverSocket.BeginAccept(AcceptCallBack, serverSocket);
        }

        static Message message = new Message();
        static void 启动服务器解决分包和粘包问题()
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //本机Ip 127.0.0.1
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 88);
            serverSocket.Bind(ipEndPoint);
            serverSocket.Listen(50);
            serverSocket.BeginAccept(AcceptCallBack解决分包和粘包, serverSocket);
        }

        static void AcceptCallBack解决分包和粘包(IAsyncResult ar)
        {
            Socket serverSocket = ar.AsyncState as Socket;
            Socket clientSocket = serverSocket.EndAccept(ar);
            string msg = "你好，客户端！";
            byte[] data = Encoding.UTF8.GetBytes(msg);
            clientSocket.Send(data);
            clientSocket.BeginReceive(dataBuffer, 0, 1024, SocketFlags.None, ReceiveCallBack解决分包和粘包, clientSocket);
            serverSocket.BeginAccept(AcceptCallBack解决分包和粘包, serverSocket);
        }
        static void ReceiveCallBack解决分包和粘包(IAsyncResult ar)
        {
            Socket clientSocket = null;
            try
            {
                clientSocket = ar.AsyncState as Socket;
                int count = clientSocket.EndReceive(ar);
                if (count == 0)
                {
                    clientSocket.Close();
                    return;
                }

                string msg = Encoding.UTF8.GetString(dataBuffer, 0, count);
                Console.WriteLine("服务器接收数据:" + msg);
                clientSocket.BeginReceive(dataBuffer, 0, 1024, SocketFlags.None, ReceiveCallBack解决分包和粘包, clientSocket);
            }
            catch (Exception e)
            {
                clientSocket.Close();
            }
        }
    }
    class Message
    {
        private byte[] data = new byte[2048];
        private int startIndex = 0;//开始读取的位置
        private int preLength = 0;
        private bool IsGet = false;
        private bool IsFirstRun = true;
        public bool AddData(byte[] data1)
        {
            if (IsFirstRun)
            {
                preLength = BitConverter.ToInt32(data1, 0);
                IsFirstRun = false;
            }
            if (IsGet)
            {
                int Length = BitConverter.ToInt32(data1, 0);
                data.CopyTo(data, startIndex+4);
                preLength = 0;
                startIndex = 0;
                if (Length != 0)
                {
                    preLength = Length;
                }
            }
            this.data= this.data.Concat(data1).ToArray();
            if (data.Length > (preLength+startIndex + 4))
            {
                return true;
            }
            return false;
        }
        public string GetData()
        {
            IsGet = true;
            return Encoding.UTF8.GetString(data, startIndex + 4, preLength);
        } 
    }
}

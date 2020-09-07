using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using 客户端;
using System.Linq;

namespace 客户端
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 88));
            byte[] data = new byte[1024];
            int count = clientSocket.Receive(data);
            string msg = Encoding.UTF8.GetString(data, 0, count);
            Console.WriteLine(msg);
            //while (true)
            //{
            //    string s = Console.ReadLine();
            //    clientSocket.Send(Encoding.UTF8.GetBytes(s));
            //}

            for(int a = 0; a < 100; a++)
            {
                clientSocket.Send(Message.GetByte(a+""));
            }
           

            Console.ReadKey();
            clientSocket.Close();
        }
    }
    class Message
    {
        public static byte[] GetByte(string data)//封装数据包，加上数据包长度
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            int dataLength = dataBytes.Length;
            byte[] lengthBytes = BitConverter.GetBytes(dataLength);
            byte[] newBytes = lengthBytes.Concat(dataBytes).ToArray();//给数据添加头储存字节长度
            return newBytes;
        }
    }
}

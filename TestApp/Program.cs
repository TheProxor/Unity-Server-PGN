using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using PGN;
using PGN.Data;
using PGN.General;

namespace TestApp
{
    class Program
    {

        static void Main(string[] args)
        {

            Console.Write("Введите порт для приема сообщений: ");
            int command = Int32.Parse(Console.ReadLine());

            if (command == 0)
                MyTcpClient.Use();
            else
                MyTcpServer.Use();

        }
    }
}
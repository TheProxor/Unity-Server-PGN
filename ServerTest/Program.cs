using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using PGN;
using PGN.Data;
using PGN.General;
using SocketServer.SQLite;
using GameCore.Commands;

namespace SocketServer
{
    internal class Program
    {
        static ServerHandler server; 
        static Thread listenThreadTCP;
        static Thread listenThreadUDP;

        static void ListenTCP()
        {
            while(true)
            {
                server.ListenTCP();
            }
        }

        static void ListenUDP()
        {
            while (true)
            {
                server.ListenUDP();
            }
        }

        static void Main(string[] args)
        {

            server = new ServerHandler();

            NetUser.onTcpMessageHandleCallback += ReceiveTCP;
            NetUser.onUdpMessageHandleCallback += ReceiveUDP;
            NetUser.onUserConnectedTCP += (User user) => { Console.WriteLine($"User {user.ID } was connected via TCP"); };
            NetUser.onUserDisconnectedTCP += (User user) => { Console.WriteLine($"User {user.ID } was disconnected via TCP"); };
            NetUser.onUserConnectedUDP += (User user) => { Console.WriteLine($"User {user.ID } was connected via UDP"); };
            NetUser.onUserDisconnectedUDP += (User user) => { Console.WriteLine($"User {user.ID } was disconnected via UDP"); };
            Handler.onLogReceived += (string text) => { Console.WriteLine(text); };

            server.SetLocalAdressTCP(Dns.GetHostByName("127.0.0.1").AddressList[0].ToString(), 8000);
            server.SetLocalAdressUDP(Dns.GetHostByName("127.0.0.1").AddressList[0].ToString(), 8001);

            SQL_Handler.Init("Users.db");
            ServerHandler.createUserBD = SQL_Handler.CreateUser;
            ServerHandler.getInfoFromBD = SQL_Handler.GetUserData;

            try
            {
                server = new ServerHandler();
                server.Start();

                listenThreadTCP = new Thread(new ThreadStart(ListenTCP));
                listenThreadTCP.Start();

                listenThreadUDP = new Thread(new ThreadStart(ListenUDP));
                listenThreadUDP.Start();
            }
            catch (Exception ex)
            {
                server.Stop();
                Console.WriteLine(ex.Message);
            }

            string command = string.Empty;

            while (command != "-exit")
            {
                command = Console.ReadLine();
                switch (command)
                {
                    case "-add":
                        SQL_Handler.CreateUser(new User());
                        break;
                    case "-info":
                        foreach(GameCore.UserInfo info in SQL_Handler.FindUserByName("TheProxor"))
                            Console.WriteLine(info.ToString());
                        break;
                }
            }
            server.Stop();
        }

        static void ReceiveTCP(NetData data, NetUser netUser)
        {
            if (data.data is string)
                Console.WriteLine(string.Format("{0} | {1}:   {2}    (via TCP, time = {3} ms, number = {4})", data.sender.ID, DateTime.Now.ToShortTimeString(), data.data as string, Math.Abs(DateTime.Now.Millisecond - data.time.Millisecond), data.number));
            else if (data.data is Command)
            {
                if (data.data is Command.Refresh)
                {
                    Console.WriteLine(string.Format("User {0} want to refresh", data.sender.ID));
                    netUser.info = SQL_Handler.GetUserData(data.sender.ID);
                    server.SendMessageToTCP(NetData.GetBytesData(new NetData(netUser.info as GameCore.UserInfo, data.sender)), netUser);
                }
                else if (data.data is Command.FindUser)
                {
                    Console.WriteLine(string.Format("User {0} want to find user with name {1}", data.sender.ID, (data.data as Command.FindUser).userName));
                    List<GameCore.UserInfo> result = SQL_Handler.FindUserByName((data.data as Command.FindUser).userName);
                    server.SendMessageToTCP(NetData.GetBytesData(new NetData(result, data.sender)), netUser);
                }
            }
        }

        static void ReceiveUDP(NetData data, NetUser netUser)
        {
            if (data.data is string)
                Console.WriteLine(string.Format("{0} | {1}:   {2}    (via UDP, time = {3} ms, number = {4})", data.sender.ID.Split('@')[0], DateTime.Now.ToShortTimeString(), data.data as string, Math.Abs(DateTime.Now.Millisecond - data.time.Millisecond), data.number));
        }
    }
}
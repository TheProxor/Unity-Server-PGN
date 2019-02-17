using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using PGN;
using PGN.Data;
using PGN.General;
using SocketServer.Database;
using GameCore;
using GameCore.Commands;

using Newtonsoft.Json;

namespace SocketServer
{
    internal class Program
    {
        internal static User serverUser;

        static ServerHandler server;

        static void Main(string[] args)
        {
            server = new ServerHandler();

            ServerHandler.onLogReceived += (string log) => { Console.WriteLine(log); };

            serverUser = new User("Server Callback User");
            server.RegistrUser(serverUser);


            ServerHandler.onUserConnectedTCP += (User user) => { Console.WriteLine($"User {user.ID} was connected via TCP"); };
            ServerHandler.onUserDisconnectedTCP += (User user) => {Console.WriteLine($"User {user.ID} was disconnected via TCP"); };

            ServerHandler.onUserConnectedUDP += (User user) => {  Console.WriteLine($"User {user.ID} was connected via UDP"); };
            ServerHandler.onUserDisconnectedUDP += (User user) => {  Console.WriteLine($"User {user.ID} was disconnected via UDP"); };

            //server.SetLocalAdressTCP("192.168.0.1", 8000);
           // server.SetLocalAdressUDP("192.168.0.1", 8001);
            server.SetLocalAdressTCP("127.0.0.1", 8000);
            server.SetLocalAdressUDP("127.0.0.1", 8001);

            Console.WriteLine("Server IP: " + "127.0.0.1");

            MySqlHandler.Init("Users.bd", "Attributes.config");
            ServerHandler.createUserBD = MySqlHandler.CreateUser;
            ServerHandler.getInfoFromBD = MySqlHandler.GetUserData;

            SynchronizableTypes.AddType(typeof(string), (object data, string senderID) => { Console.WriteLine($"{DateTime.Now.Minute}:{DateTime.Now.Second}:{DateTime.Now.Millisecond}"); Console.WriteLine($"{senderID}: {data as string}"); });
            SynchronizableTypes.AddType(typeof(UserValidateCommand.Refresh), RefreshUser);
            //SynchronizableTypes.AddType(typeof(UserValidateCommand.BuyCommand.BuyColor), 41, RefreshUser);

            server.Start();

            Console.ReadLine();
        }

        

        private static void RefreshUser(object data, string senderID)
        {    
            string callback = JsonConvert.SerializeObject(ServerHandler.clients[senderID].info);
            server.SendMessageViaTCP(new NetData(new UserValidateCommand.Refresh(callback), serverUser.ID, false), ServerHandler.clients[senderID]);
        }
    }
}

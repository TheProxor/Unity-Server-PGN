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
using GameCore;
using GameCore.Commands;

namespace SocketServer
{
    internal class Program
    {
        internal static User serverUser;

        static ServerHandler server;

        static void Main(string[] args)
        {
            server = new ServerHandler("Users.bd", "Attributes.config", new PGN.DataBase.SQLiteBehaivour());

            ServerHandler.onLogReceived += (string log) => { Console.WriteLine(log); };

            serverUser = new User("Server Callback User");
            server.RegistrUser(serverUser);


            ServerHandler.onUserConnectedTCP += (User user) => { Console.WriteLine($"User {user.ID} was connected via TCP"); };
            ServerHandler.onUserDisconnectedTCP += (User user) => {Console.WriteLine($"User {user.ID} was disconnected via TCP"); };

            ServerHandler.onUserConnectedUDP += (User user) => {  Console.WriteLine($"User {user.ID} was connected via UDP"); };
            ServerHandler.onUserDisconnectedUDP += (User user) => {  Console.WriteLine($"User {user.ID} was disconnected via UDP"); };

            ServerHandler.onRoomReleased += ReleaseRoomCallback;

            //server.SetLocalAdressTCP("192.168.0.1", 8000);
           // server.SetLocalAdressUDP("192.168.0.1", 8001);
            server.SetLocalAdressTCP("127.0.0.1", 8000);
            server.SetLocalAdressUDP("127.0.0.1", 8001);

            Console.WriteLine("Server IP: " + "127.0.0.1");

            SynchronizableTypes.EnableTransitNonValidTypes();

            PGN.Runtime.ScriptsCompiler.TestCompile();

            server.Start();

            Console.ReadLine();
        }

        public static void ReleaseRoomCallback(bool visable, List<PGN.Matchmaking.RoomFactor> roomFactors, List<User> users)
        {
            if(!visable)
            {
                foreach(PGN.Matchmaking.RoomFactor roomFactor in roomFactors)
                {
                    if (roomFactor is PGN.Matchmaking.RoomFactor.RoomMode)
                    {
                        var roomMode = roomFactor as PGN.Matchmaking.RoomFactor.RoomMode;
                        switch (roomMode.mode)
                        {
                            case "Default":
                                foreach (User user in users)
                                {
                                    user.info.dataAttributes["cubecoins"].SetValue((int)user.info.dataAttributes["cubecoins"].GetValue() + 100);
                                    user.info.dataAttributes["experience"].SetValue((float)user.info.dataAttributes["experience"].GetValue() + 100f / (int)user.info.dataAttributes["level"].GetValue());
                                    if ((float)user.info.dataAttributes["experience"].GetValue() >= 100f)
                                    {
                                        user.info.dataAttributes["experience"].SetValue(0f);
                                        user.info.dataAttributes["level"].SetValue((int)user.info.dataAttributes["level"].GetValue() + 1);
                                    }
                                }
                                break;
                    }
                    }
                }
            }
            else
            {

            }
        }

    }
}

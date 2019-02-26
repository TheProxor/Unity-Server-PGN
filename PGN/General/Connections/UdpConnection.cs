using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using PGN.Data;


namespace PGN.General.Connections
{
    internal class UdpConnection : Connection
    {
        public User user;

        public UdpConnection(IPEndPoint adress) : base(adress)
        {
        }

        public override void SendMessage(NetData message)
        {
            byte[] data = NetData.GetBytesData(message);
            ServerHandler.udpListener.Send(data, data.Length, adress);
        }

        public override void SendMessage(byte[] data)
        {
            ServerHandler.udpListener.Send(data, data.Length, adress);
        }

        public static void Recieve(UdpClient client, IAsyncResult ar)
        {
            GetMessage(client, ar);
        }

        private static void GetMessage(UdpClient client, IAsyncResult ar)
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 8001);
            UdpClient udpClient = (ar.AsyncState as UdpClient);
            byte[] bytes = null;
            try
            {
                bytes = udpClient.EndReceive(ar, ref iPEndPoint);
                if (bytes.Length < 2)
                {
                    throw new Exception("null bytes");
                }
            }
            catch
            {
                return;
            }
            finally
            {
                if (bytes != null)
                {
                    ushort type;
                    bool transitable;
                    NetData message = NetData.RecoverBytes(bytes, bytes.Length, out type, out transitable);
                    if (!transitable || !ServerHandler.udpConnections.ContainsKey(iPEndPoint))
                    {
                        if (message != null)
                        {
                            User udpUser = null;

                            if (ServerHandler.clients.ContainsKey(message.senderID))
                            {
                                if (ServerHandler.clients[message.senderID].udpConnection == null)
                                {
                                    ServerHandler.OnUserConnectedUDP(ServerHandler.clients[message.senderID]);
                                    UdpConnection udpConnection = new UdpConnection(iPEndPoint);
                                    udpConnection.user = ServerHandler.clients[message.senderID];
                                    ServerHandler.clients[message.senderID].udpConnection = udpConnection;
                                    ServerHandler.udpConnections.Add(iPEndPoint, udpConnection);
                                }
                                udpUser = ServerHandler.clients[message.senderID];
                            }
                            else
                            {
                                udpUser = new User(message.senderID);
                                udpUser.udpConnection = new UdpConnection(iPEndPoint);
                                ServerHandler.udpConnections.Add(iPEndPoint, udpUser.udpConnection);

                                ServerHandler.AddConnection(udpUser);

                                Matchmaking.DefaultRoom.instance.JoinToRoom(udpUser);

                                ServerHandler.OnUserConnectedUDP(udpUser);

                                if (udpUser.info == null)
                                    udpUser.info = ServerHandler.DataBaseBehaivour.GetUserData(message.senderID);
                                if (udpUser.info == null)
                                    udpUser.info = ServerHandler.DataBaseBehaivour.CreateUser(message.senderID);
                            }
                            SynchronizableTypes.InvokeTypeActionUDP(type, bytes, message, ServerHandler.clients[message.senderID]);
                        }
                    }
                    else
                        ServerHandler.udpConnections[iPEndPoint].user.currentRoom.BroadcastMessageUDP(bytes);
                }
            }
        }

    }
}


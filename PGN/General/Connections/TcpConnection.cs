using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using PGN.Data;

namespace PGN.General.Connections
{
    internal class TcpConnection : Connection
    {
        protected internal NetworkStream stream { get; private set; }

        public TcpClient tcpClient;

        public User user;

        public TcpConnection(TcpClient tcpClient, IPEndPoint adress) : base(adress)
        {
            this.tcpClient = tcpClient;
            stream = tcpClient.GetStream();
        }

        public void Recieve()
        {
            while(stream.CanRead)
                GetMessage();
        }

        public void GetMessage()
        {
            do
            {
                byte[] bytes = new byte[1024];
                int bytesCount = 0;
                try
                {
                    bytesCount = stream.Read(bytes, 0, bytes.Length);
                    if (bytesCount < 2)
                        throw new Exception("null bytes");
                }
                catch
                {
                    if (user != null)
                    {
                        ServerHandler.OnUserDisconnectedTCP(user);
                        ServerHandler.OnUserDisconnectedUDP(user);
                        ServerHandler.RemoveConnection(user);
                    }
                    Close();
                    return;
                }

                ushort type;
                bool transitable;
                NetData message = NetData.RecoverBytes(bytes, bytesCount, out type, out transitable);

                if (!transitable)
                {
                    if (message != null)
                    {

                        if (ServerHandler.clients.ContainsKey(message.senderID))
                        {
                            if (ServerHandler.clients[message.senderID].tcpConnection == null)
                            {
                                user = ServerHandler.clients[message.senderID];
                                user.tcpConnection = this;
                                ServerHandler.OnUserConnectedTCP(user);
                            }
                        }
                        else
                        {
                            user = new User(message.senderID);
                            user.tcpConnection = this;
                            ServerHandler.AddConnection(user);
                            ServerHandler.OnUserConnectedTCP(user);

                            Matchmaking.DefaultRoom.instance.JoinToRoom(user);

                            if (user.info == null)
                                user.info = ServerHandler.DataBaseBehaivour.GetUserData(message.senderID);
                            if (user.info == null)
                                user.info = ServerHandler.DataBaseBehaivour.CreateUser(message.senderID);
                        }

                        SynchronizableTypes.InvokeTypeActionTCP(type, bytes, message, ServerHandler.clients[message.senderID]);
                    }
                    else
                    {
                        if (user != null)
                        {
                            ServerHandler.OnUserDisconnectedTCP(user);
                            ServerHandler.OnUserDisconnectedUDP(user);
                            ServerHandler.RemoveConnection(user);
                        }
                        Close();
                        return;
                    }
                }
                else
                    user.currentRoom.BroadcastMessageTCP(bytes);
            }
            while (stream.DataAvailable);
        }
        

        public override void SendMessage(NetData message)
        {
            SendMessage(message.bytes);
        }

        public override void SendMessage(byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

        protected internal void Close()
        {
            if (stream != null)
                stream.Close();
            if (tcpClient != null)
                tcpClient.Close();
        }
    }
}

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
        private bool open;

        protected internal NetworkStream stream { get; private set; }

        public TcpClient tcpClient;

        public User user;

        public TcpConnection(TcpClient tcpClient, IPEndPoint adress) : base(adress)
        {
            this.tcpClient = tcpClient;
            stream = tcpClient.GetStream();
            open = true;
        }

        public void Recieve()
        {
            while(open)
                GetMessage();
        }

        public void GetMessage()
        {
            do
            {
                byte[] bytes = new byte[1024];
                int bytesCount = stream.Read(bytes, 0, bytes.Length);
                ushort type;
                NetData message = NetData.RecoverBytes(bytes, bytesCount, out type);
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

                        ServerHandler.defaultRoom.JoinToRoom(user);

                        if (user.info == null)
                            user.info = PGN.DataBase.MySqlHandler.GetUserData(message.senderID);
                        if (user.info == null)
                            user.info = PGN.DataBase.MySqlHandler.CreateUser(message.senderID);
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
                    open = false;
                    break;
                }
            }
            while (stream.DataAvailable);
        }
        

        public override void SendMessage(NetData message)
        {
            byte[] data = NetData.GetBytesData(message);
            SendMessage(data);
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using PGN.Data;
using PGN.General.Connections;
using PGN.Matchmaking;

namespace PGN.General
{
    public sealed class ServerHandler : Handler
    {
        internal static TcpListener tcpListener;
        internal static UdpClient udpListener;

        public static Dictionary<string, User> clients { get; private set; } = new Dictionary<string, User>();
        internal static Dictionary<IPEndPoint, UdpConnection> udpConnections = new Dictionary<IPEndPoint, UdpConnection>();
        internal static Dictionary<IPEndPoint, TcpConnection> tcpConnections = new Dictionary<IPEndPoint, TcpConnection>();

        public static event Action<User> onUserConnectedTCP;
        public static event Action<User> onUserDisconnectedTCP;

        public static event Action<User> onUserConnectedUDP;
        public static event Action<User> onUserDisconnectedUDP;

        private static TaskFactory taskFactoryTCP = new TaskFactory();
        private static TaskFactory taskFactoryUDP = new TaskFactory();

        internal static DataBase.DataBaseBehaivour DataBaseBehaivour;


        public ServerHandler(string databasePath, string attributesPath, DataBase.DataBaseBehaivour dataBaseBehaivour) : base()
        {
            DataBaseBehaivour = dataBaseBehaivour;
            dataBaseBehaivour.Init(databasePath, attributesPath);

            SynchronizableTypes.AddType(typeof(ValidateServerCall.Refresh),
                (object data, string id) =>
                {
                    ValidateServerCall.Refresh refresh = data as ValidateServerCall.Refresh;
                    if (refresh.info == null)
                        SendMessageViaTCP(new NetData(new ValidateServerCall.Refresh(clients[id].info), false), clients[id]);
                    else
                        clients[id].info = refresh.info;
                }
                );


            SynchronizableTypes.AddSyncSubType(typeof(DataBase.UserInfo));
            SynchronizableTypes.AddSyncSubType(typeof(DataBase.DataProperty));
        }

        internal static void OnUserConnectedTCP(User user)
        {
            onUserConnectedTCP(user);
        }

        internal static void OnUserConnectedUDP(User user)
        {
            onUserConnectedUDP(user);
        }

        internal static void OnUserDisconnectedTCP(User user)
        {
            onUserDisconnectedTCP(user);
        }

        internal static void OnUserDisconnectedUDP(User user)
        {
            onUserDisconnectedUDP(user);
        }

        internal static void AddConnection(User client)
        {
            lock (clients)
            {
                if (!clients.ContainsKey(client.ID))
                    clients.Add(client.ID, client);
            }
        }

        internal static void RemoveConnection(User client)
        {
            lock (clients)
            {
                lock (tcpConnections)
                {
                    lock (udpConnections)
                    {
                        if (clients.ContainsKey(client.ID))
                        {
                            DataBaseBehaivour.SaveUserData(client);
                            if (tcpConnections.ContainsKey(user.tcpConnection.adress))
                                tcpConnections.Remove(user.tcpConnection.adress);
                            if (udpConnections.ContainsKey(user.udpConnection.adress))
                                udpConnections.Remove(user.udpConnection.adress);
                            client.currentRoom.LeaveFromRoom(client);
                            clients.Remove(client.ID);
                        }
                    }
                }
            }
        }

        internal static void RemoveConnection(string id)
        {
            if (clients.ContainsKey(id))
                clients.Remove(id);
        }

        public void Start()
        {
            tcpListener = new TcpListener(localAddressTCP);
    
            udpListener = new UdpClient(localAddressUDP);
            udpListener.EnableBroadcast = true;
            udpListener.Client.ReceiveBufferSize = 1024;

            tcpListener.Start();
            OnLogReceived("Server was created.");

            ListenTCP();
            ListenUDP();
        }

        private void ListenTCP()
        {
            taskFactoryTCP.FromAsync(tcpListener.BeginAcceptTcpClient, tcpListener.EndAcceptTcpClient, tcpListener).ContinueWith(antecedent =>
            {           
                AcceptCallback(antecedent.Result);
            });
        }


        private void ListenUDP()
        {
            taskFactoryUDP.FromAsync(udpListener.BeginReceive, ReceiveCallback, udpListener);
        }

        internal void AcceptCallback(TcpClient tcpClient)
        {
            ListenTCP();
            TcpConnection tcpConnection = new TcpConnection(tcpClient, tcpClient.Client.RemoteEndPoint as IPEndPoint);
            tcpConnections.Add(tcpClient.Client.RemoteEndPoint as IPEndPoint, tcpConnection);
            tcpConnection.Recieve();
        }


        internal void ReceiveCallback(IAsyncResult ar)
        {
            ListenUDP();
            UdpConnection.Recieve(udpListener, ar);
        }

        public void BroadcastMessageTCP(byte[] data, User sender)
        {
            foreach (string key in clients.Keys)
            {
                if (clients[key].ID != sender.ID && clients[key].tcpConnection != null)
                    clients[key].tcpConnection.SendMessage(data);
            }
        }

        public void BroadcastMessageUDP(byte[] data, User sender)
        {
            foreach (string key in clients.Keys)
                if (clients[key].ID != sender.ID && clients[key].udpConnection != null)
                    clients[key].udpConnection.SendMessage(data);
        }


        public void SendMessageViaTCP(NetData message, User user)
        {
            clients[user.ID].tcpConnection.SendMessage(message);
        }

        public void SendMessageViaUDP(NetData message, User user)
        {
            clients[user.ID].udpConnection.SendMessage(message);
        }

        public void SendMessageViaTCP(byte[] bytes, User user)
        {
            clients[user.ID].tcpConnection.SendMessage(bytes);
        }

        public void SendMessageViaUDP(byte[] bytes, User user)
        {
            clients[user.ID].udpConnection.SendMessage(bytes);
        }


        public void Stop()
        {
            tcpListener.Stop();
            udpListener.Close();

            foreach (User client in clients.Values)
                client.tcpConnection.Close();

            OnLogReceived("Server was stopped!");
        }
    }
}

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

        public static Dictionary<string, List<Room>> freeRooms { get; private set; } = new Dictionary<string, List<Room>>();
        public static Dictionary<string, Room> rooms { get; private set; } = new Dictionary<string, Room>();

        public static ServerHandler server;

        public static event Action<User> onUserConnectedTCP;
        public static event Action<User> onUserDisconnectedTCP;

        public static event Action<User> onUserConnectedUDP;
        public static event Action<User> onUserDisconnectedUDP;

        internal static Room defaultRoom;

        private static TaskFactory taskFactoryTCP = new TaskFactory();
        private static TaskFactory taskFactoryUDP = new TaskFactory();

        //DATABASE FUNCTIONS
        public static Func<string, object> createUserBD;
        public static Func<string, object> getInfoFromBD;

        public ServerHandler() : base()
        {
            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.CreateRoom), 
                (object data, string id) =>
                {
                    MatchmakingServerCall.CreateRoom createRoom = data as MatchmakingServerCall.CreateRoom;
                    CreateRoom(clients[id], createRoom.roomFactors);
                }
                );
            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.JoinToFreeRoom),
                  (object data, string id) =>
                  {
                      MatchmakingServerCall.JoinToFreeRoom joinToFreeRoom = data as MatchmakingServerCall.JoinToFreeRoom;
                      JoinToFreeRoom(clients[id], joinToFreeRoom.roomFactors);
                  }
                );

          //  SynchronizableTypes.AddType(typeof(MatchmakingServerCall.JoinToRoom), 7, null);
          //  SynchronizableTypes.AddType(typeof(MatchmakingServerCall.LeaveFromRoom), 8, null);
          //  SynchronizableTypes.AddType(typeof(MatchmakingServerCall.GetRoomsList), 9, null);

            SynchronizableTypes.AddSyncSubType(typeof(RoomCount));
            SynchronizableTypes.AddSyncSubType(typeof(RoomMode));
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
                if (clients.ContainsKey(client.ID))
                {
                    client.currentRoom.LeaveFromRoom(client);
                    clients.Remove(client.ID);
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

            defaultRoom = new Room("defaultRoomType");
            freeRooms.Add("defaultRoomType", new List<Room>(1));
            freeRooms["defaultRoomType"].Add(defaultRoom);

            ListenTCP();
            ListenUDP();
        }

        private void JoinToFreeRoom(User user, params IRoomFactor[] roomFactors)
        {
            string key = string.Empty;
            foreach (IRoomFactor factor in roomFactors)
                key += factor.GetFactorUssage();

            if (freeRooms.ContainsKey(key))
            {
                user.currentRoom.LeaveFromRoom(user);
                freeRooms[key][0].JoinToRoom(user);
            }
            else
                CreateRoom(user, key);
        }

        private void CreateRoom(User user, string roomFactors)
        {
            Room room = new Room(roomFactors);
            room.JoinToRoom(user);
            List<Room> rooms = new List<Room>();
            rooms.Add(room);
            freeRooms.Add(roomFactors, rooms);
        }

        private void CreateRoom(User user, params IRoomFactor[] roomFactors)
        {
            string key = string.Empty;
            RoomCount count = null;
            RoomMode mode = null;

            foreach (IRoomFactor factor in roomFactors)
            {
                if (factor is RoomCount)
                    count = factor as RoomCount;
                else if (factor is RoomMode)
                    mode = factor as RoomMode;
                key += factor.GetFactorUssage();
            }

            Room room = new Room(count, mode, key);
            room.JoinToRoom(user);
            List<Room> rooms = new List<Room>();
            rooms.Add(room);
            freeRooms.Add(key, rooms);
        }

        private void ListenTCP()
        {
            taskFactoryTCP.FromAsync(tcpListener.BeginAcceptTcpClient, tcpListener.EndAcceptTcpClient, tcpListener).ContinueWith(antecedent =>
            {
                AcceptCallback(antecedent.Result);
                ListenTCP();
            });
        }

        private void ListenUDP()
        {
            taskFactoryUDP.FromAsync(udpListener.BeginReceive, ReceiveCallback, udpListener).ContinueWith(antecedent =>
            {
                ListenUDP();
            });
        }

        internal void AcceptCallback(TcpClient tcpClient)
        {
            TcpConnection tcpConnection = new TcpConnection(tcpClient, tcpClient.Client.RemoteEndPoint as IPEndPoint);
            tcpConnection.Recieve();
        }

        internal void ReceiveCallback(IAsyncResult ar)
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 8001);
            UdpClient udpClient = (ar.AsyncState as UdpClient);
            byte[] bytes = udpClient.EndReceive(ar, ref iPEndPoint);

            if (bytes != null && bytes.Length > 10)
            {
                NetData message = NetData.RecoverBytes(bytes, bytes.Length);
                User udpUser = null;

                if (clients.ContainsKey(message.senderID))
                {
                    if (clients[message.senderID].udpConnection == null)
                    {
                        OnUserConnectedUDP(clients[message.senderID]);
                        UdpConnection udpConnection = new UdpConnection(iPEndPoint);
                        udpConnection.user = clients[message.senderID];
                        clients[message.senderID].udpConnection = udpConnection;
                    }
                    user = clients[message.senderID];
                }
                else
                {
                    udpUser = new User(message.senderID);
                    udpUser.udpConnection = new UdpConnection(iPEndPoint);
                    AddConnection(udpUser);

                    defaultRoom.JoinToRoom(udpUser);

                    OnUserConnectedUDP(udpUser);

                    if (user.info == null)
                        user.info = getInfoFromBD(message.senderID);
                    if (user.info == null)
                        user.info = createUserBD(message.senderID);
                }
                SynchronizableTypes.InvokeTypeActionUDP(bytes[0], bytes, message, clients[message.senderID]);
            }
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

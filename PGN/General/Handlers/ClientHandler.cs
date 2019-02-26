using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using PGN.Data;
using PGN.Matchmaking;

namespace PGN.General
{
    public sealed class ClientHandler : Handler
    {
        internal class PhantomMessage
        {
            internal PhantomMessage(byte[] bytes, int bytesCount)
            {
                this.bytes = bytes;
                this.bytesCount = bytesCount;
            }

            internal byte[] bytes;
            internal int bytesCount;
        }

        private TcpClient tcpClient;
        private UdpClient udpClient;

        private NetworkStream stream;

        private bool connected, onConnectCondition, onDisconnectCondition, onServerNotAvaibleCondition, onConnectionLostCondition;

        public event Action onConnect;

        /// <summary>
        /// Invoke if you sucsessed disconnected from server
        /// Вызывается, если вы успешно отключились от сервера
        /// </summary>
        public event Action onDisconnect;

        /// <summary>
        /// Invoke if your Internetwork is not avaible
        /// Вызывается, если нет соеденения с сетью
        /// </summary>
        public event Action onNetworkNotAvaible;

        /// <summary>
        /// Invoke if your Server is not avaible
        /// Вызывается, если нет соеденения с сервером
        /// </summary>
        public event Action onServerNotAvaible;

        /// <summary>
        /// Invoke if connection is lost
        /// Вызывается, если потеряно соеденение с сервером
        /// </summary>
        public event Action onConnectionLost;

        private int connectTry = 0;

        private static Queue<PhantomMessage> messages = new Queue<PhantomMessage>();

        public event Action<DataBase.UserInfo> onRefreshed;
        public event Action<DataBase.UserInfo[]> onMatchReady;
        public event Action<string[]> onJoinedToRoom;
        public event Action<List<Lobby>> onGetLobbysList;
        public event Action<string> onUserLeaveRoom;
        public event Action onRoomReleased;

        public ClientHandler() : base()
        {       
            SynchronizableTypes.AddType(typeof(ValidateServerCall.Refresh), (object data, string id) => 
            {
                OnLogReceived("Refreshed");
                var refresh = data as ValidateServerCall.Refresh;
                onRefreshed?.Invoke(refresh.info);
            });

            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.OnJoinedToRoomCallback), (object data, string id) =>
            {
                var joined = data as MatchmakingServerCall.OnJoinedToRoomCallback;
                onJoinedToRoom?.Invoke(joined.ids);
            });

            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.OnMatchReadyCallback), (object data, string id) => 
            {
                var ready = data as MatchmakingServerCall.OnMatchReadyCallback;
                onMatchReady.Invoke(ready.userInfos);
            });

            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.OnGetLobbysListCallback), (object data, string id) =>
            {
                var lobbysGet = data as MatchmakingServerCall.OnGetLobbysListCallback;
                onGetLobbysList?.Invoke(lobbysGet.lobbys);
            });

            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.OnPlayerLeaveCallback), (object data, string id) =>
            {
                onUserLeaveRoom?.Invoke(id);
            });

            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.OnRoomRealeasedCallback), (object data, string id) =>
            {
                onRoomReleased?.Invoke();
            });

            SynchronizableTypes.AddSyncSubType(typeof(DataBase.UserInfo));
            SynchronizableTypes.AddSyncSubType(typeof(DataBase.DataProperty));

            SynchronizableTypes.AddSyncSubType(typeof(MatchmakingServerCall.GetLobbysList));
            SynchronizableTypes.AddSyncSubType(typeof(MatchmakingServerCall.CreateLobby));
            SynchronizableTypes.AddSyncSubType(typeof(MatchmakingServerCall.JoinToLobby));
            SynchronizableTypes.AddSyncSubType(typeof(MatchmakingServerCall.LeaveFromRoom));
            SynchronizableTypes.AddSyncSubType(typeof(MatchmakingServerCall.ReleaseRoom));
        }

        /// <summary>
        /// Make connection with  your server 
        /// Устанавливает связь с вашим сервером
        /// </summary>
        public void Connect()
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {

                try
                {
                    tcpClient = new TcpClient(new IPEndPoint(localAddressTCP.Address, localAddressTCP.Port + connectTry));
                    udpClient = new UdpClient(new IPEndPoint(localAddressUDP.Address, localAddressUDP.Port + connectTry));
                    tcpClient.BeginConnect(remoteAddressTCP.Address, remoteAddressTCP.Port, ConnectCallbackTCP, tcpClient);
                }
                catch(Exception e)
                {
                    connectTry++;
                    Connect();
                }
            }
            else
            {
                OnLogReceived("Network is not avaible!");
                onNetworkNotAvaible?.Invoke();
            }
        }

        private void ConnectCallbackTCP(IAsyncResult ar)
        {
            try
            {
                tcpClient.EndConnect(ar);
                ConnectContinue();
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10061)
                {
                    onServerNotAvaibleCondition = true;
                    OnLogReceived("Server is not avaible!");
                    return;
                }
                else
                    Connect();
            }
        }

        private void ConnectContinue()
        {
            try
            {
                udpClient.Connect(remoteAddressUDP);

                Thread.Sleep(100);

                stream = tcpClient.GetStream();

                byte[] data = new NetData(new Containers.IntContainer(0x1B39), user.ID, false).bytes;

                stream.Write(data, 0, data.Length);
                Thread.Sleep(100);
                udpClient.Send(data, data.Length);

                onConnectCondition = true;

                ReceiveMessageTCP();
                ReceiveMessageUDP();
            }
            catch(Exception e)
            {
                OnLogReceived(e.Message);
            }
        }

        /// <summary>
        /// Begin recieve message via TCP
        /// Начать слушать сообщения по протоколу TCP
        /// </summary>
        public void ReceiveMessageTCP()
        {
            System.Threading.Tasks.Task.Factory.StartNew(ReceiveMessageTCPCallback);
        }

        private void ReceiveMessageTCPCallback()
        {
            try
            {
                while (true)
                {
                    do
                    {
                        byte[] bytes = new byte[1024];
                        int bytesCount = stream.Read(bytes, 0, bytes.Length);
                        if (bytesCount > 0)
                        {
                            lock (messages)
                            {
                                messages.Enqueue(new PhantomMessage(bytes, bytesCount));
                            }
                        }
                    }
                    while (stream.DataAvailable);
                }
            }
            catch (Exception e)
            {
                if (stream != null)
                {
                    stream.Flush();
                    stream.Dispose();
                    stream = null;
                }

                if (tcpClient != null)
                {
                    tcpClient.Dispose();
                    tcpClient = null;
                }

                if (udpClient != null)
                {
                    udpClient.Dispose();
                    udpClient = null;
                }

                onConnectionLostCondition = true;
            }
        }

        /// <summary>
        /// Begin recieve message via UDP
        /// Начать слушать сообщения по протоколу UDP
        /// </summary>
        public void ReceiveMessageUDP()
        {
            System.Threading.Tasks.Task.Factory.StartNew(ReceiveMessageUDPCallback);
        }

        private void ReceiveMessageUDPCallback()
        {
            try
            {
                while (true)
                {
                    byte[] bytes = udpClient.Receive(ref remoteAddressUDP);
                    lock (messages)
                    {
                        messages.Enqueue(new PhantomMessage(bytes, bytes.Length));
                    }
                }
            }
            catch
            {
                if (stream != null)
                {
                    stream.Flush();
                    stream.Dispose();
                    stream = null;
                }

                if (tcpClient != null)
                {
                    tcpClient.Dispose();
                    tcpClient = null;
                }

                if (udpClient != null)
                {
                    udpClient.Dispose();
                    udpClient = null;
                }


                onConnectionLostCondition = true;
            }
        }

        public void SendMessageTCP(NetData message)
        {
            byte[] data = NetData.GetBytesData(message);
            stream.Write(data, 0, data.Length);
        }

        public void SendMessageUDP(NetData message)
        {
            byte[] data = NetData.GetBytesData(message);
            udpClient.Send(data, data.Length);
        }

        public void Disconnect()
        {
            if (stream != null)
            {
                stream.Flush();
                stream.Dispose();
                stream = null;
            }

            if (tcpClient != null)
            {
                tcpClient.Dispose();
                tcpClient = null;
            }

            if (udpClient != null)
            {
                udpClient.Dispose();
                udpClient = null;
            }


            onDisconnectCondition = true;
        }

        #region ServerCalls 
        public void Refresh()
        {
            SendMessageTCP(new NetData(new ValidateServerCall.Refresh(), false));
        }
        
        public void JoinToFreeRoom(params Matchmaking.RoomFactor[] roomFactors)
        {
            SendMessageTCP(new NetData(new MatchmakingServerCall.JoinToMatch(roomFactors), false));
        }

        public void LeaveFromRoom()
        {
            SendMessageTCP(new NetData(new MatchmakingServerCall.LeaveFromRoom(), false));
        }
       
        public void ReleaseRoom()
        {
            SendMessageTCP(new NetData(new MatchmakingServerCall.ReleaseRoom(), false));
        }

        public void GetLobbyList()
        {
            SendMessageTCP(new NetData(new MatchmakingServerCall.GetLobbysList(), false));
        }

        public void CreateLobby(string name, params RoomFactor[] roomFactors)
        {
            SendMessageTCP(new NetData(new MatchmakingServerCall.CreateLobby(name, roomFactors), false));
        }

        public void JoinToLobby(string lobbyID)
        {
            SendMessageTCP(new NetData(new MatchmakingServerCall.JoinToLobby(lobbyID), false));
        }

        #endregion

        /// <summary>
        /// Handle recieved messages
        /// Обработка пришедших сообщений
        /// </summary>
        /// 
        public void HandleData()
        {
            if(onConnectCondition)
            {
                onConnect?.Invoke();
                onConnectCondition = false;
                connected = true;
            }

            if(onDisconnectCondition)
            {
                onDisconnect?.Invoke();
                onDisconnectCondition = false;
                connected = false;
            }

            if(onConnectionLostCondition)
            {
                onConnectionLost?.Invoke();
                onConnectionLostCondition = false;
            }

            if(onServerNotAvaibleCondition)
            {
                onServerNotAvaible?.Invoke();
                onServerNotAvaibleCondition = false;
            }

            if (connected)
            {
                lock (messages)
                {
                    while (messages.Count != 0)
                    {
                        PhantomMessage phantomMessage = messages.Dequeue();
                        SynchronizableTypes.InvokeClientAction(phantomMessage.bytes, phantomMessage.bytesCount);
                    }
                }
            }
        }
    }
}


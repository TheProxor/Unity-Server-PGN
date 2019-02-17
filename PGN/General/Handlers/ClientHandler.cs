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

using Newtonsoft.Json;

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

        public Action<DataBase.UserInfo> onRefreshed;
        public Action onJoinedToFreeRoom;

        public ClientHandler() : base()
        {
            SynchronizableTypes.AddType(typeof(ValidateServerCall.Refresh), (object data, string id) => 
            {
                var refresh = data as ValidateServerCall.Refresh;
                onRefreshed?.Invoke(JsonConvert.DeserializeObject<DataBase.UserInfo>(refresh.refreshData));
            });

            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.CreateRoom), null);
            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.JoinToFreeRoom), (object data, string id) => { onJoinedToFreeRoom?.Invoke(); });
            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.JoinToRoom), null);
            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.LeaveFromRoom), null);
            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.GetRoomsList), null);
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
                catch 
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
            udpClient.Connect(remoteAddressUDP);

            Thread.Sleep(100);

            stream = tcpClient.GetStream();

            NetData message = new NetData(0x1B39, user.ID, false);
            byte[] data = NetData.GetBytesData(message);

            stream.Write(data, 0, data.Length);
            Thread.Sleep(100);
            udpClient.Send(data, data.Length);

            onConnectCondition = true;

            ReceiveMessageTCP();
            ReceiveMessageUDP();
        }

        /// <summary>
        /// Begin recieve message via TCP
        /// Начать слушать сообщения по протоколу TCP
        /// </summary>
        public void ReceiveMessageTCP()
        {
            byte[] bytes = new byte[1024];
            try
            {
                stream.BeginRead(bytes, 0, bytes.Length, new AsyncCallback(ReceiveMessageTCPCallback), bytes);
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
                    tcpClient.Client.Close();
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

        private void ReceiveMessageTCPCallback(IAsyncResult ar)
        {
            ReceiveMessageTCP();
            try
            {
                int bytesCount = stream.EndRead(ar);
                if (bytesCount > 0)
                {
                    byte[] bytes = ar.AsyncState as byte[];
                    lock (messages)
                    {
                        messages.Enqueue(new PhantomMessage(bytes, bytesCount));
                    }
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
            try
            {
                udpClient.BeginReceive(new AsyncCallback(ReceiveMessageUDPCallback), udpClient);
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

        private void ReceiveMessageUDPCallback(IAsyncResult ar)
        {
            ReceiveMessageUDP();
            try
            {
                byte[] bytes = udpClient.EndReceive(ar, ref remoteAddressUDP);
                lock (messages)
                {
                    messages.Enqueue(new PhantomMessage(bytes, bytes.Length));
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

       
        public void Refresh()
        {
            SendMessageTCP(new NetData(new ValidateServerCall.Refresh(), false));
        }

        public void JoinToFreeRoom(params Matchmaking.RoomFactor[] roomFactors)
        {
            SendMessageTCP(new NetData(new MatchmakingServerCall.JoinToFreeRoom(roomFactors), false));
        }

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
                        SynchronizableTypes.InvokeClientAction(phantomMessage.bytes, BitConverter.ToUInt16(phantomMessage.bytes, 0), phantomMessage.bytesCount);
                    }
                }
            }
        }
    }
}


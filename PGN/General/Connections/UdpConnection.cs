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

    }
}


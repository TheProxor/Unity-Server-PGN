using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

using PGN.General.Connections;
using PGN.Matchmaking;

namespace PGN.General
{
    public class User
    {     
        public string ID = string.Empty;

        public DataBase.UserInfo info;

        internal TcpConnection tcpConnection { get; set; }
        internal UdpConnection udpConnection { get; set; }

        public Room currentRoom;

        public User()
        {
            ID = Dns.GetHostName();
        }

        public User(string id)
        {
            this.ID = id;
        }

        public override string ToString()
        {
            return $"\nID: {ID}; {info.ToString()}"; 
        }
    }
}

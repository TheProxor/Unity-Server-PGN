using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

using PGN.Data;
using PGN.General;
using PGN.General.Connections;

using Newtonsoft.Json;

namespace PGN.Matchmaking
{
    public class Room
    {
        public string id;
        public string roomFactorKey;

        public RoomFactor.RoomMode roomMode;
        public RoomFactor.RoomCount count = new RoomFactor.RoomCount(uint.MaxValue);
        public RoomFactor.RoomMap map;

        public Dictionary<string, User> participants { get; private set; } = new Dictionary<string, User>();

        public Room(string roomFactorKey)
        {
            this.roomFactorKey = roomFactorKey;
            id = Guid.NewGuid().ToString();
            ServerHandler.rooms.Add(id, this);
        }

        public Room(RoomFactor.RoomCount count, RoomFactor.RoomMode mode, RoomFactor.RoomMap map, string roomFactorKey)
        {
            this.roomFactorKey = roomFactorKey;
            this.count = count;
            this.map = map;
            id = Guid.NewGuid().ToString();
            ServerHandler.rooms.Add(id, this);
        }

        public void JoinToRoom(User user)
        {
            lock (participants)
            {
                if (!participants.ContainsKey(user.ID))
                {
                    participants.Add(user.ID, user);
                    user.currentRoom = this;
                    if (participants.Count == count.count)
                    {
                        ServerHandler.freeRooms[roomFactorKey].Remove(this);
                       // BroadcastMessageTCP(new NetData(MatchmakingServerCall.OnRoomReadyCallback(participants)))
                    }
                }
            }
        }

        public void LeaveFromRoom(User user)
        {
            user.currentRoom = null;
            participants.Remove(user.ID);
        }

        public void BroadcastMessageTCP(byte[] data, User sender)
        {
            foreach (string key in participants.Keys)
            {
                if (participants[key].ID != sender.ID && participants[key].tcpConnection != null)
                    participants[key].tcpConnection.SendMessage(data);
            }
        }

        public void BroadcastMessageUDP(byte[] data, User sender)
        {
            foreach (string key in participants.Keys)
                if (participants[key].ID != sender.ID && participants[key].udpConnection != null)
                    participants[key].udpConnection.SendMessage(data);
        }
    }
}

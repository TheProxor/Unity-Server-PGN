using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

using PGN.Data;
using PGN.General;
using PGN.General.Connections;

namespace PGN.Matchmaking
{
    public class Room
    {
        public static Room defaultRoom;

        public string id;
        public string roomFactorKey;

        public RoomFactor.RoomMode mode;
        public RoomFactor.RoomCount count = new RoomFactor.RoomCount(uint.MaxValue);
        public RoomFactor.RoomMap map;

        public Dictionary<string, User> participants { get; private set; } = new Dictionary<string, User>();

        public bool visable { get; internal set; } = false;

        private bool isDefault;

        public void SetAsDefault()
        {
            isDefault = true;
            defaultRoom = this;
        }

        public Room(string roomFactorKey)
        {
            this.roomFactorKey = roomFactorKey;
            id = Guid.NewGuid().ToString();
            ServerHandler.rooms.Add(id, this);
        }

        public Room(RoomFactor.RoomCount count, RoomFactor.RoomMode mode, RoomFactor.RoomMap map, string roomFactorKey)
        {
            this.roomFactorKey = roomFactorKey;
            if (count != null)
                this.count = count;
            else
                this.count = new RoomFactor.RoomCount(uint.MaxValue);
            this.map = map;
            this.mode = mode;
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
                    if (participants.Count == count.count && !isDefault)
                    {
                        ServerHandler.freeRooms[roomFactorKey].Remove(this);
                        List<DataBase.UserInfo> userInfos = new List<DataBase.UserInfo>(participants.Count);
                        foreach (string id in participants.Keys)
                            userInfos.Add(participants[id].info);
                        BroadcastMessageTCP(new NetData(new MatchmakingServerCall.OnRoomReadyCallback(DataBase.UserInfo.GetUserInfoArrayBytes(userInfos.ToArray())), false).bytes);
                    }
                }
            }
        }

        public void LeaveFromRoom(User user)
        {
            lock (participants)
            {
                participants.Remove(user.ID);
                if (!isDefault)
                {
                    user.currentRoom = defaultRoom;
                    BroadcastMessageTCP(new NetData(new MatchmakingServerCall.OnPlayerLeaveCallback(), user.ID, false).bytes);
                    if (participants.Count == 1)
                        ReleaseRoom();
                    else if (participants.Count == 0)
                        ServerHandler.freeRooms[roomFactorKey].Add(this);
                }
            }
        }

        public void ReleaseRoom()
        {
            lock (participants)
            {
                if (!isDefault)
                {
                    List<RoomFactor> roomFactors = new List<RoomFactor>();

                    if (count != null)
                        roomFactors.Add(count);
                    if (mode != null)
                        roomFactors.Add(mode);
                    if (map != null)
                        roomFactors.Add(map);

                    if (roomFactors.Count > 0)
                        ServerHandler.ReleaseRoom(visable, roomFactors, new List<User>(participants.Values));

                    foreach (string key in participants.Keys)
                    {
                        participants[key].tcpConnection.SendMessage(new NetData(new ValidateServerCall.Refresh(participants[key].info.bytes), false).bytes);
                        participants[key].tcpConnection.SendMessage(new NetData(new MatchmakingServerCall.OnRoomRealeasedCallback(), false).bytes);
                        participants[key].currentRoom = defaultRoom;
                    }
                    participants.Clear();
                    ServerHandler.freeRooms[roomFactorKey].Add(this);
                }
            }
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

        public void BroadcastMessageTCP(byte[] data)
        {
            foreach (string key in participants.Keys)
            {
                if (participants[key].tcpConnection != null)
                    participants[key].tcpConnection.SendMessage(data);
            }
        }

        public void BroadcastMessageUDP(byte[] data)
        {
            foreach (string key in participants.Keys)
                if (participants[key].udpConnection != null)
                    participants[key].udpConnection.SendMessage(data);
        }
    }
}

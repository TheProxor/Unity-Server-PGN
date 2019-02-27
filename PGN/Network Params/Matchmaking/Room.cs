using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

using PGN.Data;
using PGN.General;
using PGN.General.Connections;

using ProtoBuf;

namespace PGN.Matchmaking
{
    [Synchronizable, ProtoContract]
    public class Room : ISync
    {
        [ProtoMember(1)]
        public string id;
        [ProtoMember(2)]
        public string name = string.Empty;
        [ProtoMember(3)]
        public string roomFactorKey;
        [ProtoMember(4)]
        public RoomFactor.RoomMode mode;
        [ProtoMember(5)]
        public RoomFactor.RoomCount count = new RoomFactor.RoomCount(uint.MaxValue);
        [ProtoMember(6)]
        public RoomFactor.RoomMap map;

        public Dictionary<string, User> participants { get; private set; } = new Dictionary<string, User>();

        public Room(string roomFactorKey)
        {
            this.roomFactorKey = roomFactorKey;
            id = Guid.NewGuid().ToString();
            MatchmakingController.rooms.Add(id, this);
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
            MatchmakingController.rooms.Add(id, this);
        }

        public Room(RoomFactor.RoomCount count, RoomFactor.RoomMode mode, RoomFactor.RoomMap map, string roomFactorKey, string name)
        {
            this.roomFactorKey = roomFactorKey;
            if (count != null)
                this.count = count;
            else
                this.count = new RoomFactor.RoomCount(uint.MaxValue);
            this.map = map;
            this.mode = mode;
            this.name = name;
            id = Guid.NewGuid().ToString();
            MatchmakingController.rooms.Add(id, this);
        }


        public virtual void JoinToRoom(User user)
        {
            lock (participants)
            {
                if (!participants.ContainsKey(user.ID))
                {
                    participants.Add(user.ID, user);
                    user.currentRoom = this;
                    if (participants.Count == count.count)
                        StartRoom();
                }
            }
        }

        public virtual void LeaveFromRoom(User user)
        {
            lock (participants)
            {
                participants.Remove(user.ID);
                user.currentRoom = DefaultRoom.instance;
                BroadcastMessageTCP(new NetData(new MatchmakingServerCall.OnPlayerLeaveCallback(), user.ID, false).bytes);
                if (participants.Count == 1)
                    ReleaseRoom();
                else if (participants.Count == 0)
                    MatchmakingController.matchmakingRooms[roomFactorKey].Add(this);
            }
        }

        public virtual void StartRoom()
        {
            MatchmakingController.matchmakingRooms[roomFactorKey].Remove(this);
            List<DataBase.UserInfo> userInfos = new List<DataBase.UserInfo>(participants.Count);
            foreach (string id in participants.Keys)
                userInfos.Add(participants[id].info);
            BroadcastMessageTCP(new NetData(new MatchmakingServerCall.OnMatchReadyCallback(userInfos.ToArray()), false).bytes);
        }

        public virtual void ReleaseRoom()
        {
            lock (participants)
            {
                foreach (string key in participants.Keys)
                {
                    participants[key].tcpConnection.SendMessage(new NetData(new MatchmakingServerCall.OnRoomRealeasedCallback(), false).bytes);
                    participants[key].currentRoom = DefaultRoom.instance;
                }
                participants.Clear();
                MatchmakingController.matchmakingRooms[roomFactorKey].Add(this);
            }
        }

        public virtual void DestroyRoom()
        {
            lock (participants)
            {
                foreach (string key in participants.Keys)
                {
                    participants[key].tcpConnection.SendMessage(new NetData(new MatchmakingServerCall.OnRoomRealeasedCallback(), false).bytes);
                    participants[key].currentRoom = DefaultRoom.instance;
                }
                participants.Clear();
                MatchmakingController.matchmakingRooms[roomFactorKey].Add(this);
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

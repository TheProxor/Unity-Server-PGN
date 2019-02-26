using System;
using System.Collections.Generic;
using System.Text;

using PGN;
using PGN.General;

using ProtoBuf;

namespace PGN.Matchmaking
{
    [ProtoContract, Synchronizable]
    public class DefaultRoom : Room
    {
        internal static DefaultRoom instance;

        public DefaultRoom(string roomFactorKey) : base(roomFactorKey)
        {
            instance = this;
        }

        public DefaultRoom(RoomFactor.RoomCount count, RoomFactor.RoomMode mode, RoomFactor.RoomMap map, string roomFactorKey) : base(count, mode, map, roomFactorKey)
        {
            instance = this;
        }

        public DefaultRoom(RoomFactor.RoomCount count, RoomFactor.RoomMode mode, RoomFactor.RoomMap map, string roomFactorKey, string name) : base(count, mode, map, roomFactorKey, name)
        {
            instance = this;
        }

        public override void JoinToRoom(User user)
        {
            lock (participants)
            {
                if (!participants.ContainsKey(user.ID))
                {
                    participants.Add(user.ID, user);
                    user.currentRoom = this;
                }
            }
        }

        public override void LeaveFromRoom(User user)
        {
            lock (participants)
            {
                participants.Remove(user.ID);
            }
        }

        public override void ReleaseRoom()
        {
            return;
        }

    }
}

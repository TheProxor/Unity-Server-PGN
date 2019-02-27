using System;
using System.Collections.Generic;
using System.Text;

using PGN;
using PGN.General;

using ProtoBuf;

namespace PGN.Matchmaking
{
    [ProtoContract, Synchronizable]
    public class Lobby : Room
    {
        public Lobby(string roomFactorKey) : base(roomFactorKey)
        {

        }

        public Lobby(RoomFactor.RoomCount count, RoomFactor.RoomMode mode, RoomFactor.RoomMap map, string roomFactorKey) : base(count, mode, map, roomFactorKey)
        {
        }

        public Lobby(RoomFactor.RoomCount count, RoomFactor.RoomMode mode, RoomFactor.RoomMap map, string roomFactorKey, string name) : base(count, mode, map, roomFactorKey, name)
        {

        }

        public override void JoinToRoom(User user)
        {
            base.JoinToRoom(user);
        }

        public override void DestroyRoom()
        {
            base.DestroyRoom();
        }

        public override void StartRoom()
        {
            base.StartRoom();
        }

        public override void LeaveFromRoom(User user)
        {
            base.LeaveFromRoom(user);
        }

        public override void ReleaseRoom()
        {
            base.ReleaseRoom();
        }

    }
}

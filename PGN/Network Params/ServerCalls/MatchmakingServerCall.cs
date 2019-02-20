using System;
using System.Collections.Generic;
using System.Text;

using PGN.Matchmaking;

using ProtoBuf;

namespace PGN
{
    [Synchronizable, ProtoContract]
    public class MatchmakingServerCall : IServerCall
    {
        [Synchronizable, ProtoContract]
        public class JoinToFreeRoom : MatchmakingServerCall
        {
            [ProtoMember(1)]
            public RoomFactor[] roomFactors;

            public JoinToFreeRoom(params RoomFactor[] roomFactors)
            {
                this.roomFactors = roomFactors;
            }

            public JoinToFreeRoom()
            {

            }
        }

        [Synchronizable, ProtoContract]
        public class OnRoomReadyCallback : MatchmakingServerCall
        {
            [ProtoMember(1)]
            public string opponentData;

            public OnRoomReadyCallback(string opponentData)
            {
                this.opponentData = opponentData;
            }

            public OnRoomReadyCallback()
            {

            }
        }

        [Synchronizable, ProtoContract]
        public class OnPlayerLeaveCallback : MatchmakingServerCall
        {
            public OnPlayerLeaveCallback()
            {

            }
        }

        [Synchronizable, ProtoContract]
        public class ReleaseRoom : MatchmakingServerCall
        {
            public ReleaseRoom()
            {

            }
        }

        [Synchronizable, ProtoContract]
        public class OnRoomRealeasedCallback : MatchmakingServerCall
        {
            public OnRoomRealeasedCallback()
            {

            }
        }

        [Synchronizable, ProtoContract]
        internal class CreateRoom : MatchmakingServerCall
        {
            [ProtoMember(1)]
            public RoomFactor[] roomFactors;

            public CreateRoom(params RoomFactor[] roomFactors)
            {
                this.roomFactors = roomFactors;
            }

            public CreateRoom()
            {

            }
        }

        [Synchronizable, ProtoContract]
        internal class LeaveFromRoom : MatchmakingServerCall
        {
            public LeaveFromRoom()
            {

            }
        }

        [Synchronizable, ProtoContract]
        internal class JoinToRoom : MatchmakingServerCall
        {
            [ProtoMember(1)]
            public string factorKey;
            [ProtoMember(2)]
            public string id;

            public JoinToRoom(string factorKey, string id)
            {
                this.factorKey = factorKey;
                this.id = id;
            }

            public JoinToRoom()
            {

            }
        }

        [Synchronizable, ProtoContract]
        internal class GetRoomsList : MatchmakingServerCall
        {
            [ProtoMember(1)]
            public RoomFactor[] roomFactors;

            public GetRoomsList(params RoomFactor[] roomFactors)
            {
                this.roomFactors = roomFactors;
            }

            public GetRoomsList()
            {

            }
        }

        [Synchronizable, ProtoContract]
        internal class FreeRoomsListCallback : MatchmakingServerCall
        {
            [ProtoMember(1)]
            public string roomsData;

            public FreeRoomsListCallback(string roomsData)
            {
                this.roomsData = roomsData;
            }

            public FreeRoomsListCallback()
            {

            }
        }

    }
}

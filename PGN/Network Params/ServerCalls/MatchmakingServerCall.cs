using System;
using System.Collections.Generic;
using System.Text;

using PGN.Matchmaking;

using ProtoBuf;

namespace PGN
{
    [Synchronizable, ProtoContract]
    internal class MatchmakingServerCall : IServerCall
    {
        [Synchronizable, ProtoContract]
        internal class JoinToFreeRoom : MatchmakingServerCall
        {
            [ProtoMember(1)]
            public IRoomFactor[] roomFactors;

            public JoinToFreeRoom(params IRoomFactor[] roomFactors)
            {
                this.roomFactors = roomFactors;
            }

            public JoinToFreeRoom()
            {

            }
        }

        [Synchronizable, ProtoContract]
        internal class CreateRoom : MatchmakingServerCall
        {
            [ProtoMember(1)]
            public IRoomFactor[] roomFactors;

            public CreateRoom(params IRoomFactor[] roomFactors)
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
            public GetRoomsList()
            {

            }
        }

    }
}

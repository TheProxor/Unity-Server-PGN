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
        public class JoinToMatch : MatchmakingServerCall
        {
            [ProtoMember(1)]
            public RoomFactor[] roomFactors;

            public JoinToMatch(params RoomFactor[] roomFactors)
            {
                this.roomFactors = roomFactors;
            }

            public JoinToMatch()
            {

            }
        }

        [Synchronizable, ProtoContract]
        public class JoinToLobby : MatchmakingServerCall
        {
            [ProtoMember(1)]
            public string id;

            public JoinToLobby(string id)
            {
                this.id = id;
            }

            public JoinToLobby()
            {

            }
        }

        [Synchronizable, ProtoContract]
        public class OnJoinedToRoomCallback : MatchmakingServerCall
        {
            [ProtoMember(1)]
            public string[] ids;

            public OnJoinedToRoomCallback(string[] ids)
            {
                this.ids = ids;
            }

            public OnJoinedToRoomCallback()
            {

            }
        }


        [Synchronizable, ProtoContract]
        public class StartLobby : MatchmakingServerCall
        {
            public StartLobby()
            {

            }
        }

        [Synchronizable, ProtoContract]
        public class DestroyLobby : MatchmakingServerCall
        {
            public DestroyLobby()
            {

            }
        }

        [Synchronizable, ProtoContract]
        public class OnLobbyDestoyedCallback : MatchmakingServerCall
        {
            public OnLobbyDestoyedCallback()
            {

            }
        }

        [Synchronizable, ProtoContract]
        public class OnMatchReadyCallback : MatchmakingServerCall
        {
            [ProtoMember(1)]
            public DataBase.UserInfo[] userInfos;

            public OnMatchReadyCallback(DataBase.UserInfo[] userInfos)
            {
                this.userInfos = userInfos;
            }

            public OnMatchReadyCallback()
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
        internal class CreateLobby : MatchmakingServerCall
        {
            [ProtoMember(1)]
            public RoomFactor[] roomFactors;
            [ProtoMember(2)]
            public string name;

            public CreateLobby(string name, params RoomFactor[] roomFactors)
            {
                this.roomFactors = roomFactors;
                this.name = name;
            }

            public CreateLobby()
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
        internal class GetLobbysList : MatchmakingServerCall
        {
            public GetLobbysList()
            {

            }
        }

        [Synchronizable, ProtoContract]
        internal class OnGetLobbysListCallback : MatchmakingServerCall
        {
            [ProtoMember(1)]
            public List<Lobby> lobbys;

            public OnGetLobbysListCallback(List<Lobby> lobbys)
            {
                this.lobbys = lobbys;
            }

            public OnGetLobbysListCallback()
            {

            }
        }

    }
}

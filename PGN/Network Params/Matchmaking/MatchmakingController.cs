using System;
using System.Collections.Generic;
using System.Text;

using PGN;
using PGN.Data;
using PGN.General;

namespace PGN.Matchmaking
{
    internal static class MatchmakingController
    {
        internal static Dictionary<string, List<Room>> matchmakingRooms { get; private set; } = new Dictionary<string, List<Room>>();
        internal static Dictionary<string, List<Lobby>> lobbys { get; private set; } = new Dictionary<string, List<Lobby>>();
        internal static Room defaultRoom;

        internal static Dictionary<string, Room> rooms { get; private set; } = new Dictionary<string, Room>();


        public static void Init()
        {
            defaultRoom = new DefaultRoom("defaultRoomType");
            matchmakingRooms.Add("defaultRoomType", new List<Room>(1));
            matchmakingRooms["defaultRoomType"].Add(defaultRoom);

            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.CreateLobby),
                (object data, string id) =>
                {
                    MatchmakingServerCall.CreateLobby createRoom = data as MatchmakingServerCall.CreateLobby;
                    CreateLobby(ServerHandler.clients[id], createRoom.name, createRoom.roomFactors);
                }
                );

            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.GetLobbysList),
               (object data, string id) =>
               {
                   ServerHandler.clients[id].tcpConnection.SendMessage(new NetData(new MatchmakingServerCall.OnGetLobbysListCallback(GetLobbyList()), false));
               }
               );

            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.JoinToMatch),
                  (object data, string id) =>
                  {
                      MatchmakingServerCall.JoinToMatch joinToMatch = data as MatchmakingServerCall.JoinToMatch;
                      JoinToMatch(ServerHandler.clients[id], joinToMatch.roomFactors);
                  }
                );

            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.JoinToLobby),
               (object data, string id) =>
               {
                   MatchmakingServerCall.JoinToLobby joinToLobby = data as MatchmakingServerCall.JoinToLobby;
                   if (rooms.ContainsKey(joinToLobby.id))
                       rooms[joinToLobby.id].JoinToRoom(ServerHandler.clients[id]);
               }
             );

            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.LeaveFromRoom),
                  (object data, string id) =>
                  {
                      ServerHandler.clients[id].currentRoom.LeaveFromRoom(ServerHandler.clients[id]);
                  }
                );

            SynchronizableTypes.AddType(typeof(MatchmakingServerCall.ReleaseRoom),
                (object data, string id) =>
                {
                    ServerHandler.clients[id].currentRoom.ReleaseRoom();
                }
              );

            SynchronizableTypes.AddSyncSubType(typeof(MatchmakingServerCall.OnMatchReadyCallback));
            SynchronizableTypes.AddSyncSubType(typeof(MatchmakingServerCall.OnPlayerLeaveCallback));
            SynchronizableTypes.AddSyncSubType(typeof(MatchmakingServerCall.OnJoinedToRoomCallback));
            SynchronizableTypes.AddSyncSubType(typeof(MatchmakingServerCall.OnGetLobbysListCallback));
            SynchronizableTypes.AddSyncSubType(typeof(MatchmakingServerCall.OnRoomRealeasedCallback));
        }

        private static void JoinToMatch(User user, params RoomFactor[] roomFactors)
        {
            user.currentRoom.LeaveFromRoom(user);

            string key;
            RoomFactor.RoomCount count;
            RoomFactor.RoomMode mode;
            RoomFactor.RoomMap map;

            GetFactorKey(out key, out count, out mode, out map, roomFactors);

            if (matchmakingRooms.ContainsKey(key))
            {
                if (matchmakingRooms[key].Count > 0)
                    matchmakingRooms[key][0].JoinToRoom(user);
                else
                    CreateRoom(user, key, count, mode, map, matchmakingRooms[key]);
            }
            else
                CreateRoom(user, key, count, mode, map);
        }

        private static void CreateRoom(User user, string roomFactorsKey, RoomFactor.RoomCount count, RoomFactor.RoomMode mode, RoomFactor.RoomMap map, List<Room> rooms)
        {
            Room room = new Room(count, mode, map, roomFactorsKey);
            room.JoinToRoom(user);
            rooms.Add(room);
        }

        private static void CreateRoom(User user, string roomFactorsKey, RoomFactor.RoomCount count, RoomFactor.RoomMode mode, RoomFactor.RoomMap map)
        {
            Room room = new Room(count, mode, map, roomFactorsKey);
            room.JoinToRoom(user);
            List<Room> rooms = new List<Room>();
            rooms.Add(room);
            matchmakingRooms.Add(roomFactorsKey, rooms);
        }

        private static void CreateLobby(User user, string name, params RoomFactor[] roomFactors)
        {
            string key;
            RoomFactor.RoomCount count;
            RoomFactor.RoomMode mode;
            RoomFactor.RoomMap map;

            GetFactorKey(out key, out count, out mode, out map, roomFactors);

            Lobby lobby = new Lobby(count, mode, map, key, name);
            lobby.JoinToRoom(user);
            if (lobbys[key] == null || lobbys[key].Count == 0)
            {
                List<Lobby> lobbysList = new List<Lobby>();
                lobbysList.Add(lobby);
                lobbys.Add(key, lobbysList);
            }
            else
                lobbys[key].Add(lobby);
        }

        private static List<Lobby> GetLobbyList()
        {
            List<Lobby> lobbysList = new List<Lobby>();
            foreach (string key in lobbys.Keys)
                lobbysList.AddRange(lobbys[key]);
            return lobbysList;
        }

        private static void GetFactorKey(out string key, out RoomFactor.RoomCount count, out RoomFactor.RoomMode mode, out RoomFactor.RoomMap map, RoomFactor[] roomFactors)
        {
            key = "none";
            count = null;
            mode = null;
            map = null;
            foreach (RoomFactor factor in roomFactors)
            {
                if (factor is RoomFactor.RoomCount)
                    count = factor as RoomFactor.RoomCount;
                else if (factor is RoomFactor.RoomMode)
                    mode = factor as RoomFactor.RoomMode;
                else if (factor is RoomFactor.RoomMap)
                    map = factor as RoomFactor.RoomMap;
                key += factor.GetFactorUssage();
            }
        }
    }
}

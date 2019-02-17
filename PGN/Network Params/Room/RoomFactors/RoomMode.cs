using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace PGN.Matchmaking
{
    [Synchronizable, ProtoContract]
    public class RoomMode : IRoomFactor
    {
        [ProtoMember(1)]
        public string name;

        public RoomMode(string name)
        {
            this.name = name;
        }

        public RoomMode()
        {

        }

        public string GetFactorUssage()
        {
            return name;
        }
    }
}

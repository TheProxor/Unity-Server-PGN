using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

namespace PGN.Matchmaking
{
    [Synchronizable, ProtoContract]
    public class RoomCount : IRoomFactor
    {
        [ProtoMember(1)]
        public uint count;

        public RoomCount(uint count)
        {
            this.count = count;
        }

        public RoomCount()
        {

        }

        public string GetFactorUssage()
        {
            return count.ToString();
        }
    }
}

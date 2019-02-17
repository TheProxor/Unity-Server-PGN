using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

namespace PGN.Matchmaking
{
    [ProtoContract, Synchronizable, ProtoInclude(50000, typeof(RoomMode)), ProtoInclude(50001, typeof(RoomCount)), ProtoInclude(50002, typeof(RoomMap))]
    public class RoomFactor : ISync
    {
        public virtual string GetFactorUssage()
        {
            return string.Empty;
        }

        [ProtoContract]
        public class RoomMode : RoomFactor
        {
            [ProtoMember(1)]
            public string mode;

            public RoomMode(string mode)
            {
                this.mode = mode;
            }

            public RoomMode()
            {

            }

            public override string GetFactorUssage()
            {
                return mode;
            }
        }


        [ProtoContract]
        public class RoomCount : RoomFactor
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

            public override string GetFactorUssage()
            {
                return count.ToString();
            }
        }

       

        [ProtoContract]
        public class RoomMap : RoomFactor
        {
            [ProtoMember(1)]
            public string name;

            public RoomMap(string name)
            {
                this.name = name;
            }

            public RoomMap()
            {

            }

            public override string GetFactorUssage()
            {
                return name;
            }
        } 
    }
}

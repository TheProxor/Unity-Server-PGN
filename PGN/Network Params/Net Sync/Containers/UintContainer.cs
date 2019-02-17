using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

namespace PGN.Containers
{
    [Synchronizable, ProtoContract]
    internal class UintContainer : ISync
    {
        public UintContainer()
        {

        }

        public UintContainer(uint value)
        {
            this.value = value;
        }

        [ProtoMember(1)]
        public uint value;
    }
}
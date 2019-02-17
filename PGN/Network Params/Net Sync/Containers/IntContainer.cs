using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

namespace PGN.Containers
{
    [Synchronizable, ProtoContract]
    internal class IntContainer : ISync
    {
        public IntContainer()
        {

        }

        public IntContainer(int value)
        {
            this.value = value;
        }

        [ProtoMember(1)]
        public int value;
    }
}
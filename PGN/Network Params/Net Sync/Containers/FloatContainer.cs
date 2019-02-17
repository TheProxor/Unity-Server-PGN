using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

namespace PGN.Containers
{
    [Synchronizable, ProtoContract]
    internal class FloatContainer : ISync
    {
        public FloatContainer()
        {

        }

        public FloatContainer(float value)
        {
            this.value = value;
        }

        [ProtoMember(1)]
        public float value;
    }
}
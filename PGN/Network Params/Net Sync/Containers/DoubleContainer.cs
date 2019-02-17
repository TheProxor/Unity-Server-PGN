using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

namespace PGN.Containers
{
    [Synchronizable, ProtoContract]
    internal class DoubleContainer : ISync
    {
        public DoubleContainer()
        {

        }

        public DoubleContainer(double value)
        {
            this.value = value;
        }

        [ProtoMember(1)]
        public double value;
    }
}
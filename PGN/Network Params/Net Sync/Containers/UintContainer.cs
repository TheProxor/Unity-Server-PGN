using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

namespace PGN.Containers
{
    [Synchronizable, ProtoContract]
    internal class UintContainer : Container
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

        public override object GetValue()
        {
            return value;
        }

        public override void SetValue(object value)
        {
            this.value = Convert.ToUInt32(value);
        }
    }
}
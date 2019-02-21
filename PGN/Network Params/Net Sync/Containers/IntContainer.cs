using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

namespace PGN.Containers
{
    [Synchronizable, ProtoContract]
    internal class IntContainer : Container
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

        public override object GetValue()
        {
            return value;
        }

        public override void SetValue(object value)
        {
            this.value = Convert.ToInt32(value);
        }
    }
}
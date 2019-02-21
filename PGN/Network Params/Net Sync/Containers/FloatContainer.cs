using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

namespace PGN.Containers
{
    [Synchronizable, ProtoContract]
    internal class FloatContainer : Container
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

        public override object GetValue()
        {
            return value;
        }

        public override void SetValue(object value)
        {
            this.value = Convert.ToSingle(value);
        }
    }
}
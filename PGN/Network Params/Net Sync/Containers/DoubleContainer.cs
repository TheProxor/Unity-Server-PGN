using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

namespace PGN.Containers
{
    [Synchronizable, ProtoContract]
    internal class DoubleContainer : Container
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

        public override object GetValue()
        {
            return value;
        }

        public override void SetValue(object value)
        {
            this.value = Convert.ToDouble(value);
        }
    }
}
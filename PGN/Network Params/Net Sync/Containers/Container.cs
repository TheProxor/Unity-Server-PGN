using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

namespace PGN.Containers
{
    [Synchronizable, ProtoContract]
    internal class Container : ISync
    {
        public virtual object GetValue()
        {
            return null;
        }

        public virtual void SetValue(object value)
        {
            return;
        }
    }
}

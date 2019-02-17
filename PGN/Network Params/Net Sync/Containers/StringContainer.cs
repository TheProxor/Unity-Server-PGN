using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

namespace PGN.Containers
{
    [Synchronizable, ProtoContract]
    internal class StringContainer : ISync
    {
        public StringContainer()
        {

        }
            
        public StringContainer(string value)
        {
            this.value = value;
        }

        [ProtoMember(1)]
        public string value;
    }
}

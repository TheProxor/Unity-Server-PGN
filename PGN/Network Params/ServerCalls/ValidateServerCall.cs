using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

namespace PGN
{
    [Synchronizable, ProtoContract]
    public class ValidateServerCall : ISync
    {
        [Synchronizable, ProtoContract]
        public class Refresh : ValidateServerCall
        {
            public Refresh()
            {
                this.refreshData = string.Empty;
            }

            public Refresh(string refreshData)
            {
                this.refreshData = refreshData;
            }

            [ProtoMember(1)]
            public string refreshData;
        }

    }
}

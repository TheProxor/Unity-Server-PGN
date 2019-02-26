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
            [ProtoMember(1)]
            public DataBase.UserInfo info = null;

            public Refresh()
            {
                info = null;
            }

            public Refresh(DataBase.UserInfo info)
            {
                this.info = info;
            }
        }
    }
}

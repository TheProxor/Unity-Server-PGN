using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;
using PGN;

namespace PGN
{
    [Synchronizable, ProtoContract]
    public interface IServerCall : ISync
    {

    }
}

using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

namespace PGN.Matchmaking
{
    [Synchronizable, ProtoContract]
    public interface IRoomFactor : ISync
    {
        string GetFactorUssage();

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using ProtoBuf;

namespace PGN.DataBase
{
    [Synchronizable, ProtoContract]
    public class UserInfo : ISync
    {
        [ProtoMember(1)]
        public string id { get; set; }
        [ProtoMember(2)]
        public Dictionary<string, DataProperty> dataAttributes;

        public UserInfo(int capacity)
        {
            this.id = string.Empty;
            dataAttributes = new Dictionary<string, DataProperty>(capacity);
        }

        public UserInfo()
        {
            dataAttributes = new Dictionary<string, DataProperty>();
        }
    }
}

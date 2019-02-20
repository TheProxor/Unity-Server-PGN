using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGN.DataBase
{
    [Serializable]
    public class UserInfo
    {
        public static uint attributeCount = 0;

        public string id { get; set; }

        public Dictionary<string, DataProperty> dataAttributes;

        public UserInfo(int capacity)
        {
            this.id = string.Empty;
            dataAttributes = new Dictionary<string, DataProperty>(capacity);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    [Serializable]
    public class UserInfo
    {
        public static uint attributeCount = 0;

        public string id { get; set; }

        public Dictionary<string, GameCore.Game.DataAttribute> dataAttributes;

        public UserInfo(int capacity)
        {
            this.id = string.Empty;
            dataAttributes = new Dictionary<string, Game.DataAttribute>(capacity);
        }

        public string roomID = string.Empty;
    }
}

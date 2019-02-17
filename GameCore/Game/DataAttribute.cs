using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore.Game
{
    [Serializable]
    public class DataAttribute : IData
    {
        public string id;
        public object value;

        public DataAttribute(string id, object value)
        {
            this.id = id;
            this.value = value;
        }
    }
}

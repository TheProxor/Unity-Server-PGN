using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGN.DataBase
{
    [Serializable]
    public class DataProperty : IData
    {
        public string id;
        public object value;

        public DataProperty()
        {

        }

        public DataProperty(string id, object value)
        {
            this.id = id;
            this.value = value;
        }
    }
}

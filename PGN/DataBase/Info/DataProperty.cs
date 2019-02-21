using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PGN.Containers;

using ProtoBuf;

namespace PGN.DataBase
{
    [ProtoContract, Synchronizable]
    public class DataProperty : ISync
    {
        [ProtoMember(1)]
        public string id;
        [ProtoMember(2)]
        internal Container value;

        public DataProperty()
        {
            
        }

        public DataProperty(string id, string value)
        {
            this.id = id;
            this.value = new StringContainer(value);
        }

        public DataProperty(string id, int value)
        {
            this.id = id;
            this.value = new IntContainer(value);
        }

        public DataProperty(string id, float value)
        {
            this.id = id;
            this.value = new FloatContainer(value);
        }

        public DataProperty(string id, double value)
        {
            this.id = id;
            this.value = new DoubleContainer(value);
        }

        public DataProperty(string id, object value)
        {
            this.id = id;
            if(value is int)
                this.value = new DoubleContainer(Convert.ToInt32(value));
            else if(value is float)
                this.value = new FloatContainer(Convert.ToSingle(value));
            else if(value is string)
                this.value = new StringContainer(value.ToString());
            else if(value is double)
                this.value = new DoubleContainer(Convert.ToDouble(value));
        }

        public void SetValue(object value)
        {
            this.value.SetValue(value);
        }

        public object GetValue()
        {
            return value.GetValue();
        }
    }
}

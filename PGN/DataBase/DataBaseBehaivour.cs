using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using PGN;
using PGN.General;

namespace PGN.DataBase
{
    public abstract class DataBaseBehaivour
    {
        protected const string dbName = "Users";
        protected List<DataProperty> datas;

        internal virtual void Init(string path, string attributesPath)
        {
            if (File.Exists(attributesPath))
            {
                string[] lines = File.ReadAllLines(attributesPath);
                foreach (string line in lines)
                {
                    string[] components = line.Split(' ');
                    CreateDataBaseColumn(components[1], components[0], components[2]);
                }
            }
        }

        protected void SetDatasValue(string type, string name, string defaultValue)
        {
            switch (type)
            {
                case "TEXT": datas.Add(new DataProperty(name, defaultValue)); break;
                case "INT": datas.Add(new DataProperty(name, int.Parse(defaultValue))); break;
                case "FLOAT": datas.Add(new DataProperty(name, float.Parse(defaultValue))); break;
                case "DOUBLE": datas.Add(new DataProperty(name, double.Parse(defaultValue))); break;
            }
        }

        protected abstract void CreateDataBaseColumn(string type, string name, string defaultValue);

        internal abstract UserInfo GetUserData(string id);
        internal abstract void SetUserData(string id, string param, object value);
        internal abstract void SaveUserData(User user);
        internal abstract UserInfo CreateUser(string userID);
    }
}

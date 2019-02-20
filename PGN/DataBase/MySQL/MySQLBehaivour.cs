using System;
using System.Collections.Generic;
using System.Text;
using PGN.General;

namespace PGN.DataBase
{
    public class MySQLBehaivour : DataBaseBehaivour
    {
        private const string connectionCommand = "server = sql7.freemysqlhosting.net; User Id = sql7275273; password = i3CMg8Y7C2; database = sql7275273; Pooling = false";

        internal override void Init(string path, string attributesPath)
        {
            base.Init(path, attributesPath);
        }

        protected override void CreateDataBaseColumn(string type, string name, string defalutValue)
        {
            throw new NotImplementedException();
        }

        internal override UserInfo CreateUser(string userID)
        {
            throw new NotImplementedException();
        }

        internal override UserInfo GetUserData(string id)
        {
            throw new NotImplementedException();
        }

        internal override void SaveUserData(User user)
        {
            throw new NotImplementedException();
        }

        internal override void SetUserData(string id, string param, object value)
        {
            throw new NotImplementedException();
        }
    }
}

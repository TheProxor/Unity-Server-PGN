using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SQLite;

using PGN;
using PGN.General;

namespace PGN.DataBase
{
    public class SQLiteBehaivour : DataBaseBehaivour
    {
        private SQLiteConnection connection;
        private SQLiteCommand command;

      
        internal override void Init(string path, string attributesPath)
        {
            command = new SQLiteCommand();
            datas = new List<DataProperty>();

            if (!File.Exists(path))
            {
                SQLiteConnection.CreateFile(path);

                connection = new SQLiteConnection("Data Source=" + path + ";Version=3;");
                connection.Open();
                command.Connection = connection;
                command.CommandText = "CREATE TABLE IF NOT EXISTS Users (id TEXT PRIMARY KEY)";
                command.ExecuteNonQuery();
            }
            else
            {
                connection = new SQLiteConnection("Data Source=" + path + ";Version=3;");
                connection.Open();
                command.Connection = connection;
            }

            base.Init(path, attributesPath);
        }

        protected override void CreateDataBaseColumn(string type, string name, string defaultValue)
        {
            try
            {
                command = new SQLiteCommand($"ALTER TABLE {dbName} ADD COLUMN {name} {type};", connection);
                command.ExecuteNonQuery();
            }
            catch { }
            finally
            {
                SetDatasValue(type, name, defaultValue);
            }
        }

        internal override UserInfo GetUserData(string id)
        {
            try
            {
                command = new SQLiteCommand($"SELECT * FROM '{dbName}' WHERE id = '{id}';", connection);
                var reader = command.ExecuteReader();
                if (reader.StepCount > 0)
                {
                    UserInfo info = new UserInfo(datas.Count);
                    while (reader.Read())
                    {
                        info.id = reader.GetString(0);
                        for (int i = 0; i < datas.Count; i++)
                        {
                            string name = reader.GetName(i + 1);
                            object value = reader.GetValue(i + 1);
                            if (datas[i].GetValue() is string)
                                info.dataAttributes.Add(name, new DataProperty(name, value.ToString()));
                            else if (datas[i].GetValue() is int)
                                info.dataAttributes.Add(name, new DataProperty(name, Convert.ToInt32(value)));
                            else if (datas[i].GetValue() is float)
                                info.dataAttributes.Add(name, new DataProperty(name, Convert.ToSingle(value)));
                            else if (datas[i].GetValue() is double)
                                info.dataAttributes.Add(name, new DataProperty(name, Convert.ToDouble(value)));
                        }
                    }
                    reader.Close();
                    return info;
                }
                else
                {
                    reader.Close();
                    return null;
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("SQL ERROR:" + ex.Message);
            }
            return null;
        }

     
        internal override void SetUserData(string id, string param, object value)
        {
            command = new SQLiteCommand($"UPDATE '{dbName}' SET '{param}' = '{value}' WHERE id = '{id}';", connection);
            command.ExecuteNonQuery();
        }

        internal override void SaveUserData(User user)
        {
            string fields = string.Empty;
            for (int i = 0; i < datas.Count; i++)
                fields += $",'{datas[i].id}' = '{user.info.dataAttributes[datas[i].id].GetValue()}'";

            command = new SQLiteCommand($"UPDATE '{dbName}' SET '{datas[0].id}' = '{user.info.dataAttributes[datas[0].id].GetValue()}'{fields} WHERE id = '{user.ID}';", connection);
            command.ExecuteNonQuery();
        }

        internal override UserInfo CreateUser(string userID)
        {
            UserInfo info = new UserInfo(datas.Count);
            info.id = userID;

            string fields = string.Empty;
            string values = string.Empty;
            for (int i = 0; i < datas.Count; i++)
            {
                fields += $",'{datas[i].id}'";
                values += $",'{datas[i].GetValue()}'";
                info.dataAttributes.Add(datas[i].id, new DataProperty(datas[i].id, datas[i].value.GetValue()));
            }

            command = new SQLiteCommand($"INSERT INTO {dbName} ('id'{fields}) VALUES ('{userID}'{values});", connection);
            command.ExecuteNonQuery();

            return info;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Data;
using System.Data.SQLite;

using PGN;
using PGN.General;

namespace PGN.DataBase
{
    internal static class MySqlHandler
    {
        private const string connectionCommand = "server = sql7.freemysqlhosting.net; User Id = sql7275273; password = i3CMg8Y7C2; database = sql7275273; Pooling = false";
        private const string dbName = "Users";

        private static SQLiteConnection connection;
        private static SQLiteCommand command;

        private static List<DataProperty> datas;

        public static void Init(string path, string attributesPath)
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


            if (File.Exists(attributesPath))
            {
                string[] lines = File.ReadAllLines(attributesPath);
                foreach (string line in lines)
                {
                    string[] components = line.Split(' ');
                    try
                    {
                        command = new  SQLiteCommand( $"ALTER TABLE {dbName} ADD COLUMN {components[0]} {components[1]};", connection);
                        command.ExecuteNonQuery();
                    }
                    catch
                    {

                    }
                    finally
                    {
                        switch (components[1])
                        {
                            case "TEXT": datas.Add(new DataProperty(components[0], components[2])); break;
                            case "INT": datas.Add(new DataProperty(components[0], int.Parse(components[2]))); break;
                            case "FLOAT": datas.Add(new DataProperty(components[0], float.Parse(components[2]))); break;
                            case "DOUBLE": datas.Add(new DataProperty(components[0], double.Parse(components[2]))); break;
                        }
                    }
                }
            }
        }

        public static UserInfo GetUserData(string id)
        {
            try
            {
                command = new SQLiteCommand( $"SELECT * FROM '{dbName}' WHERE id = '{id}';", connection);
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
                            if (datas[i].value is string)
                                info.dataAttributes.Add(name, new DataProperty(name, value.ToString()));
                            else if(datas[i].value is int)
                                info.dataAttributes.Add(name, new DataProperty(name, Convert.ToInt32(value)));
                            else if (datas[i].value is float)
                                info.dataAttributes.Add(name, new DataProperty(name, Convert.ToSingle(value)));
                            else if (datas[i].value is double)
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

        public static List<UserInfo> FindUserByName(string name)
        {
            try
            {
                command.CommandText = $"SELECT * FROM '{dbName}' WHERE name LIKE '{name}';";
                var reader = command.ExecuteReader();

                List<UserInfo> users = new List<UserInfo>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    while (reader.Read())
                    {
                        UserInfo info = new UserInfo(datas.Count);
                        info.id = reader.GetString(0);
                        for (int k = 1; k < datas.Count; k++)
                            info.dataAttributes.Add(reader.GetName(k), new DataProperty(reader.GetName(k), reader.GetValue(k)));
                        users.Add(info);
                    }
                }
                return users;
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("SQL ERROR:" + ex.Message);
                return null;
            }
        }

        public static void SetUserData(string id, string param, object value)
        {
            command = new SQLiteCommand( $"UPDATE '{dbName}' SET '{param}' = '{value}' WHERE id = '{id}';", connection);
            command.ExecuteNonQuery();
        }

        public static void SaveUserData(User user)
        {
            string fields = string.Empty;
            for (int i = 0; i < datas.Count; i++)
                fields += $",'{datas[i].id}' = '{user.info.dataAttributes[datas[i].id].value}'";

            command = new SQLiteCommand($"UPDATE '{dbName}' SET '{datas[0].id}' = '{user.info.dataAttributes[datas[0].id].value}'{fields} WHERE id = '{user.ID}';", connection);
            command.ExecuteNonQuery();
        }

        public static UserInfo CreateUser(string userID)
        {
            UserInfo info = new UserInfo(datas.Count);
            info.id = userID;

            string fields = string.Empty;
            string values = string.Empty;
            for(int i = 0; i < datas.Count; i++)
            {
                fields += $",'{datas[i].id}'";
                values += $",'{datas[i].value}'";
                info.dataAttributes.Add(datas[i].id, new DataProperty(datas[i].id, datas[i].value));
            }

            command = new SQLiteCommand($"INSERT INTO {dbName} ('id'{fields}) VALUES ('{userID}'{values});", connection);
            command.ExecuteNonQuery();

            return info;
        }
    }
}

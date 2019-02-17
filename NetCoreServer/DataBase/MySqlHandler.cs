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

using GameCore;
using GameCore.Game;

namespace SocketServer.Database
{
    internal static class MySqlHandler
    {
        private const string connectionCommand = "server = sql7.freemysqlhosting.net; User Id = sql7275273; password = i3CMg8Y7C2; database = sql7275273; Pooling = false";
        private const string dbName = "Users";

        private static SQLiteConnection connection;
        private static SQLiteCommand command;

        private static List<DataAttribute> datas;

        public static void Init(string path, string attributesPath)
        {
            command = new SQLiteCommand();
            datas = new List<DataAttribute>();

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
                        command.CommandText = $"ALTER TABLE {dbName} ADD COLUMN {components[0]} {components[1]};";
                        command.ExecuteNonQuery();
                    }
                    catch
                    {

                    }
                    finally
                    {
                        switch (components[1])
                        {
                            case "TEXT": datas.Add(new DataAttribute(components[0], components[2])); break;
                            case "INT": datas.Add(new DataAttribute(components[0], int.Parse(components[2]))); break;
                            case "FLOAT": datas.Add(new DataAttribute(components[0], float.Parse(components[2]))); break;
                            case "DOUBLE": datas.Add(new DataAttribute(components[0], double.Parse(components[2]))); break;
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
                        for (int i = 1; i < datas.Count; i++)
                            info.dataAttributes.Add(reader.GetName(i), new DataAttribute(reader.GetName(i), reader.GetValue(i)));
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
                            info.dataAttributes.Add(reader.GetName(k), new DataAttribute(reader.GetName(k), reader.GetValue(k)));
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
            command.CommandText = string.Format("UPDATE 'Users' SET {0} = '{1}' WHERE id = '{2}';", param, value, id);
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
                info.dataAttributes.Add(datas[i].id, datas[i]);
            }

            command.CommandText = $"INSERT INTO {dbName} ('id'{fields}) VALUES ('{userID}'{values});";
            command.ExecuteNonQuery();

            return info;
        }
    }
}

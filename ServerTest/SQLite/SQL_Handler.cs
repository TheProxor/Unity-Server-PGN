using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using System.IO;
using System.Data;

using PGN;
using PGN.General;

using GameCore;

namespace SocketServer.SQLite
{
    internal static class SQL_Handler
    {
        private static string path;

        private static SQLiteConnection connection;
        private static SQLiteCommand command;

        public static void Init(string _path)
        {
            path = _path;
            command = new SQLiteCommand();

            try
            {
                if (!File.Exists(path))
                {
                    SQLiteConnection.CreateFile(path);

                    connection = new SQLiteConnection("Data Source=" + path + ";Version=3;");
                    connection.Open();
                    command.Connection = connection;

                    if (GameHandler.game_bd_items != string.Empty && GameHandler.game_bd_items != null)
                        command.CommandText = string.Format("CREATE TABLE IF NOT EXISTS Users (id TEXT PRIMARY KEY, name TEXT, {0})", GameHandler.game_bd_items);
                    else
                        command.CommandText = "CREATE TABLE IF NOT EXISTS Users (id TEXT PRIMARY KEY, name TEXT)";

                    command.ExecuteNonQuery();

                }
                else
                {
                    connection = new SQLiteConnection("Data Source=" + path + ";Version=3;");
                    connection.Open();
                    command.Connection = connection;
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("SQL ERROR:" + ex.Message);
            }
        }

        public static UserInfo GetUserData(string id)
        {
            try
            {
                command = new SQLiteCommand("SELECT * FROM 'Users' WHERE id = '" + id +"';", connection);
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.StepCount > 0)
                {
                    UserInfo info = new UserInfo();
                    while (reader.Read())
                    {
                        info.id = reader.GetString(0);
                        info.name = reader.GetString(1);
                        info.fragments = (uint)reader.GetInt32(2);
                        info.crystals = (uint)reader.GetInt32(3);
                        info.experiance = (uint)reader.GetInt32(4);
                        info.level = (uint)reader.GetInt32(5);
                        info.items_ids = reader.GetString(6);
                        info.status = reader.GetString(7);
                        info.rang = reader.GetString(8);
                        info.freinds_ids = reader.GetString(9);
                    }
                    return info;
                }
                else
                    return null;
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
                command = new SQLiteCommand("SELECT * FROM 'Users' WHERE name LIKE '" + name + "';", connection);
                SQLiteDataReader reader = command.ExecuteReader();

                List<UserInfo> users = new List<UserInfo>();

                for (int i = 0; i < reader.StepCount; i++)
                {
                    while (reader.Read())
                    {
                        UserInfo info = new UserInfo();
                        info.id = reader.GetString(0);
                        info.name = reader.GetString(1);
                        info.fragments = (uint)reader.GetInt32(2);
                        info.crystals = (uint)reader.GetInt32(3);
                        info.experiance = (uint)reader.GetInt32(4);
                        info.level = (uint)reader.GetInt32(5);
                        info.items_ids = reader.GetString(6);
                        info.status = reader.GetString(7);
                        info.rang = reader.GetString(8);
                        info.freinds_ids = reader.GetString(9);
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

        public static void SetUserData(User user, UserInfo info)
        {
           //SQLiteDataAdapter
        }

        public static UserInfo CreateUser(User user)
        {
            UserInfo info = new UserInfo();

            info.fragments = 0;
            info.crystals = 0;
            info.experiance = 0;
            info.level = 1;
            info.status = string.Empty;
            info.rang = "Novice";
            info.freinds_ids = string.Empty;
            info.name = user.name;
            info.id = user.ID;

            

            SQLiteCommand command = new SQLiteCommand(string.Format("INSERT INTO 'Users' ('id', 'name', 'fragments', 'crystals', 'experience', 'level', 'status', 'rang', 'items', 'friends') VALUES ('{0}', '{1}', {2}, {3}, {4}, {5}, '{6}', '{7}', '{8}', '{9}');", user.ID, user.name, info.fragments, info.crystals, info.experiance, info.level, info.status, info.rang, info.items_ids ,info.freinds_ids), connection);
            command.ExecuteNonQuery();

            return info;
        }
    }
}

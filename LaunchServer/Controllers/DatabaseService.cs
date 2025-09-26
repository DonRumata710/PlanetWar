using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

using Interfaces.Models;
using LaunchServer.Models;

namespace LaunchServer.Controllers
{
    public class DatabaseService
    {
        public DatabaseService(string _login, string _password, string _server = "localhost", int _port = 3306)
        {
            login = _login;
            password = _password;
            server = _server;
            port = _port;
        }

        public Session GetSession(int id)
        {
            string strSQL = "SELECT * FROM `session_list` WHERE `id`=@id;";
            using (MySqlConnection connection = CreateConnection())
            using (MySqlCommand cmd = new MySqlCommand(strSQL, connection))
            {
                connection.Open();
                cmd.Parameters.Add("id", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = id;
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        SessionStartParameters parameters = GetSessionParameters(dr);
                        return new Session
                        {
                            Parameters = parameters,
                            Players = GetSessionPlayers(id)
                        };
                    }
                    dr.Close();
                }
            }

            throw new Exception("Failed get session information");
        }

        public Dictionary<int, Session> GetNotStartedSessions()
        {
            return GetSessions("WHERE `start_time` IS NULL");
        }

        public Dictionary<int, Session> GetSessions()
        {
            return GetSessions("");
        }

        private Dictionary<int, Session> GetSessions(string limit)
        {
            Dictionary<int, Session> res = new Dictionary<int, Session>();

            string strSQL = "SELECT * FROM `session_list` " + limit;
            using (MySqlConnection connection = CreateConnection())
            using (MySqlCommand cmd = new MySqlCommand(strSQL, connection))
            {
                connection.Open();
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        SessionStartParameters parameters = GetSessionParameters(dr);
                        int id = dr.GetInt32("id");
                        res.Add(id, new Session
                        {
                            Parameters = parameters,
                            Players = GetSessionPlayers(id)
                        });
                    }
                    dr.Close();
                }
                return res;
            }
        }

        private SessionStartParameters GetSessionParameters(MySqlDataReader dr)
        {
            return new SessionStartParameters
            {
                Name = dr.GetString("name"),
                Description = dr.GetString("description"),
                Size = dr.GetInt32("size"),
                PlanetCount = dr.GetInt32("planet_count"),
                PlayerLimit = dr.GetInt32("player_count")
            };
        }

        public List<int> GetSessionPlayers(int sessionId)
        {
            List<int> res = new List<int>();

            string strSQL = "SELECT `user_id` FROM `session_members` WHERE `session_id`=@session_id;";
            using (MySqlConnection connection = CreateConnection())
            using (MySqlCommand cmd = new MySqlCommand(strSQL, connection))
            {
                connection.Open();
                cmd.Parameters.Add("session_id", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = sessionId;
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        res.Add(dr.GetInt32("user_id"));
                    dr.Close();
                }
                return res;
            }
        }

        public int CreateNewSession(SessionStartParameters session)
        {
            string strSQL = "create_session";
            using (MySqlConnection connection = CreateConnection())
            using (MySqlCommand cmd = new MySqlCommand(strSQL, connection))
            {
                connection.Open();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add("sessionName", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = session.Name;
                cmd.Parameters.Add("sessionDescription", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = session.Description;
                cmd.Parameters.Add("size", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = session.Size;
                cmd.Parameters.Add("planetCount", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = session.PlanetCount;
                cmd.Parameters.Add("playerCount", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = session.PlayerLimit;
                cmd.Parameters.Add("id", MySql.Data.MySqlClient.MySqlDbType.Int32).Direction = System.Data.ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                return (int)cmd.Parameters["id"].Value;
            }

            throw new Exception("Failed create session");
        }

        public void RefreshSession(int id, SessionStartParameters session)
        {
            string strSQL = "UPDATE `session_list` SET `name`=@name, `description`=@description, `size`=@size, `planet_count`=@planet_count, `player_count`=@player_count;";
            using (MySqlConnection connection = CreateConnection())
            using (MySqlCommand cmd = new MySqlCommand(strSQL, connection))
            {
                connection.Open();
                cmd.Parameters.Add("name", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = session.Name;
                cmd.Parameters.Add("description", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = session.Description;
                cmd.Parameters.Add("size", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = session.Size;
                cmd.Parameters.Add("planet_count", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = session.PlanetCount;
                cmd.Parameters.Add("player_count", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = session.PlayerLimit;
                if (cmd.ExecuteNonQuery() != 1)
                {
                    throw new Exception("Failure while update session");
                }
            }
        }

        public void StartSession(int session, int server)
        {
            string strSQL = "call start_session(@sessionId, @serverId);";
            using (MySqlConnection connection = CreateConnection())
            using (MySqlCommand cmd = new MySqlCommand(strSQL, connection))
            {
                connection.Open();
                cmd.Parameters.Add("sessionId", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = session;
                cmd.Parameters.Add("serverId", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = server;
                cmd.ExecuteNonQuery();
            }
        }

        public void AddPlayer(int playerId, int sessionId)
        {
            string strSQL = "call add_player_to_session(@sessionId, @playerId);";
            using (MySqlConnection connection = CreateConnection())
            using (MySqlCommand cmd = new MySqlCommand(strSQL, connection))
            {
                connection.Open();
                cmd.Parameters.Add("sessionId", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = sessionId;
                cmd.Parameters.Add("playerId", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = playerId;
                cmd.ExecuteNonQuery();
            }
        }

        public void RemovePlayer(int playerId, int sessionId)
        {
            string strSQL = "call remove_player_from_session(@sessionId, @playerId);";
            using (MySqlConnection connection = CreateConnection())
            using (MySqlCommand cmd = new MySqlCommand(strSQL, connection))
            {
                connection.Open();
                cmd.Parameters.Add("sessionId", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = sessionId;
                cmd.Parameters.Add("playerId", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = playerId;
                cmd.ExecuteNonQuery();
            }
        }

        public void LeaveSessions(int playerId)
        {
            string strSQL = "call leave_all_sessions(@playerId);";
            using (MySqlConnection connection = CreateConnection())
            using (MySqlCommand cmd = new MySqlCommand(strSQL, connection))
            {
                connection.Open();
                cmd.Parameters.Add("playerId", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = playerId;
                cmd.ExecuteNonQuery();
            }
        }

        public void ClearOutSessionList()
        {
            string strSQL = "call clear_out_session_list();";
            using (MySqlConnection connection = CreateConnection())
            using (MySqlCommand cmd = new MySqlCommand(strSQL, connection))
            {
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public Dictionary<int, GameServer> ServerList()
        {
            string strSQL = "SELECT * FROM `server_list` WHERE `status`=1";
            using (MySqlConnection connection = CreateConnection())
            using (MySqlCommand cmd = new MySqlCommand(strSQL, connection))
            {
                connection.Open();
                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    Dictionary<int, GameServer> res = new Dictionary<int, GameServer>();
                    while (dr.Read())
                    {
                        GameServer serverModel = new GameServer();
                        serverModel.Address = dr.GetString("address");
                        serverModel.ActiveSessions = 0;
                        serverModel.SessionLimit = dr.GetInt32("session_limit");
                        res.Add(dr.GetInt32("server_id"), serverModel);
                    }
                    dr.Close();
                    return res;
                }
            }
        }

        public UserInfo GetUserInfo(string name)
        {
            string strSQL = "SELECT * FROM `user_list` WHERE `username`=@username";
            using (MySqlConnection connection = CreateConnection())
            using (MySqlCommand cmd = new MySqlCommand(strSQL, connection))
            {
                connection.Open();
                cmd.Parameters.Add("username", MySqlDbType.String).Value = name;
                return GetUserInfo(cmd);
            }
        }

        public UserInfo GetUserInfo(int id)
        {
            string strSQL = "SELECT * FROM `user_list` WHERE `user_id`=@user_id";
            using (MySqlConnection connection = CreateConnection())
            using (MySqlCommand cmd = new MySqlCommand(strSQL, connection))
            {
                connection.Open();
                cmd.Parameters.Add("user_id", MySqlDbType.Int32).Value = id;
                return GetUserInfo(cmd);
            }
        }

        private UserInfo GetUserInfo(MySqlCommand cmd)
        {
            using (MySqlConnection connection = CreateConnection())
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                connection.Open();
                UserInfo info = new UserInfo();
                if (dr.Read())
                {
                    info.email = !dr.IsDBNull(dr.GetOrdinal("email")) ? dr.GetString("email") : "";
                    info.userId = dr.GetUInt64("user_id");
                    info.name = dr.GetString("username");
                    info.registerTime = !dr.IsDBNull(dr.GetOrdinal("create_time")) ? dr.GetDateTime("create_time") : null;
                    info.isActive = dr.GetBoolean("active_user");
                }
                dr.Close();
                return info;
            }
        }

        private MySqlConnection CreateConnection()
        {
            return new MySqlConnection("server=" + server + ";user=" + login + ";port=" + port + ";database=planetwars;password=" + password + ";");
        }

        private string server;
        private string login;
        private int port;
        private string password;
    }
}

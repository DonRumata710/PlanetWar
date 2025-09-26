using System;
using System.Collections.Generic;

namespace ChatServer
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

        private string server;
        private string login;
        private int port;
        private string password;
    }
}

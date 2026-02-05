// WingsEmu
// 
// Developed by Ascension Team

using System;

namespace Plugin.Database.DB
{
    public class DatabaseConfiguration
    {
        public DatabaseConfiguration()
        {
            Ip = Environment.GetEnvironmentVariable("DATABASE_IP") ?? "localhost";
            Username = Environment.GetEnvironmentVariable("DATABASE_USER") ?? "postgres";
            Password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "uSUs8ZSa3zT9";
            Database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "game";
            WriteBufferSize = Convert.ToInt32(Environment.GetEnvironmentVariable("DATABASE_WRITE_BUFFER_SIZE") ?? "8192");
            ReadBufferSize = Convert.ToInt32(Environment.GetEnvironmentVariable("DATABASE_READ_BUFFER_SIZE") ?? "8192");
            IncludeErrorDetail = bool.Parse(Environment.GetEnvironmentVariable("DATABASE_ERROR_DETAIL") ?? "true");
            if (!ushort.TryParse(Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432", out ushort port))
            {
                port = 5432;
            }

            Port = port;
        }


        public string Ip { get; }
        public string Username { get; }
        public string Password { get; }
        public string Database { get; }
        public ushort Port { get; }
        public int WriteBufferSize { get; }
        public int ReadBufferSize { get; }
        public bool IncludeErrorDetail { get; }

        public override string ToString() => $"Host={Ip};Port={Port.ToString()}"
            + $";Database={Database}"
            + $";Username={Username}"
            + $";Password={Password}"
            + $";Read Buffer Size={ReadBufferSize.ToString()}"
            + $";Write Buffer Size={WriteBufferSize.ToString()}"
            + $";Include Error Detail={IncludeErrorDetail.ToString()}";
    }
}
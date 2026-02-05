// WingsEmu
// 
// Developed by Ascension Team

using System;
using Microsoft.EntityFrameworkCore;

namespace PhoenixLib.DAL.EFCore.PGSQL
{
    public class PgSqlDatabaseConfiguration<TDbContext> where TDbContext : DbContext
    {
        public PgSqlDatabaseConfiguration(string ip, string username, string password, string database, int port)
        {
            Ip = ip;
            Username = username;
            Password = password;
            Database = database;
            Port = port;
        }

        public string Ip { get; }
        public string Username { get; }
        public string Password { get; }
        public string Database { get; }
        public int Port { get; }

        public static PgSqlDatabaseConfiguration<TDbContext> FromEnv()
        {
            string ip = Environment.GetEnvironmentVariable("POSTGRES_DATABASE_IP") ?? "localhost";
            string username = Environment.GetEnvironmentVariable("POSTGRES_DATABASE_USER") ?? "postgres";
            string password = Environment.GetEnvironmentVariable("POSTGRES_DATABASE_PASSWORD") ?? "uSUs8ZSa3zT9";
            string database = Environment.GetEnvironmentVariable("POSTGRES_DATABASE_NAME") ?? "postgres";
            if (!ushort.TryParse(Environment.GetEnvironmentVariable("POSTGRES_DATABASE_PORT") ?? "5432", out ushort port))
            {
                port = 5432;
            }

            return new PgSqlDatabaseConfiguration<TDbContext>(ip, username, password, database, port);
        }

        public override string ToString() => $"Server={Ip};Port={Port};Database={Database};User Id={Username};Password={Password};";
    }
}
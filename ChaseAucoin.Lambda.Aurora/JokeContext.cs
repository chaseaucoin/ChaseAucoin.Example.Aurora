using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Renci.SshNet;
using System;
using System.IO;
using System.Reflection;

namespace ChaseAucoin.Aurora
{
    public class JokeContext : DbContext, IDisposable
    {
        SshClient _client;
        ForwardedPortLocal _local;
        MySqlConnection _connection;
        public JokeContext(string auroraAddress, string auroraPassword, string sshTunnelAddress, string sshUserName, string keyFileName, bool isDebug = false)
        {
            var builder = new MySqlConnectionStringBuilder();
            builder.UserID = "admin";
            builder.Password = auroraPassword;
            builder.Database = "jokes";
            builder.PersistSecurityInfo = true;
            builder.Port = 3306;

            if (isDebug)
            {
                var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);                
                PrivateKeyFile pkfile = new PrivateKeyFile($@"{dir}/{keyFileName}");
                _client = new SshClient(sshTunnelAddress, 22, sshUserName, pkfile);
                _client.Connect();

                if (_client.IsConnected)
                {
                    _local = new ForwardedPortLocal("127.0.0.1", 3306, auroraAddress, 3306);
                    _client.AddForwardedPort(_local);
                    _local.Start();
                }

                builder.Server = "127.0.0.1";
            }
            else
            {
                builder.Server = System.Net.Dns.GetHostEntry(auroraAddress)
                    .AddressList[0]
                    .MapToIPv4()
                    .ToString();
            }

            _connection = new MySqlConnection(builder.ConnectionString);
        }


        public DbSet<Joke> Jokes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(_connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public override void Dispose()
        {
            try { _connection.Close(); } catch { }
            try { _connection.Dispose(); } catch { }
            try { _client.Dispose(); } catch { }

            base.Dispose();
        }
    }
}
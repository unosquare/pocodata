﻿namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    public partial class SqlPocoDb : IPocoDb, IDisposable
    {
        internal static PocoSchema GlobalSchema = new PocoSchema();

        private bool IsDsposed;
        private readonly string ConnectionString;
        private SqlConnection SqlConnection;

        public SqlPocoDb()
        {
            PocoReader = new SqlPocoReader();
            Definition = new SqlPocoDefinition(this);
            Commands = new SqlPocoCommands(this);
        }

        public SqlPocoDb(string connectionString)
            : this()
        {
            ConnectionString = connectionString;
        }

        public int SqlCommandTimeoutSeconds { get; set; } = 60 * 5;

        public IDbConnection Connection
        {
            get
            {
                if (SqlConnection == null)
                {
                    SqlConnection = new SqlConnection(ConnectionString);
                    SqlConnection.Open();
                }

                return SqlConnection;
            }
        }

        public PocoSchema Schema => GlobalSchema;

        public IPocoReader PocoReader { get; }

        public IPocoDefinition Definition { get; }

        public IPocoCommands Commands { get; }

        public static async Task<SqlPocoDb> OpenAsync(string connectionString)
        {
            var result = new SqlPocoDb(connectionString)
            {
                SqlConnection = new SqlConnection(connectionString)
            };

            await result.SqlConnection.OpenAsync();
            return result;
        }

        public static async Task<SqlPocoDb> OpenAsync(string host, string username, string password, string databaseName) =>
            await OpenAsync($"Data Source={host}; User ID={username}; Password={password}; Initial Catalog={databaseName}; MultipleActiveResultSets=True;");

        public static async Task<SqlPocoDb> OpenLocalAsync(string databaseName) =>
            await OpenAsync($"Data Source=.; Integrated Security=True; Initial Catalog={databaseName}; MultipleActiveResultSets=True;");

        public static SqlPocoDb Open(string connectionString)
        {
            var result = new SqlPocoDb(connectionString)
            {
                SqlConnection = new SqlConnection(connectionString)
            };

            result.SqlConnection.Open();
            return result;
        }

        public static SqlPocoDb Open(string host, string username, string password, string databaseName) =>
            Open($"Data Source={host}; User ID={username}; Password={password}; Initial Catalog={databaseName}; MultipleActiveResultSets=True;");

        public static SqlPocoDb OpenLocal(string databaseName) =>
            Open($"Data Source=.; Integrated Security=True; Initial Catalog={databaseName}; MultipleActiveResultSets=True;");

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDsposed)
            {
                if (disposing)
                {
                    SqlConnection?.Dispose();
                    SqlConnection = null;
                }

                IsDsposed = true;
            }
        }
    }
}

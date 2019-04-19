namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a SQL Server-specific implementation of a POCO database functionality.
    /// </summary>
    /// <seealso cref="IPocoDb" />
    /// <seealso cref="IDisposable" />
    public partial class SqlPocoDb : IPocoDb, IDisposable
    {
        private readonly string ConnectionString;
        private SqlConnection SqlConnection;
        private bool IsDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlPocoDb"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SqlPocoDb(string connectionString)
            : this()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SqlPocoDb"/> class from being created.
        /// </summary>
        private SqlPocoDb()
        {
            Schema = PocoSchema.Instance;
            Definition = new SqlPocoDefinition(this);
            Commands = new SqlPocoCommands(this);
        }

        /// <summary>
        /// Gets or sets the command execution timeout in seconds.
        /// </summary>
        public int SqlCommandTimeoutSeconds { get; set; } = 60 * 5;

        /// <inheritdoc />
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

        /// <summary>
        /// Provides access to the global schema store.
        /// </summary>
        public PocoSchema Schema { get; }

        /// <inheritdoc />
        public PocoReader ObjectReader => PocoReader.Instance;

        /// <inheritdoc />
        public IPocoDefinition Definition { get; }

        /// <inheritdoc />
        public IPocoCommands Commands { get; }

        /// <summary>
        /// Asynchronously opens the connection in a new database container object.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The database container object.</returns>
        public static async Task<SqlPocoDb> OpenAsync(string connectionString)
        {
            var result = new SqlPocoDb(connectionString)
            {
                SqlConnection = new SqlConnection(connectionString)
            };

            await result.SqlConnection.OpenAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Asynchronously opens the connection in a new database container object.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>The database container object.</returns>
        public static async Task<SqlPocoDb> OpenAsync(string host, string username, string password, string databaseName) =>
            await OpenAsync($"Data Source={host}; User ID={username}; Password={password}; Initial Catalog={databaseName}; MultipleActiveResultSets=True;").ConfigureAwait(false);

        /// <summary>
        /// Asynchronously opens a connection to the local server with integrated credentials and using the specified database name.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>The database container object.</returns>
        public static async Task<SqlPocoDb> OpenLocalAsync(string databaseName) =>
            await OpenAsync($"Data Source=.; Integrated Security=True; Initial Catalog={databaseName}; MultipleActiveResultSets=True;").ConfigureAwait(false);

        /// <summary>
        /// Opens the database using the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The database container object.</returns>
        public static SqlPocoDb Open(string connectionString)
        {
            var result = new SqlPocoDb(connectionString)
            {
                SqlConnection = new SqlConnection(connectionString)
            };

            result.SqlConnection.Open();
            return result;
        }

        /// <summary>
        /// Opens the database using the specified common parameters.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>The database container object.</returns>
        public static SqlPocoDb Open(string host, string username, string password, string databaseName) =>
            Open($"Data Source={host}; User ID={username}; Password={password}; Initial Catalog={databaseName}; MultipleActiveResultSets=True;");

        /// <summary>
        /// Opens a connection to the local server with integrated credentials and using the specified database name.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>The database container object.</returns>
        public static SqlPocoDb OpenLocal(string databaseName) =>
            Open($"Data Source=.; Integrated Security=True; Initial Catalog={databaseName}; MultipleActiveResultSets=True;");

        /// <inheritdoc />
        public PocoTableProxy<T> TableProxy<T>()
            where T : class, new() => new PocoTableProxy<T>(this, false);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    SqlConnection?.Dispose();
                    SqlConnection = null;
                }

                IsDisposed = true;
            }
        }
    }
}

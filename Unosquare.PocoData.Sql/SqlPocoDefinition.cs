namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides DDL actions that are specific to SQL Server.
    /// </summary>
    /// <seealso cref="IPocoDefinition" />
    internal sealed class SqlPocoDefinition : IPocoDefinition
    {
        private readonly SqlPocoDb Parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlPocoDefinition"/> class.
        /// </summary>
        /// <param name="parent">The parent database container object.</param>
        internal SqlPocoDefinition(SqlPocoDb parent)
        {
            Parent = parent;
        }

        private SqlConnection Connection => Parent.Connection as SqlConnection;

        private PocoSchema Schema => PocoSchema.Instance;

        /// <inheritdoc />
        public async Task<int> CreateTableAsync(Type T)
        {
            var table = Schema.Table(T);
            var columns = Schema.Columns(T).Where(c => !c.IsKeyColumn);
            var primaryKeyCols = Schema.Columns(T).Where(c => c.IsKeyColumn);

            var columnDefs = new List<string>(64);
            foreach (var c in primaryKeyCols)
            {
                var sqlType = DbTypes.Map(c.NativeType).ToString().ToLowerInvariant();
                if (c.NativeType == typeof(string))
                    sqlType = $"{sqlType} ({c.StringLength})";

                columnDefs.Add($"{c.QualifiedName} {sqlType} {(c.IsKeyGenerated ? "IDENTITY(1,1)" : "NOT NULL")}");
            }

            foreach (var c in columns)
            {
                var sqlType = DbTypes.Map(c.NativeType).ToString().ToLowerInvariant();
                if (c.NativeType == typeof(string))
                    sqlType = $"{sqlType} ({c.StringLength})";

                columnDefs.Add($"{c.QualifiedName} {sqlType} {(c.IsNullable ? "NULL" : "NOT NULL")}");
            }

            var createTable = $"CREATE TABLE {table.QualifiedName} ({string.Join(", ", columnDefs)}, " +
                $"CONSTRAINT PK_{table.Name.Replace(" ", "_")} " +
                $"PRIMARY KEY ({string.Join(", ", primaryKeyCols.Select(c => c.QualifiedName))}))";

            var command = Connection.CreateCommand();
            command.CommandTimeout = Parent.SqlCommandTimeoutSeconds;
            command.CommandText = createTable;
            return await command.ExecuteNonQueryAsync();
        }

        /// <inheritdoc />
        public async Task<int> CreateTableAsync<T>() => await CreateTableAsync(typeof(T));

        /// <inheritdoc />
        public async Task<bool> TableExistsAsync(Type T)
        {
            var table = Schema.Table(T);
            var command = Connection.CreateCommand();
            command.CommandTimeout = Parent.SqlCommandTimeoutSeconds;
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @TableName";
            command.AddParameter("@Schema", table.Schema);
            command.AddParameter("@TableName", table.Name);

            var result = await command.ExecuteScalarAsync();
            return (int)result > 0;
        }

        /// <inheritdoc />
        public async Task<bool> TableExistsAsync<T>() => await TableExistsAsync(typeof(T));

        /// <inheritdoc />
        public async Task DropTableAsync(Type T)
        {
            var table = Schema.Table(T);
            var command = Connection.CreateCommand();
            command.CommandTimeout = Parent.SqlCommandTimeoutSeconds;
            command.CommandText = $"DROP TABLE {table.QualifiedName}";
            await command.ExecuteNonQueryAsync();
        }

        /// <inheritdoc />
        public async Task DropTableAsync<T>() => await DropTableAsync(typeof(T));
    }
}

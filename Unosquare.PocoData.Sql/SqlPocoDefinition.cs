namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    internal sealed class SqlPocoDefinition : IPocoDefinition
    {
        private readonly SqlPocoDb Parent;

        internal SqlPocoDefinition(SqlPocoDb parent)
        {
            Parent = parent;
        }

        private SqlConnection Connection => Parent.Connection as SqlConnection;

        private PocoSchema Schema => PocoSchema.Instance;

        public async Task<int> CreateTableAsync(Type T)
        {
            var table = Schema.Table(T);
            var columns = Schema.Columns(T).Where(c => !c.IsKeyColumn);
            var primaryKeyCols = Schema.Columns(T).Where(c => c.IsKeyColumn);

            var columnDefs = new List<string>(64);
            foreach (var c in primaryKeyCols)
            {
                var sqlType = DbTypes.Map(c.PropertyNativeType).ToString().ToLowerInvariant();
                if (c.PropertyNativeType == typeof(string))
                    sqlType = $"{sqlType} ({c.Length})";

                columnDefs.Add($"{c.QualifiedName} {sqlType} {(c.IsGenerated ? "IDENTITY(1,1)" : "NOT NULL")}");
            }

            foreach (var c in columns)
            {
                var sqlType = DbTypes.Map(c.PropertyNativeType).ToString().ToLowerInvariant();
                if (c.PropertyNativeType == typeof(string))
                    sqlType = $"{sqlType} ({c.Length})";

                columnDefs.Add($"{c.QualifiedName} {sqlType} {(c.IsNullable ? "NULL" : "NOT NULL")}");
            }

            var createTable = $"CREATE TABLE {table.QualifiedName} ({string.Join(", ", columnDefs)}, " +
                $"CONSTRAINT PK_{table.Name.Replace(" ", "_")} " +
                $"PRIMARY KEY ({string.Join(", ", primaryKeyCols.Select(c => c.QualifiedName))}))";

            var command = Connection.CreateCommand();
            command.CommandText = createTable;
            return await command.ExecuteNonQueryAsync();
        }

        public async Task<int> CreateTableAsync<T>() => await CreateTableAsync(typeof(T));

        public async Task<bool> TableExistsAsync(Type T)
        {
            var table = Schema.Table(T);
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @TableName";
            command.AddParameter("@Schema", table.Schema);
            command.AddParameter("@TableName", table.Name);

            var result = await command.ExecuteScalarAsync();
            return (int)result > 0;
        }

        public async Task<bool> TableExistsAsync<T>() => await TableExistsAsync(typeof(T));

        public async Task DropTableAsync(Type T)
        {
            var table = Schema.Table(T);
            var command = Connection.CreateCommand();
            command.CommandText = $"DROP TABLE {table.QualifiedName}";
            await command.ExecuteNonQueryAsync();
        }

        public async Task DropTableAsync<T>() => await DropTableAsync(typeof(T));
    }
}

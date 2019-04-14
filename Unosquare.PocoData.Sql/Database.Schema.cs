namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    public static partial class Database
    {
        public static async Task<int> CreateTableAsync(this SqlConnection connection, Type T)
        {
            var table = GetTableMap(T);
            var columns = GetColumnMap(T).Where(c => !c.IsKeyColumn);
            var primaryKeyCols = GetColumnMap(T).Where(c => c.IsKeyColumn);

            var columnDefs = new List<string>(64);
            foreach (var c in primaryKeyCols)
            {
                var sqlType = SqlTypes[c.PropertyNativeType].ToString().ToLowerInvariant();
                if (c.PropertyNativeType == typeof(string))
                    sqlType = $"{sqlType} ({c.Length})";

                columnDefs.Add($"{c.QualifiedName} {sqlType} {(c.IsGenerated ? "IDENTITY(1,1)" : "NOT NULL")}");
            }

            foreach (var c in columns)
            {
                var sqlType = SqlTypes[c.PropertyNativeType].ToString().ToLowerInvariant();
                if (c.PropertyNativeType == typeof(string))
                    sqlType = $"{sqlType} ({c.Length})";

                columnDefs.Add($"{c.QualifiedName} {sqlType} {(c.IsNullable ? "NULL" : "NOT NULL")}");
            }

            var createTable = $"CREATE TABLE {table.QualifiedName} ({string.Join(", ", columnDefs)}, " +
                $"CONSTRAINT PK_{table.Name.Replace(" ", "_")} " +
                $"PRIMARY KEY ({string.Join(", ", primaryKeyCols.Select(c => c.QualifiedName))}))";

            var command = connection.CreateCommand();
            command.CommandText = createTable;
            return await command.ExecuteNonQueryAsync();
        }

        public static async Task<int> CreateTableAsync<T>(this SqlConnection connection) => await connection.CreateTableAsync(typeof(T));

        public static async Task<bool> TableExistsAsync(this SqlConnection connection, Type T)
        {
            var table = GetTableMap(T);
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @TableName";
            command.AddParameter("@Schema", table.Schema);
            command.AddParameter("@TableName", table.Name);

            var result = await command.ExecuteScalarAsync();
            return (int)result > 0;
        }

        public static async Task<bool> TableExistsAsync<T>(this SqlConnection connection) => await connection.TableExistsAsync(typeof(T));

        public static async Task DropTableAsync(this SqlConnection connection, Type T)
        {
            var table = GetTableMap(T);
            var command = connection.CreateCommand();
            command.CommandText = $"DROP TABLE {table.QualifiedName}";
            await command.ExecuteNonQueryAsync();
        }

        public static async Task DropTableAsync<T>(this SqlConnection connection) => await connection.DropTableAsync(typeof(T));
    }
}

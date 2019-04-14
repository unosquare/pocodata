namespace Unosquare.PocoData.Sql
{
    using Annotations;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    public static partial class Database
    {
        public static async Task<List<T>> RetrieveAsync<T>(this SqlCommand command)
            where T : class, new()
        {
            var result = new List<T>(4096);
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Add(reader.ReadObject<T>());
                }
            }

            return result;
        }

        public static async Task<List<T>> RetrieveAsync<T>(this SqlConnection db, string tableName, params string[] columnNames)
            where T : class, new()
        {
            var selectArgument = columnNames == null || columnNames.Length == 0
                ? "*"
                : string.Join(", ", columnNames);

            using (var command = db.CreateCommand())
            {
                command.CommandTimeout = SqlCommandTimeoutSeconds;
                command.CommandText = $"SELECT {selectArgument} FROM {tableName}";
                return await command.RetrieveAsync<T>();
            }
        }

        public static async Task<List<T>> RetrieveAsync<T>(this SqlConnection db)
            where T : class, new()
        {
            var table = GetTableMap(typeof(T)) ?? throw new ArgumentException($"{typeof(T)} does not specify {typeof(TableAttribute)}");
            var columns = GetColumnMap(typeof(T));
            return await RetrieveAsync<T>(db, table.QualifiedName, columns.Select(c => c.QualifiedName).ToArray());
        }

        public static async Task<List<T>> RetrieveAsync<T>(this SqlConnection db, string sqlQueryText, int timeoutSeconds = 600)
            where T : class, new()
        {
            using (var command = db.CreateCommand())
            {
                command.CommandText = sqlQueryText;
                command.CommandTimeout = timeoutSeconds;
                return await command.RetrieveAsync<T>();
            }
        }

        public static async Task<int> InsertAsync(this SqlConnection db, object obj, bool update)
        {
            var T = obj.GetType();
            var columns = GetColumnMap(T);

            var generatedColumn = columns.FirstOrDefault(c => c.IsKeyColumn && c.IsGenerated);
            object insertResult;
            var insertCommand = db.CreateInsertCommand(obj);

            using (var tran = db.BeginTransaction())
            {
                insertCommand.Transaction = tran;
                insertResult = generatedColumn == null
                    ? await insertCommand.ExecuteNonQueryAsync()
                    : await insertCommand.ExecuteScalarAsync();
                generatedColumn?.SetValue(obj, Convert.ChangeType(insertResult, generatedColumn.PropertyNativeType));

                if (update)
                {
                    var selectCommand = db.CreateSelectCommand(obj);
                    selectCommand.Transaction = tran;
                    using (var reader = await selectCommand.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                            reader.ReadObject(obj);
                    }
                }

                tran.Commit();
            }

            return 1;
        }

        public static async Task<int> UpdateAsync(this SqlConnection db, object obj) =>
            await db.CreateUpdateCommand(obj).ExecuteNonQueryAsync();

        private static SqlCommand CreateInsertCommand(this SqlConnection db, object obj)
        {
            var T = obj.GetType();
            var columns = GetColumnMap(T);

            var command = db.CreateCommand();
            command.CommandText = GetCreateCommandText(T);
            foreach (var col in columns)
            {
                if (col.IsKeyColumn && col.IsGenerated)
                    continue;

                command.AddParameter(col.ParameterName, col.GetValue(obj));
            }

            return command;
        }

        private static SqlCommand CreateSelectCommand(this SqlConnection db, object obj)
        {
            var T = obj.GetType();
            var columns = GetColumnMap(T);

            var command = db.CreateCommand();
            command.CommandText = GetRetrieveCommandText(T);
            foreach (var col in columns)
            {
                if (!col.IsKeyColumn)
                    continue;

                command.AddParameter(col.ParameterName, col.GetValue(obj));
            }

            return command;
        }

        private static SqlCommand CreateUpdateCommand(this SqlConnection db, object obj)
        {
            var T = obj.GetType();
            var columns = GetColumnMap(T);

            var command = db.CreateCommand();
            command.CommandText = GetUpdateCommandText(T);
            foreach (var col in columns)
            {
                if (col.IsGenerated && !col.IsKeyColumn)
                    continue;

                command.AddParameter(col.ParameterName, col.GetValue(obj));
            }

            return command;
        }
    }
}

namespace Unosquare.PocoData.Sql
{
    using Annotations;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class SqlPocoDb
    {
        public async Task<IReadOnlyList<T>> RetrieveAsync<T>(SqlCommand command)
            where T : class, new()
        {
            var result = new List<T>(4096);
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Add(PocoReader.ReadObject<T>(reader));
                }
            }

            return result;
        }

        public async Task<IReadOnlyList<T>> RetrieveAsync<T>(string tableName, params string[] columnNames)
            where T : class, new()
        {
            var selectArgument = columnNames == null || columnNames.Length == 0
                ? "*"
                : string.Join(", ", columnNames);

            using (var command = SqlConnection.CreateCommand())
            {
                command.CommandTimeout = SqlCommandTimeoutSeconds;
                command.CommandText = $"SELECT {selectArgument} FROM {tableName}";
                return await RetrieveAsync<T>(command);
            }
        }

        public async Task<IReadOnlyList<T>> RetrieveAsync<T>()
            where T : class, new()
        {
            var table = Schema.Table(typeof(T)) ?? throw new ArgumentException($"{typeof(T)} does not specify {typeof(TableAttribute)}");
            var columns = Schema.Columns(typeof(T));
            return await RetrieveAsync<T>(table.QualifiedName, columns.Select(c => c.QualifiedName).ToArray());
        }

        public async Task<IReadOnlyList<T>> RetrieveAsync<T>(string sqlQueryText, int timeoutSeconds = 600)
            where T : class, new()
        {
            using (var command = SqlConnection.CreateCommand())
            {
                command.CommandText = sqlQueryText;
                command.CommandTimeout = timeoutSeconds;
                return await RetrieveAsync<T>(command);
            }
        }

        public async Task<int> InsertAsync(object obj, bool update)
        {
            var T = obj.GetType();
            var columns = Schema.Columns(T);

            var generatedColumn = columns.FirstOrDefault(c => c.IsKeyColumn && c.IsGenerated);
            object insertResult;
            var insertCommand = Commands.CreateInsertCommand(obj) as SqlCommand;

            using (var tran = SqlConnection.BeginTransaction())
            {
                insertCommand.Transaction = tran;
                insertResult = generatedColumn == null
                    ? await insertCommand.ExecuteNonQueryAsync()
                    : await insertCommand.ExecuteScalarAsync();
                generatedColumn?.SetValue(obj, Convert.ChangeType(insertResult, generatedColumn.PropertyNativeType));

                if (update)
                {
                    var selectCommand = Commands.CreateSelectCommand(obj) as SqlCommand;
                    selectCommand.Transaction = tran;
                    using (var reader = await selectCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            PocoReader.ReadObject(reader, obj);
                    }
                }

                tran.Commit();
            }

            return 1;
        }

        public async Task<int> UpdateAsync(object obj) =>
            await (Commands.CreateUpdateCommand(obj) as SqlCommand).ExecuteNonQueryAsync();

        public async Task<int> DeleteAsync(object obj) =>
            await (Commands.CreateDeleteCommand(obj) as SqlCommand).ExecuteNonQueryAsync();
    }
}

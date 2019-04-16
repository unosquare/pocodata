namespace Unosquare.PocoData.Sql
{
    using Annotations;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class SqlPocoDb
    {
        public IEnumerable SelectAll(Type T) => SelectMany(T, Commands.CreateSelectAllCommand(T));

        public async Task<IEnumerable> SelectAllAsync(Type T) => await SelectManyAsync(T, Commands.CreateSelectAllCommand(T));

        public IEnumerable<T> SelectAll<T>() where T : class, new() => SelectAll(typeof(T)).Cast<T>();

        public async Task<IEnumerable<T>> SelectAllAsync<T>() where T : class, new() => (await SelectAllAsync(typeof(T))).Cast<T>();

        public IEnumerable SelectMany(Type T, IDbCommand command)
        {
            var result = new List<object>(4096);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var item = Activator.CreateInstance(T);
                    result.Add(PocoReader.ReadObject(reader, item));
                }
            }

            return result;
        }

        public async Task<IEnumerable> SelectManyAsync(Type T, IDbCommand command)
        {
            var result = new List<object>(4096);
            var sqlCommand = command as SqlCommand;

            using (var reader = await sqlCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var item = Activator.CreateInstance(T);
                    result.Add(PocoReader.ReadObject(reader, item));
                }
            }

            return result;
        }

        public IEnumerable<T> SelectMany<T>(IDbCommand command) where T : class, new() => SelectMany(typeof(T), command).Cast<T>();

        public async Task<IEnumerable<T>> SelectManyAsync<T>(IDbCommand command) where T : class, new() => (await SelectManyAsync(typeof(T), command)).Cast<T>();

        public bool SelectSingle(object target)
        {
            var result = false;
            var command = Commands.CreateSelectSingleCommand(target);
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    PocoReader.ReadObject(reader, target);
                    result = true;
                }
            }

            return result;
        }

        public async Task<bool> SelectSingleAsync(object target)
        {
            var result = false;
            var command = Commands.CreateSelectSingleCommand(target) as SqlCommand;
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    PocoReader.ReadObject(reader, target);
                    result = true;
                }
            }

            return result;
        }

        public async Task<int> InsertAsync(object item, bool update)
        {
            var T = item.GetType();
            var columns = Schema.Columns(T);

            var generatedColumn = columns.FirstOrDefault(c => c.IsKeyColumn && c.IsKeyGenerated);
            object insertResult;
            var insertCommand = Commands.CreateInsertCommand(item) as SqlCommand;

            using (var tran = SqlConnection.BeginTransaction())
            {
                insertCommand.Transaction = tran;
                insertResult = generatedColumn == null
                    ? await insertCommand.ExecuteNonQueryAsync()
                    : await insertCommand.ExecuteScalarAsync();
                generatedColumn?.SetValue(item, Convert.ChangeType(insertResult, generatedColumn.NativeType));

                if (update)
                {
                    var selectCommand = Commands.CreateSelectSingleCommand(item) as SqlCommand;
                    selectCommand.Transaction = tran;
                    using (var reader = await selectCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            PocoReader.ReadObject(reader, item);
                    }
                }

                tran.Commit();
            }

            return 1;
        }

        public int Insert(object item, bool update)
        {
            var T = item.GetType();
            var columns = Schema.Columns(T);

            var generatedColumn = columns.FirstOrDefault(c => c.IsKeyColumn && c.IsKeyGenerated);
            object insertResult;
            var insertCommand = Commands.CreateInsertCommand(item);

            using (var tran = SqlConnection.BeginTransaction())
            {
                insertCommand.Transaction = tran;
                insertResult = generatedColumn == null
                    ? insertCommand.ExecuteNonQuery()
                    : insertCommand.ExecuteScalar();
                generatedColumn?.SetValue(item, Convert.ChangeType(insertResult, generatedColumn.NativeType));

                if (update)
                {
                    var selectCommand = Commands.CreateSelectSingleCommand(item);
                    selectCommand.Transaction = tran;
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                            PocoReader.ReadObject(reader, item);
                    }
                }

                tran.Commit();
            }

            return 1;
        }

        public async Task<int> InsertManyAsync(IEnumerable targetItems, bool update)
        {
            var insertCount = 0;
            var items = targetItems.Cast<object>();
            var firstItem = items.FirstOrDefault();
            if (firstItem == null) return 0;

            var T = firstItem.GetType();
            var columns = Schema.Columns(T);
            var insertCommandColumns = columns.Where(c => !c.IsKeyGenerated);
            var selectCommandColumns = columns.Where(c => c.IsKeyColumn);

            var generatedColumn = columns.FirstOrDefault(c => c.IsKeyColumn && c.IsKeyGenerated);
            object insertResult;

            // we will reuse the commands
            var insertCommand = Commands.CreateInsertCommand(firstItem) as SqlCommand;
            var selectCommand = Commands.CreateSelectSingleCommand(firstItem) as SqlCommand;

            using (var tran = SqlConnection.BeginTransaction())
            {
                insertCommand.Transaction = tran;
                selectCommand.Transaction = tran;

                insertCommand.Prepare();
                selectCommand.Prepare();

                foreach (var item in items)
                {
                    insertCommand.AddParameters(insertCommandColumns, item);

                    insertResult = generatedColumn == null
                        ? await insertCommand.ExecuteNonQueryAsync()
                        : await insertCommand.ExecuteScalarAsync();

                    generatedColumn?.SetValue(item, Convert.ChangeType(insertResult, generatedColumn.NativeType));

                    if (update)
                    {
                        selectCommand.AddParameters(selectCommandColumns, item);
                        using (var reader = await selectCommand.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                                PocoReader.ReadObject(reader, item);
                        }
                    }

                    insertCount++;
                }

                tran.Commit();
            }

            return insertCount;
        }

        public int InsertMany(IEnumerable targetItems, bool update)
        {
            var insertCount = 0;
            var items = targetItems.Cast<object>();
            var firstItem = items.FirstOrDefault();
            if (firstItem == null) return 0;

            var T = firstItem.GetType();
            var columns = Schema.Columns(T);
            var insertCommandColumns = columns.Where(c => !c.IsKeyGenerated);
            var selectCommandColumns = columns.Where(c => c.IsKeyColumn);

            var generatedColumn = columns.FirstOrDefault(c => c.IsKeyColumn && c.IsKeyGenerated);
            object insertResult;

            // we will reuse the commands
            var insertCommand = Commands.CreateInsertCommand(firstItem) as SqlCommand;
            var selectCommand = Commands.CreateSelectSingleCommand(firstItem) as SqlCommand;

            using (var tran = SqlConnection.BeginTransaction())
            {
                insertCommand.Transaction = tran;
                selectCommand.Transaction = tran;

                insertCommand.Prepare();
                selectCommand.Prepare();

                foreach (var item in items)
                {
                    insertCommand.AddParameters(insertCommandColumns, item);

                    insertResult = generatedColumn == null
                        ? insertCommand.ExecuteNonQuery()
                        : insertCommand.ExecuteScalar();

                    generatedColumn?.SetValue(item, Convert.ChangeType(insertResult, generatedColumn.NativeType));

                    if (update)
                    {
                        selectCommand.AddParameters(selectCommandColumns, item);
                        using (var reader = selectCommand.ExecuteReader())
                        {
                            if (reader.Read())
                                PocoReader.ReadObject(reader, item);
                        }
                    }

                    insertCount++;
                }

                tran.Commit();
            }

            return insertCount;
        }

        public async Task<int> UpdateAsync(object item) =>
            await (Commands.CreateUpdateCommand(item) as SqlCommand).ExecuteNonQueryAsync();

        public int Update(object item) =>
            Commands.CreateUpdateCommand(item).ExecuteNonQuery();

        public async Task<int> UpdateManyAsync(IEnumerable targetItems)
        {
            var updateCount = 0;
            var items = targetItems.Cast<object>();
            var firstItem = items.FirstOrDefault();
            if (firstItem == null) return 0;

            var T = firstItem.GetType();
            var columns = Schema.Columns(T);

            // we will reuse the command
            var updateCommand = Commands.CreateUpdateCommand(firstItem) as SqlCommand;

            using (var tran = SqlConnection.BeginTransaction())
            {
                updateCommand.Transaction = tran;
                updateCommand.Prepare();

                foreach (var item in items)
                {
                    updateCommand.AddParameters(columns, item);
                    updateCommand.DebugCommand();
                    updateCount += await updateCommand.ExecuteNonQueryAsync();
                }

                tran.Commit();
            }

            return updateCount;
        }

        public int UpdateMany(IEnumerable targetItems)
        {
            var updateCount = 0;
            var items = targetItems.Cast<object>();
            var firstItem = items.FirstOrDefault();
            if (firstItem == null) return 0;

            var T = firstItem.GetType();
            var columns = Schema.Columns(T);

            // we will reuse the command
            var updateCommand = Commands.CreateUpdateCommand(firstItem) as SqlCommand;

            using (var tran = SqlConnection.BeginTransaction())
            {
                updateCommand.Transaction = tran;
                updateCommand.Prepare();

                foreach (var item in items)
                {
                    updateCommand.AddParameters(columns, item);
                    updateCount += updateCommand.ExecuteNonQuery();
                }

                tran.Commit();
            }

            return updateCount;
        }

        public async Task<int> DeleteAsync(object obj) =>
            await (Commands.CreateDeleteCommand(obj) as SqlCommand).ExecuteNonQueryAsync();

        public int Delete(object obj) =>
            Commands.CreateDeleteCommand(obj).ExecuteNonQuery();

        public async Task<int> CountAllAsync(Type T) => Convert.ToInt32(await (Commands.CreateCountAllCommand(T) as SqlCommand).ExecuteScalarAsync());

        public int CountAll(Type T) => Convert.ToInt32(Commands.CreateCountAllCommand(T).ExecuteScalar());
    }
}

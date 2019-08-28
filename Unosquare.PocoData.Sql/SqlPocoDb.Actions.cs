namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class SqlPocoDb
    {
        /// <inheritdoc />
        public IEnumerable SelectAll(Type mappedType) => SelectMany(mappedType, Commands.CreateSelectAllCommand(mappedType));

        /// <inheritdoc />
        public async Task<IEnumerable> SelectAllAsync(Type mappedType) =>
            await SelectManyAsync(mappedType, Commands.CreateSelectAllCommand(mappedType)).ConfigureAwait(false);

        /// <inheritdoc />
        public IEnumerable<T> SelectAll<T>()
            where T : class, new() => SelectAll(typeof(T)).Cast<T>();

        /// <inheritdoc />
        public async Task<IEnumerable<T>> SelectAllAsync<T>()
            where T : class, new() => (await SelectAllAsync(typeof(T)).ConfigureAwait(false)).Cast<T>();

        /// <inheritdoc />
        public IEnumerable SelectMany(Type mappedType, IDbCommand command)
        {
            if(command == null)
            {
                throw new ArgumentNullException(string.Empty);
            }

            var result = new List<object>(4096);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var item = Activator.CreateInstance(mappedType);
                    result.Add(ObjectReader.ReadObject(reader, item));
                }
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<IEnumerable> SelectManyAsync(Type mappedType, IDbCommand command)
        {
            var result = new List<object>(4096);
            var sqlCommand = command as SqlCommand;

            if(sqlCommand == null)
            {
                throw new ArgumentNullException(string.Empty);
            }

            using (var reader = await sqlCommand.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var item = Activator.CreateInstance(mappedType);
                    result.Add(ObjectReader.ReadObject(reader, item));
                }
            }

            return result;
        }

        /// <inheritdoc />
        public IEnumerable<T> SelectMany<T>(IDbCommand command)
            where T : class, new() => SelectMany(typeof(T), command).Cast<T>();

        /// <inheritdoc />
        public async Task<IEnumerable<T>> SelectManyAsync<T>(IDbCommand command)
            where T : class, new() => (await SelectManyAsync(typeof(T), command).ConfigureAwait(false)).Cast<T>();

        /// <inheritdoc />
        public bool SelectSingle(object target)
        {
            var result = false;
            var command = Commands.CreateSelectSingleCommand(target);
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    ObjectReader.ReadObject(reader, target);
                    result = true;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<bool> SelectSingleAsync(object target)
        {
            var result = false;
            var command = Commands.CreateSelectSingleCommand(target) as SqlCommand;

            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    ObjectReader.ReadObject(reader, target);
                    result = true;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<int> InsertAsync(object item, bool update)
        {
            if(item == null)
            {
                throw new ArgumentNullException(string.Empty);
            }

            var itemType = item.GetType();
            var columns = Schema.Columns(itemType);

            var generatedColumn = columns.FirstOrDefault(c => c.IsKeyColumn && c.IsKeyGenerated);
            var insertCommand = Commands.CreateInsertCommand(item) as SqlCommand;

            using (var tran = SqlConnection.BeginTransaction())
            {
                insertCommand.Transaction = tran;
                var insertResult = generatedColumn == null
                    ? await insertCommand.ExecuteNonQueryAsync().ConfigureAwait(false)
                    : await insertCommand.ExecuteScalarAsync().ConfigureAwait(false);

                generatedColumn?.SetValue(item,
                    Convert.ChangeType(insertResult, generatedColumn.NativeType, CultureInfo.InvariantCulture));

                if (update)
                {
                    var selectCommand = Commands.CreateSelectSingleCommand(item) as SqlCommand;
                    selectCommand.Transaction = tran;
                    using (var reader = await selectCommand.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (await reader.ReadAsync().ConfigureAwait(false))
                            ObjectReader.ReadObject(reader, item);
                    }
                }

                tran.Commit();
            }

            return 1;
        }

        /// <inheritdoc />
        public int Insert(object item, bool update)
        {
            if(item == null)
            {
                throw new ArgumentNullException(string.Empty);
            }

            var itemType = item.GetType();
            var columns = Schema.Columns(itemType);

            var generatedColumn = columns.FirstOrDefault(c => c.IsKeyColumn && c.IsKeyGenerated);
            var insertCommand = Commands.CreateInsertCommand(item);

            using (var tran = SqlConnection.BeginTransaction())
            {
                insertCommand.Transaction = tran;
                var insertResult = generatedColumn == null
                    ? insertCommand.ExecuteNonQuery()
                    : insertCommand.ExecuteScalar();
                generatedColumn?.SetValue(item, Convert.ChangeType(insertResult, generatedColumn.NativeType, CultureInfo.InvariantCulture));

                if (update)
                {
                    var selectCommand = Commands.CreateSelectSingleCommand(item);
                    selectCommand.Transaction = tran;
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                            ObjectReader.ReadObject(reader, item);
                    }
                }

                tran.Commit();
            }

            return 1;
        }

        /// <inheritdoc />
        public async Task<int> InsertManyAsync(IEnumerable targetItems, bool update)
        {
            var insertCount = 0;
            var items = targetItems.Cast<object>();
            var firstItem = items.FirstOrDefault();
            if (firstItem == null) return 0;

            var mappedType = firstItem.GetType();
            var columns = Schema.Columns(mappedType);
            var insertCommandColumns = columns.Where(c => !c.IsKeyGenerated);
            var selectCommandColumns = columns.Where(c => c.IsKeyColumn);

            var generatedColumn = columns.FirstOrDefault(c => c.IsKeyColumn && c.IsKeyGenerated);

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
                    insertCommand.AddOrUpdateParameters(insertCommandColumns, item);

                    var insertResult = generatedColumn == null
                        ? await insertCommand.ExecuteNonQueryAsync().ConfigureAwait(false)
                        : await insertCommand.ExecuteScalarAsync().ConfigureAwait(false);

                    generatedColumn?.SetValue(item, Convert.ChangeType(insertResult, generatedColumn.NativeType, CultureInfo.InvariantCulture));

                    if (update)
                    {
                        selectCommand.AddOrUpdateParameters(selectCommandColumns, item);
                        using (var reader = await selectCommand.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            if (await reader.ReadAsync().ConfigureAwait(false))
                                ObjectReader.ReadObject(reader, item);
                        }
                    }

                    insertCount++;
                }

                tran.Commit();
            }

            return insertCount;
        }

        /// <inheritdoc />
        public int InsertMany(IEnumerable targetItems, bool update)
        {
            var insertCount = 0;
            var items = targetItems.Cast<object>();
            var firstItem = items.FirstOrDefault();
            if (firstItem == null) return 0;

            var mappedType = firstItem.GetType();
            var columns = Schema.Columns(mappedType);
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
                    insertCommand.AddOrUpdateParameters(insertCommandColumns, item);

                    insertResult = generatedColumn == null
                        ? insertCommand.ExecuteNonQuery()
                        : insertCommand.ExecuteScalar();

                    generatedColumn?.SetValue(item, Convert.ChangeType(insertResult, generatedColumn.NativeType, CultureInfo.InvariantCulture));

                    if (update)
                    {
                        selectCommand.AddOrUpdateParameters(selectCommandColumns, item);
                        using (var reader = selectCommand.ExecuteReader())
                        {
                            if (reader.Read())
                                ObjectReader.ReadObject(reader, item);
                        }
                    }

                    insertCount++;
                }

                tran.Commit();
            }

            return insertCount;
        }

        /// <inheritdoc />
        public async Task<int> UpdateAsync(object item) =>
            await (Commands.CreateUpdateCommand(item) as SqlCommand).ExecuteNonQueryAsync().ConfigureAwait(false);

        /// <inheritdoc />
        public int Update(object item) =>
            Commands.CreateUpdateCommand(item).ExecuteNonQuery();

        /// <inheritdoc />
        public async Task<int> UpdateManyAsync(IEnumerable targetItems)
        {
            var updateCount = 0;
            var items = targetItems.Cast<object>();
            var firstItem = items.FirstOrDefault();
            if (firstItem == null) return 0;

            var mappedType = firstItem.GetType();
            var columns = Schema.Columns(mappedType);

            // we will reuse the command
            var updateCommand = Commands.CreateUpdateCommand(firstItem) as SqlCommand;

            using (var tran = SqlConnection.BeginTransaction())
            {
                updateCommand.Transaction = tran;
                updateCommand.Prepare();

                foreach (var item in items)
                {
                    updateCommand.AddOrUpdateParameters(columns, item);
                    updateCount += await updateCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                tran.Commit();
            }

            return updateCount;
        }

        /// <inheritdoc />
        public int UpdateMany(IEnumerable targetItems)
        {
            var updateCount = 0;
            var items = targetItems.Cast<object>();
            var firstItem = items.FirstOrDefault();
            if (firstItem == null) return 0;

            var mappedType = firstItem.GetType();
            var columns = Schema.Columns(mappedType);

            // we will reuse the command
            var updateCommand = Commands.CreateUpdateCommand(firstItem) as SqlCommand;

            using (var tran = SqlConnection.BeginTransaction())
            {
                updateCommand.Transaction = tran;
                updateCommand.Prepare();

                foreach (var item in items)
                {
                    updateCommand.AddOrUpdateParameters(columns, item);
                    updateCount += updateCommand.ExecuteNonQuery();
                }

                tran.Commit();
            }

            return updateCount;
        }

        /// <inheritdoc />
        public async Task<int> DeleteAsync(object item) =>
            await (Commands.CreateDeleteCommand(item) as SqlCommand).ExecuteNonQueryAsync().ConfigureAwait(false);

        /// <inheritdoc />
        public int Delete(object item) =>
            Commands.CreateDeleteCommand(item).ExecuteNonQuery();

        /// <inheritdoc />
        public async Task<int> CountAllAsync(Type mappedType) =>
            Convert.ToInt32(await (Commands.CreateCountAllCommand(mappedType) as SqlCommand).ExecuteScalarAsync().ConfigureAwait(false), CultureInfo.InvariantCulture);

        /// <inheritdoc />
        public int CountAll(Type mappedType) =>
            Convert.ToInt32(Commands.CreateCountAllCommand(mappedType).ExecuteScalar(), CultureInfo.InvariantCulture);
    }
}

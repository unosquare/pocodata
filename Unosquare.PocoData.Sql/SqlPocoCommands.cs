namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;

    /// <summary>
    /// Provides SQL Server-specific implementation of a POCO command generator.
    /// </summary>
    /// <seealso cref="IPocoCommands" />
    internal sealed class SqlPocoCommands : IPocoCommands
    {
        private static readonly object SyncLock = new object();

        private static readonly Dictionary<Type, string> InsertCommandTexts = new Dictionary<Type, string>(32);
        private static readonly Dictionary<Type, string> UpdateCommandTexts = new Dictionary<Type, string>(32);
        private static readonly Dictionary<Type, string> DeleteCommandTexts = new Dictionary<Type, string>(32);
        private static readonly Dictionary<Type, string> SelectSingleCommandTexts = new Dictionary<Type, string>(32);
        private static readonly Dictionary<Type, string> SelectAllCommandTexts = new Dictionary<Type, string>(32);

        private readonly SqlPocoDb Parent;

        internal SqlPocoCommands(SqlPocoDb parent)
        {
            Schema = PocoSchema.Instance;
            Parent = parent;
        }

        private PocoSchema Schema { get; }

        private SqlConnection Connection => Parent.Connection as SqlConnection;

        /// <inheritdoc />
        public string SelectAllCommandText(Type mappedType)
        {
            lock (SyncLock)
            {
                if (SelectAllCommandTexts.ContainsKey(mappedType))
                    return SelectAllCommandTexts[mappedType];

                var table = Schema.Table(mappedType);
                var columns = Schema.Columns(mappedType);
                var columnNames = columns.Select(c => c.QualifiedName);

                var commandText =
                    $"SELECT {string.Join(", ", columnNames)} FROM {table.QualifiedName}";

                SelectAllCommandTexts[mappedType] = commandText;
                return commandText;
            }
        }

        /// <inheritdoc />
        public string SelectSingleCommandText(Type mappedType)
        {
            lock (SyncLock)
            {
                if (SelectSingleCommandTexts.ContainsKey(mappedType))
                    return SelectSingleCommandTexts[mappedType];

                var table = Schema.Table(mappedType);
                var columns = Schema.Columns(mappedType);

                var columnNames = columns.Select(c => c.QualifiedName);
                var parameterNames = columns.Where(c => c.IsKeyColumn).Select(c => $"{c.QualifiedName} = {c.ParameterName}");

                var commandText =
                    $"SELECT {string.Join(", ", columnNames)} FROM {table.QualifiedName} WHERE {string.Join(" AND ", parameterNames)}";

                SelectSingleCommandTexts[mappedType] = commandText;
                return commandText;
            }
        }

        /// <inheritdoc />
        public string InsertCommandText(Type mappedType)
        {
            lock (SyncLock)
            {
                if (InsertCommandTexts.ContainsKey(mappedType))
                    return InsertCommandTexts[mappedType];

                var table = Schema.Table(mappedType);
                var columns = Schema.Columns(mappedType).Where(c => c.IsKeyGenerated == false);

                var columnNames = columns.Select(c => c.QualifiedName);
                var parameterNames = columns.Select(c => c.ParameterName);

                var commandText =
                    $"INSERT INTO {table.QualifiedName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", parameterNames)}) SELECT SCOPE_IDENTITY()";

                InsertCommandTexts[mappedType] = commandText;
                return commandText;
            }
        }

        /// <inheritdoc />
        public string UpdateCommandText(Type mappedType)
        {
            lock (SyncLock)
            {
                if (UpdateCommandTexts.ContainsKey(mappedType))
                    return UpdateCommandTexts[mappedType];

                var table = Schema.Table(mappedType);
                var setColumns = Schema.Columns(mappedType).Where(c => c.IsKeyGenerated == false && c.IsKeyColumn == false);
                var keyColumns = Schema.Columns(mappedType).Where(c => c.IsKeyColumn);

                var setArgument = setColumns.Select(col => $"{col.QualifiedName} = {col.ParameterName}");
                var keyArgument = keyColumns.Select(col => $"{col.QualifiedName} = {col.ParameterName}");

                var commandText =
                    $"UPDATE {table.QualifiedName} SET {string.Join(", ", setArgument)} WHERE {string.Join(" AND ", keyArgument)}";

                UpdateCommandTexts[mappedType] = commandText;
                return commandText;
            }
        }

        /// <inheritdoc />
        public string DeleteCommandText(Type mappedType)
        {
            lock (SyncLock)
            {
                if (DeleteCommandTexts.ContainsKey(mappedType))
                    return DeleteCommandTexts[mappedType];

                var table = Schema.Table(mappedType);
                var keyColumns = Schema.Columns(mappedType).Where(c => c.IsKeyColumn);
                var keyArgument = keyColumns.Select(col => $"{col.QualifiedName} = {col.ParameterName}");

                var commandText =
                    $"DELETE FROM {table.QualifiedName} WHERE {string.Join(" AND ", keyArgument)}";

                DeleteCommandTexts[mappedType] = commandText;
                return commandText;
            }
        }

        /// <inheritdoc />
        public string CountAllCommandText(Type mappedType) =>
            $"SELECT COUNT (*) FROM {Schema.Table(mappedType).QualifiedName}";

        /// <inheritdoc />
        public IDbCommand CreateSelectAllCommand(Type mappedType)
        {
            var command = Connection.CreateCommand();
            command.CommandTimeout = Parent.SqlCommandTimeoutSeconds;
            command.CommandText = SelectAllCommandText(mappedType);
            return command;
        }

        /// <inheritdoc />
        public IDbCommand CreateSelectSingleCommand(object item)
        {
            var mappedTypeT = item.GetType();
            var columns = Schema.Columns(mappedTypeT);

            var command = Connection.CreateCommand();
            command.CommandTimeout = Parent.SqlCommandTimeoutSeconds;
            command.CommandText = SelectSingleCommandText(mappedTypeT);
            command.AddOrUpdateParameters(columns.Where(c => c.IsKeyColumn), item);
            return command;
        }

        /// <inheritdoc />
        public IDbCommand CreateInsertCommand(object item)
        {
            var mappedType = item.GetType();
            var columns = Schema.Columns(mappedType);

            var command = Connection.CreateCommand();
            command.CommandTimeout = Parent.SqlCommandTimeoutSeconds;
            command.CommandText = InsertCommandText(mappedType);
            command.AddOrUpdateParameters(columns.Where(c => !c.IsKeyGenerated), item);
            return command;
        }

        /// <inheritdoc />
        public IDbCommand CreateUpdateCommand(object item)
        {
            var mappedType = item.GetType();
            var columns = Schema.Columns(mappedType);

            var command = Connection.CreateCommand();
            command.CommandTimeout = Parent.SqlCommandTimeoutSeconds;
            command.CommandText = UpdateCommandText(mappedType);
            command.AddOrUpdateParameters(columns, item);
            return command;
        }

        /// <inheritdoc />
        public IDbCommand CreateDeleteCommand(object item)
        {
            var mappedType = item.GetType();
            var keyColumns = Schema.Columns(mappedType).Where(c => c.IsKeyColumn);

            var command = Connection.CreateCommand();
            command.CommandTimeout = Parent.SqlCommandTimeoutSeconds;
            command.CommandText = DeleteCommandText(mappedType);
            command.AddOrUpdateParameters(keyColumns, item);

            return command;
        }

        /// <inheritdoc />
        public IDbCommand CreateCountAllCommand(Type mappedType)
        {
            var command = Connection.CreateCommand();
            command.CommandTimeout = Parent.SqlCommandTimeoutSeconds;
            command.CommandText = CountAllCommandText(mappedType);

            return command;
        }
    }
}

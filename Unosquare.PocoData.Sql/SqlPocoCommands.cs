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
            Parent = parent;
        }

        private PocoSchema Schema => PocoSchema.Instance;

        private SqlConnection Connection => Parent.Connection as SqlConnection;

        /// <inheritdoc />
        public string SelectAllCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (SelectAllCommandTexts.ContainsKey(T))
                    return SelectAllCommandTexts[T];

                var table = Schema.Table(T);
                var columns = Schema.Columns(T);
                var columnNames = columns.Select(c => c.QualifiedName);

                var commandText =
                    $"SELECT {string.Join(", ", columnNames)} FROM {table.QualifiedName}";

                SelectAllCommandTexts[T] = commandText;
                return commandText;
            }
        }

        /// <inheritdoc />
        public string SelectSingleCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (SelectSingleCommandTexts.ContainsKey(T))
                    return SelectSingleCommandTexts[T];

                var table = Schema.Table(T);
                var columns = Schema.Columns(T);

                var columnNames = columns.Select(c => c.QualifiedName);
                var parameterNames = columns.Where(c => c.IsKeyColumn).Select(c => $"{c.QualifiedName} = {c.ParameterName}");

                var commandText =
                    $"SELECT {string.Join(", ", columnNames)} FROM {table.QualifiedName} WHERE {string.Join(" AND ", parameterNames)}";

                SelectSingleCommandTexts[T] = commandText;
                return commandText;
            }
        }

        /// <inheritdoc />
        public string InsertCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (InsertCommandTexts.ContainsKey(T))
                    return InsertCommandTexts[T];

                var table = Schema.Table(T);
                var columns = Schema.Columns(T).Where(c => c.IsKeyGenerated == false);

                var columnNames = columns.Select(c => c.QualifiedName);
                var parameterNames = columns.Select(c => c.ParameterName);

                var commandText =
                    $"INSERT INTO {table.QualifiedName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", parameterNames)}) SELECT SCOPE_IDENTITY()";

                InsertCommandTexts[T] = commandText;
                return commandText;
            }
        }

        /// <inheritdoc />
        public string UpdateCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (UpdateCommandTexts.ContainsKey(T))
                    return UpdateCommandTexts[T];

                var table = Schema.Table(T);
                var setColumns = Schema.Columns(T).Where(c => c.IsKeyGenerated == false && c.IsKeyColumn == false);
                var keyColumns = Schema.Columns(T).Where(c => c.IsKeyColumn);

                var setArgument = setColumns.Select(col => $"{col.QualifiedName} = {col.ParameterName}");
                var keyArgument = keyColumns.Select(col => $"{col.QualifiedName} = {col.ParameterName}");

                var commandText =
                    $"UPDATE {table.QualifiedName} SET {string.Join(", ", setArgument)} WHERE {string.Join(" AND ", keyArgument)}";

                UpdateCommandTexts[T] = commandText;
                return commandText;
            }
        }

        /// <inheritdoc />
        public string DeleteCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (DeleteCommandTexts.ContainsKey(T))
                    return DeleteCommandTexts[T];

                var table = Schema.Table(T);
                var keyColumns = Schema.Columns(T).Where(c => c.IsKeyColumn);
                var keyArgument = keyColumns.Select(col => $"{col.QualifiedName} = {col.ParameterName}");

                var commandText =
                    $"DELETE FROM {table.QualifiedName} WHERE {string.Join(" AND ", keyArgument)}";

                DeleteCommandTexts[T] = commandText;
                return commandText;
            }
        }

        /// <inheritdoc />
        public string CountAllCommandText(Type T) =>
            $"SELECT COUNT (*) FROM {Schema.Table(T).QualifiedName}";

        /// <inheritdoc />
        public IDbCommand CreateSelectAllCommand(Type T)
        {
            var command = Connection.CreateCommand();
            command.CommandTimeout = Parent.SqlCommandTimeoutSeconds;
            command.CommandText = SelectAllCommandText(T);
            return command;
        }

        /// <inheritdoc />
        public IDbCommand CreateSelectSingleCommand(object obj)
        {
            var T = obj.GetType();
            var columns = Schema.Columns(T);

            var command = Connection.CreateCommand();
            command.CommandTimeout = Parent.SqlCommandTimeoutSeconds;
            command.CommandText = SelectSingleCommandText(T);
            command.AddOrUpdateParameters(columns.Where(c => c.IsKeyColumn), obj);
            return command;
        }

        /// <inheritdoc />
        public IDbCommand CreateInsertCommand(object obj)
        {
            var T = obj.GetType();
            var columns = Schema.Columns(T);

            var command = Connection.CreateCommand();
            command.CommandTimeout = Parent.SqlCommandTimeoutSeconds;
            command.CommandText = InsertCommandText(T);
            command.AddOrUpdateParameters(columns.Where(c => !c.IsKeyGenerated), obj);
            return command;
        }

        /// <inheritdoc />
        public IDbCommand CreateUpdateCommand(object obj)
        {
            var T = obj.GetType();
            var columns = Schema.Columns(T);

            var command = Connection.CreateCommand();
            command.CommandTimeout = Parent.SqlCommandTimeoutSeconds;
            command.CommandText = UpdateCommandText(T);
            command.AddOrUpdateParameters(columns, obj);
            return command;
        }

        /// <inheritdoc />
        public IDbCommand CreateDeleteCommand(object obj)
        {
            var T = obj.GetType();
            var keyColumns = Schema.Columns(T).Where(c => c.IsKeyColumn);

            var command = Connection.CreateCommand();
            command.CommandTimeout = Parent.SqlCommandTimeoutSeconds;
            command.CommandText = DeleteCommandText(T);
            command.AddOrUpdateParameters(keyColumns, obj);

            return command;
        }

        /// <inheritdoc />
        public IDbCommand CreateCountAllCommand(Type T)
        {
            var command = Connection.CreateCommand();
            command.CommandTimeout = Parent.SqlCommandTimeoutSeconds;
            command.CommandText = CountAllCommandText(T);

            return command;
        }
    }
}

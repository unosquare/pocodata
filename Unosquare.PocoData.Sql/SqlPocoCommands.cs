namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;

    public sealed class SqlPocoCommands : IPocoCommands
    {
        private static readonly object SyncLock = new object();

        private static readonly Dictionary<Type, string> InsertCommandTexts = new Dictionary<Type, string>(32);
        private static readonly Dictionary<Type, string> UpdateCommandTexts = new Dictionary<Type, string>(32);
        private static readonly Dictionary<Type, string> DeleteCommandTexts = new Dictionary<Type, string>(32);
        private static readonly Dictionary<Type, string> SelectCommandTexts = new Dictionary<Type, string>(32);

        private readonly SqlPocoDb Parent;

        internal SqlPocoCommands(SqlPocoDb parent)
        {
            Parent = parent;
        }

        private PocoSchema Schema => SqlPocoDb.GlobalSchema;

        private SqlConnection Connection => Parent.Connection as SqlConnection;

        public string SelectCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (SelectCommandTexts.ContainsKey(T))
                    return SelectCommandTexts[T];

                var table = Schema.Table(T);
                var columns = Schema.Columns(T);

                var columnNames = columns.Select(c => c.QualifiedName);
                var parameterNames = columns.Where(c => c.IsKeyColumn).Select(c => $"{c.QualifiedName} = {c.ParameterName}");

                var commandText =
                    $"SELECT {string.Join(", ", columnNames)} FROM {table.QualifiedName} WHERE {string.Join(" AND ", parameterNames)}";

                SelectCommandTexts[T] = commandText;
                return commandText;
            }
        }

        public string InsertCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (InsertCommandTexts.ContainsKey(T))
                    return InsertCommandTexts[T];

                var table = Schema.Table(T);
                var columns = Schema.Columns(T).Where(c => c.IsGenerated == false);

                var columnNames = columns.Select(c => c.QualifiedName);
                var parameterNames = columns.Select(c => c.ParameterName);

                var commandText =
                    $"INSERT INTO {table.QualifiedName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", parameterNames)}) SELECT SCOPE_IDENTITY()";

                InsertCommandTexts[T] = commandText;
                return commandText;
            }
        }

        public string InsertCommandText<T>() => InsertCommandText(typeof(T));

        public string UpdateCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (UpdateCommandTexts.ContainsKey(T))
                    return UpdateCommandTexts[T];

                var table = Schema.Table(T);
                var setColumns = Schema.Columns(T).Where(c => c.IsGenerated == false && c.IsKeyColumn == false);
                var keyColumns = Schema.Columns(T).Where(c => c.IsKeyColumn);

                var setArgument = setColumns.Select(col => $"{col.QualifiedName} = {col.ParameterName}");
                var keyArgument = keyColumns.Select(col => $"{col.QualifiedName} = {col.ParameterName}");

                var commandText =
                    $"UPDATE {table.QualifiedName} SET {string.Join(", ", setArgument)} WHERE {string.Join(" AND ", keyArgument)}";

                UpdateCommandTexts[T] = commandText;
                return commandText;
            }
        }

        public string UpdateCommandText<T>() => UpdateCommandText(typeof(T));

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

        public string DeleteCommandText<T>() => DeleteCommandText(typeof(T));


        public IDbCommand CreateInsertCommand(object obj)
        {
            var T = obj.GetType();
            var columns = Schema.Columns(T);

            var command = Connection.CreateCommand();
            command.CommandText = InsertCommandText(T);
            foreach (var col in columns)
            {
                if (col.IsKeyColumn && col.IsGenerated)
                    continue;

                command.AddParameter(col.ParameterName, col.GetValue(obj));
            }

            return command;
        }

        public IDbCommand CreateSelectCommand( object obj)
        {
            var T = obj.GetType();
            var columns = Schema.Columns(T);

            var command = Connection.CreateCommand();
            command.CommandText = SelectCommandText(T);
            foreach (var col in columns)
            {
                if (!col.IsKeyColumn)
                    continue;

                command.AddParameter(col.ParameterName, col.GetValue(obj));
            }

            return command;
        }

        public IDbCommand CreateUpdateCommand(object obj)
        {
            var T = obj.GetType();
            var columns = Schema.Columns(T);

            var command = Connection.CreateCommand();
            command.CommandText = UpdateCommandText(T);
            foreach (var col in columns)
            {
                if (col.IsGenerated && !col.IsKeyColumn)
                    continue;

                command.AddParameter(col.ParameterName, col.GetValue(obj));
            }

            return command;
        }

        public IDbCommand CreateDeleteCommand(object obj)
        {
            throw new NotImplementedException();
        }
    }
}

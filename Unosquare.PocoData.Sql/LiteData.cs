namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public static partial class LiteData
    {
        private static readonly IReadOnlyList<Type> PersistableValueTypes = new List<Type>()
        {
            typeof(sbyte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(byte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(char),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(bool),
            typeof(Guid),
            typeof(DateTime)
        };
        private static readonly IReadOnlyDictionary<Type, SqlDbType> SqlTypes = new Dictionary<Type, SqlDbType>()
        {
            { typeof(string), SqlDbType.NVarChar },
            { typeof(sbyte), SqlDbType.SmallInt },
            { typeof(short), SqlDbType.SmallInt },
            { typeof(int), SqlDbType.Int },
            { typeof(long), SqlDbType.BigInt },
            { typeof(byte), SqlDbType.TinyInt },
            { typeof(ushort), SqlDbType.Int },
            { typeof(uint), SqlDbType.BigInt },
            { typeof(ulong), SqlDbType.Float },
            { typeof(char), SqlDbType.NChar },
            { typeof(float), SqlDbType.Float },
            { typeof(double), SqlDbType.Float },
            { typeof(decimal), SqlDbType.Money },
            { typeof(bool), SqlDbType.Bit },
            { typeof(Guid), SqlDbType.UniqueIdentifier },
            { typeof(DateTime), SqlDbType.DateTime },
        };

        private static readonly Dictionary<Type, string> CreateCommandText = new Dictionary<Type, string>(32);
        private static readonly Dictionary<Type, string> UpdateCommandText = new Dictionary<Type, string>(32);
        private static readonly Dictionary<Type, string> DeleteCommandText = new Dictionary<Type, string>(32);
        private static readonly Dictionary<Type, string> RetrieveCommandText = new Dictionary<Type, string>(32);

        public static int SqlCommandTimeoutSeconds { get; set; } = 60 * 5;

        public static async Task<SqlConnection> OpenConnectionAsync(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public static async Task<SqlConnection> OpenLocalConnectionAsync(string databaseName) =>
            await OpenConnectionAsync($"Data Source=.; Integrated Security=True; Initial Catalog={databaseName}; MultipleActiveResultSets=True;");

        public static async Task<SqlConnection> OpenConnectionAsync(string host, string username, string password, string databaseName) =>
            await OpenConnectionAsync($"Data Source={host}; User ID={username}; Password={password}; Initial Catalog={databaseName}; MultipleActiveResultSets=True;");

        public static SqlParameter AddParameter(this SqlCommand command, string parameterName, SqlDbType dbType, object value)
        {
            var param = command.CreateParameter();
            param.ParameterName = parameterName;
            param.SqlDbType = dbType;
            param.Value = value ?? DBNull.Value;

            command.Parameters.Add(param);
            return param;
        }

        public static SqlParameter AddParameter(this SqlCommand command, string parameterName, object value)
        {
            var param = command.CreateParameter();
            param.ParameterName = parameterName;
            param.Value = value ?? DBNull.Value;
            if (value != null && SqlTypes.ContainsKey(value.GetType()))
                param.SqlDbType = SqlTypes[value.GetType()];

            command.Parameters.Add(param);
            return param;
        }

        public static IReadOnlyList<SqlParameter> AddParameters(this SqlCommand command, object values)
        {
            var result = new List<SqlParameter>(32);
            var properties = values.GetType().GetProperties(BindingFlags.Public).ToArray();
            foreach (var p in properties)
            {
                result.Add(command.AddParameter(p.Name, p.GetValue(values)));
            }

            return result;
        }

        public static string GetRetrieveCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (RetrieveCommandText.ContainsKey(T))
                    return RetrieveCommandText[T];

                var table = GetTableMap(T);
                var columns = GetColumnMap(T);

                var columnNames = columns.Select(c => c.QualifiedName);
                var parameterNames = columns.Where(c => c.IsKeyColumn).Select(c => $"{c.QualifiedName} = {c.ParameterName}");

                var commandText =
                    $"SELECT {string.Join(", ", columnNames)} FROM {table.QualifiedName} WHERE {string.Join(" AND ", parameterNames)}";

                RetrieveCommandText[T] = commandText;
                return commandText;
            }
        }

        public static string GetCreateCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (CreateCommandText.ContainsKey(T))
                    return CreateCommandText[T];

                var table = GetTableMap(T);
                var columns = GetColumnMap(T).Where(c => c.IsGenerated == false);

                var columnNames = columns.Select(c => c.QualifiedName);
                var parameterNames = columns.Select(c => c.ParameterName);

                var commandText =
                    $"INSERT INTO {table.QualifiedName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", parameterNames)}) SELECT SCOPE_IDENTITY()";

                CreateCommandText[T] = commandText;
                return commandText;
            }
        }

        public static string GetCreateCommandText<T>() => GetCreateCommandText(typeof(T));

        public static string GetUpdateCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (UpdateCommandText.ContainsKey(T))
                    return UpdateCommandText[T];

                var table = GetTableMap(T);
                var setColumns = GetColumnMap(T).Where(c => c.IsGenerated == false && c.IsKeyColumn == false);
                var keyColumns = GetColumnMap(T).Where(c => c.IsKeyColumn);

                var setArgument = setColumns.Select(col => $"{col.QualifiedName} = {col.ParameterName}");
                var keyArgument = keyColumns.Select(col => $"{col.QualifiedName} = {col.ParameterName}");

                var commandText =
                    $"UPDATE {table.QualifiedName} SET {string.Join(", ", setArgument)} WHERE {string.Join(" AND ", keyArgument)}";

                UpdateCommandText[T] = commandText;
                return commandText;
            }
        }

        public static string GetUpdateCommandText<T>() => GetUpdateCommandText(typeof(T));

        public static string GetDeleteCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (DeleteCommandText.ContainsKey(T))
                    return DeleteCommandText[T];

                var table = GetTableMap(T);
                var keyColumns = GetColumnMap(T).Where(c => c.IsKeyColumn);
                var keyArgument = keyColumns.Select(col => $"{col.QualifiedName} = {col.ParameterName}");

                var commandText =
                    $"DELETE FROM {table.QualifiedName} WHERE {string.Join(" AND ", keyArgument)}";

                DeleteCommandText[T] = commandText;
                return commandText;
            }
        }

        public static string GetDeleteCommandText<T>() => GetDeleteCommandText(typeof(T));
    }
}

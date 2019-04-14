namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public static partial class Database
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

        public static int SqlCommandTimeoutSeconds { get; set; } = 60 * 5;

        public static async Task<SqlConnection> OpenConnectionAsync(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public static SqlConnection OpenConnection(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static async Task<SqlConnection> OpenLocalConnectionAsync(string databaseName) =>
            await OpenConnectionAsync($"Data Source=.; Integrated Security=True; Initial Catalog={databaseName}; MultipleActiveResultSets=True;");

        public static SqlConnection OpenLocalConnection(string databaseName) =>
            OpenConnection($"Data Source=.; Integrated Security=True; Initial Catalog={databaseName}; MultipleActiveResultSets=True;");

        public static async Task<SqlConnection> OpenConnectionAsync(string host, string username, string password, string databaseName) =>
            await OpenConnectionAsync($"Data Source={host}; User ID={username}; Password={password}; Initial Catalog={databaseName}; MultipleActiveResultSets=True;");

        public static SqlConnection OpenConnection(string host, string username, string password, string databaseName) =>
            OpenConnection($"Data Source={host}; User ID={username}; Password={password}; Initial Catalog={databaseName}; MultipleActiveResultSets=True;");

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
    }
}

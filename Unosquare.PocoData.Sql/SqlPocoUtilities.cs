namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public static class SqlPocoUtilities
    {
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
            if (value != null && DbTypes.CanMap(value.GetType()))
                param.SqlDbType = DbTypes.Map(value.GetType());

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

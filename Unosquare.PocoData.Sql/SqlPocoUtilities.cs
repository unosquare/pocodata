namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public static class SqlPocoUtilities
    {
        // TODO: https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlcommand.prepare?view=netframework-4.7.2

        public static SqlParameter AddParameter(this SqlCommand command, string parameterName, object value, int size = -1)
        {
            var T = value.GetType();
            var param = command.CreateParameter();
            param.ParameterName = parameterName;
            param.Value = value ?? DBNull.Value;

            if (value != null && DbTypes.CanMap(T))
                param.SqlDbType = DbTypes.Map(T);

            if (size > 0 && T == typeof(string))
                param.Size = size;

            command.Parameters.Add(param);
            return param;
        }

        public static void AddParameters(this SqlCommand command, IEnumerable<ColumnMetadata> columns, object item)
        {
            foreach (var col in columns)
            {
                var isNew = false;
                var param = command.Parameters.Contains(col.ParameterName)
                    ? command.Parameters[col.ParameterName]
                    : null;

                if (param == null)
                {
                    isNew = true;
                    param = command.CreateParameter();
                    command.Parameters.Add(param);
                }
                
                var value = col.GetValue(item);
                param.Value = value ?? DBNull.Value;

                if (!isNew) continue;
                
                param.ParameterName = col.ParameterName;
                var T = col.PropertyNativeType;

                if (DbTypes.CanMap(T))
                    param.SqlDbType = DbTypes.Map(T);

                if (T == typeof(string) && col.Length > 0)
                    param.Size = col.Length;
            }
        }
    }
}

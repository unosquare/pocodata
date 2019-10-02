namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;

    /// <summary>
    /// Provides extension methods for database commands and their parameters.
    /// </summary>
    public static class SqlPocoUtilities
    {
        /// <summary>
        /// Adds a parameter to the given SQL command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="size">The size.</param>
        /// <returns>The SQL Parameter.</returns>
        public static SqlParameter AddParameter(this SqlCommand command, string parameterName, object value, int size = -1)
        {
            if (value == null || command == null)
                throw new ArgumentNullException(string.Empty);

            var valueType = value.GetType();
            var param = command.CreateParameter();
            param.ParameterName = parameterName;
            param.Value = value;

            if (DbTypes.CanMap(valueType))
                param.SqlDbType = DbTypes.Map(valueType);

            if (size > 0 && valueType == typeof(string))
                param.Size = size;

            command.Parameters.Add(param);
            return param;
        }

        /// <summary>
        /// Adds or updates the parameters for the specified command using column mappings.
        /// </summary>
        /// <param name="command">The command to add or change parameters from.</param>
        /// <param name="columns">The column metadata.</param>
        /// <param name="item">The table-mapped object containing the values to inject as parameters.</param>
        public static void AddOrUpdateParameters(this SqlCommand command, IEnumerable<ColumnMetadata> columns, object item)
        {
            if (columns == null || command == null)
                throw new ArgumentNullException(string.Empty);

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
                var nativeType = col.NativeType;

                if (DbTypes.CanMap(nativeType))
                    param.SqlDbType = DbTypes.Map(nativeType);

                if (nativeType == typeof(string) && col.StringLength > 0)
                    param.Size = col.StringLength;
            }
        }

        /// <summary>
        /// Provides a text-based representation of the command and its parameters.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="outputToConsole">if set to <c>true</c> it will output to the console.</param>
        /// <returns>The string representation of the command and its parameters.</returns>
        public static string DebugCommand(this IDbCommand command, bool outputToConsole = true)
        {
            if (command == null)
                throw new ArgumentNullException(string.Empty);

            var builder = new StringBuilder();
            builder.AppendLine($"[{command.GetType()}]  {command.CommandText}");
            foreach (var parameter in command.Parameters)
            {
                if (parameter is SqlParameter sp)
                {
                    builder.AppendLine($"    {$"{sp.SqlDbType,-10} {(sp.Size <= 0 ? string.Empty : $"({sp.Size})")}",-20} {sp.ParameterName,-20} = {(sp.Value == DBNull.Value ? nameof(DBNull) : sp.Value)}");
                }
                else
                {
                    var p = parameter as IDbDataParameter;
                    builder.AppendLine($"    {$"{p.DbType,-10} {(p.Size <= 0 ? string.Empty : $"({p.Size})")}",-20} {p.ParameterName,-20} = {(p.Value == DBNull.Value ? nameof(DBNull) : p.Value)}");
                }
            }

            var result = builder.ToString().TrimEnd();
            if (outputToConsole)
                Console.WriteLine(result);

            return result;
        }
    }
}

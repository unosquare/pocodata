namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public static partial class Database
    {
        public static T ReadValue<T>(this SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            try
            {
                return reader.IsDBNull(ordinal)
                    ? default
                    : reader.GetFieldValue<T>(ordinal);
            }
            catch (InvalidCastException icex)
            {
                throw new InvalidCastException($"Unable to cast value of '{columnName}' to {typeof(T).Name}.", icex);
            }
        }

        public static object ReadObject(this SqlDataReader reader, object result)
        {
            var T = result.GetType();
            var map = GetColumnMap(T);
            var fieldNames = new List<string>(reader.FieldCount);
            for (var i = 0; i < reader.FieldCount; i++)
                fieldNames.Add(reader.GetName(i));

            foreach (var property in map)
            {
                var ordinal = fieldNames.IndexOf(property.ColumnName);
                if (ordinal < 0) continue;

                if (reader.IsDBNull(ordinal))
                {
                    property.SetValue(result, property.GetDefault());
                    continue;
                }

                var fieldValue = reader.GetValue(ordinal);
                try
                {
                    property.SetValue(result, fieldValue);
                }
                catch
                {
                    try
                    {
                        var propertyValue = Convert.ChangeType(fieldValue, property.PropertyNativeType);
                        property.SetValue(result, propertyValue);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidCastException(
                            $"Unable to convert '{fieldValue.GetType().Name}' to '{property.PropertyNativeType.Name}' for column '{property.ColumnName}'",
                            ex);
                    }
                }
            }

            return result;
        }

        public static T ReadObject<T>(this SqlDataReader reader, T result)
            where T : class
        {
            return reader.ReadObject((object)result) as T;
        }

        public static T ReadObject<T>(this SqlDataReader reader)
            where T : class, new()
        {
            var result = new T();
            return reader.ReadObject<T>(result);
        }
    }
}

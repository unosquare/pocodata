namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;

    internal sealed class SqlPocoReader : IPocoReader
    {
        internal SqlPocoReader()
        {
            // placeholder
        }

        private PocoSchema Schema => PocoSchema.Instance;

        public T ReadValue<T>(DbDataReader reader, string columnName)
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

        public object ReadObject(DbDataReader reader, object result)
        {
            var T = result.GetType();
            var map = Schema.Columns(T);
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

        public T ReadObject<T>(DbDataReader reader, T result)
            where T : class
        {
            return ReadObject(reader, (object)result) as T;
        }

        public T ReadObject<T>(DbDataReader reader)
            where T : class, new()
        {
            var result = new T();
            return ReadObject<T>(reader, result);
        }
    }
}

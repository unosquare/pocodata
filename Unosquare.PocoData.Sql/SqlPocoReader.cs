namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;

    internal sealed class SqlPocoReader : IPocoReader
    {
        internal SqlPocoReader()
        {
            // placeholder
        }

        private PocoSchema Schema => PocoSchema.Instance;

        public object ReadObject(IDataReader reader, object result)
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
                        var propertyValue = Convert.ChangeType(fieldValue, property.NativeType);
                        property.SetValue(result, propertyValue);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidCastException(
                            $"Unable to convert '{fieldValue.GetType().Name}' to '{property.NativeType.Name}' for column '{property.ColumnName}'",
                            ex);
                    }
                }
            }

            return result;
        }

        public T ReadObject<T>(IDataReader reader, T result)
            where T : class
        {
            return ReadObject(reader, (object)result) as T;
        }

        public T ReadObject<T>(IDataReader reader)
            where T : class, new()
        {
            var result = new T();
            return ReadObject<T>(reader, result);
        }
    }
}

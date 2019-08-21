namespace Unosquare.PocoData
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;

    /// <summary>
    /// Provides helper methods to read row data into POCOs.
    /// </summary>
    public sealed class PocoReader
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="PocoReader"/> class from being created.
        /// </summary>
        private PocoReader()
        {
            Schema = PocoSchema.Instance;
        }

        /// <summary>
        /// Gets the singleton instance of this class.
        /// </summary>
        public static PocoReader Instance { get; } = new PocoReader();

        /// <summary>
        /// Gets object that stores table and column mappings.
        /// </summary>
        private PocoSchema Schema { get; }

        /// <summary>
        /// Reads data reader data into the corresponding table-mapped object.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="item">The table-mapped object.</param>
        /// <returns>The resulting table-mapped object.</returns>
        /// <exception cref="InvalidCastException">Unable to convert '{fieldValue.GetType().Name}' to '{property.NativeType.Name}' for column '{property.ColumnName}.</exception>
        public object ReadObject(IDataReader reader, object item)
        {
            if (reader == null)
            {
                throw new ArgumentException(string.Empty);
            }

            var itemType = item?.GetType();
            var map = Schema.Columns(itemType);
            var fieldNames = new List<string>(reader.FieldCount);
            for (var i = 0; i < reader.FieldCount; i++)
                fieldNames.Add(reader.GetName(i));

            foreach (var property in map)
            {
                var ordinal = fieldNames.IndexOf(property.ColumnName);
                if (ordinal < 0) continue;

                if (reader.IsDBNull(ordinal))
                {
                    property.SetValue(item, property.GetDefault());
                    continue;
                }

                var fieldValue = reader.GetValue(ordinal);
                try
                {
                    property.SetValue(item, fieldValue);
                }
                catch
                {
                    try
                    {
                        var propertyValue = Convert.ChangeType(fieldValue, property.NativeType, CultureInfo.InvariantCulture);
                        property.SetValue(item, propertyValue);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidCastException(
                            $"Unable to convert '{fieldValue.GetType().Name}' to '{property.NativeType.Name}' for column '{property.ColumnName}'",
                            ex);
                    }
                }
            }

            return item;
        }

        /// <summary>
        /// Reads data reader data into the corresponding table-mapped object.
        /// </summary>
        /// <typeparam name="T">The table-mapped type.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="result">The the object to read into.</param>
        /// <returns>The object that was read.</returns>
        public T ReadObject<T>(IDataReader reader, T result)
            where T : class
        {
            return ReadObject(reader, (object)result) as T;
        }

        /// <summary>
        /// Reads data reader data into the corresponding table-mapped object.
        /// </summary>
        /// <typeparam name="T">The table-mapped type</typeparam>
        /// <param name="reader">The reader.</param>
        /// <returns>The object that was read.</returns>
        public T ReadObject<T>(IDataReader reader)
            where T : class, new()
        {
            var result = new T();
            return ReadObject(reader, result);
        }
    }
}

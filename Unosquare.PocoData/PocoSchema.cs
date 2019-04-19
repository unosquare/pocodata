namespace Unosquare.PocoData
{
    using Annotations;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Storeas table and column mappings and their corresponding metadata.
    /// </summary>
    public sealed class PocoSchema
    {
        private readonly object SyncLock = new object();

        private readonly PropertyTypeCache TypeCache = new PropertyTypeCache();
        private readonly Dictionary<Type, IReadOnlyList<ColumnMetadata>> ColumnMaps = new Dictionary<Type, IReadOnlyList<ColumnMetadata>>(32);
        private readonly Dictionary<Type, TableAttribute> TableMaps = new Dictionary<Type, TableAttribute>(32);

        /// <summary>
        /// Prevents a default instance of the <see cref="PocoSchema"/> class from being created.
        /// </summary>
        private PocoSchema()
        {
            // placeholder
        }

        /// <summary>
        /// Gets the singleton instance of this class.
        /// </summary>
        public static PocoSchema Instance { get; } = new PocoSchema();

        /// <summary>
        /// Gets a list of the standard value types.
        /// </summary>
        public static IReadOnlyList<Type> StandardValueTypes { get; } = new List<Type>()
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

        /// <summary>
        /// Provides column metadata for the specified table-mapped type.
        /// </summary>
        /// <param name="mappedType">The table-mapped type.</param>
        /// <returns>A list with column metadata.</returns>
        public IReadOnlyList<ColumnMetadata> Columns(Type mappedType)
        {
            lock (SyncLock)
            {
                if (ColumnMaps.ContainsKey(mappedType))
                    return ColumnMaps[mappedType];

                var properties = TypeCache.RetrieveAllProperties(mappedType).ToArray();
                var result = new List<ColumnMetadata>(properties.Length);
                foreach (var property in properties)
                {
                    // We don't want properties that are not read/write
                    if (!property.CanRead || !property.CanWrite)
                        continue;

                    // we don't want properties that can't be mapped
                    var propType = property.PropertyType;
                    var propTypeNullable = Nullable.GetUnderlyingType(propType);
                    propType = propTypeNullable ?? propType;

                    if (propType != typeof(string) && !propType.IsEnum && !StandardValueTypes.Contains(propType))
                        continue;

                    var columnName = property.Name;
                    var isKey = false;
                    var isNullable = propTypeNullable != null || propType == typeof(string);
                    var ignore = false;
                    var isGenerated = false;
                    var length = 255;

                    // extract column properties from attributes
                    var attribs = property.GetCustomAttributes(true).ToArray();
                    foreach (var attrib in attribs)
                    {
                        if (attrib is ColumnAttribute columnAttrib)
                            columnName = columnAttrib.Name;

                        if (attrib is KeyAttribute liteKey)
                        {
                            isKey = true;
                            isGenerated = liteKey.IsGenerated;
                        }

                        if (attrib is StringLengthAttribute liteLen)
                            length = liteLen.Length;

                        if (attrib is NotMappedAttribute)
                            ignore = true;

                        if (attrib is RequiredAttribute lineNotNull)
                        {
                            if (propType == typeof(string))
                                isNullable = false;
                        }
                    }

                    if (ignore)
                        continue;

                    result.Add(new ColumnMetadata(property, columnName, length, isNullable, isKey, isGenerated));
                }

                ColumnMaps[mappedType] = result;
                return result;
            }
        }

        /// <summary>
        /// Provides column metadata for the specified table-mapped type.
        /// </summary>
        /// <typeparam name="T">The table-mapped type.</typeparam>
        /// <returns>A list with column metadata.</returns>
        public IReadOnlyList<ColumnMetadata> Columns<T>()
            where T : class => Columns(typeof(T));

        /// <summary>
        /// Provides table metadata for the specified table-mapped type.
        /// </summary>
        /// <param name="mappedType">The table-mapped type.</param>
        /// <returns>The table attributes applied to the type.</returns>
        public TableAttribute Table(Type mappedType)
        {
            lock (SyncLock)
            {
                if (TableMaps.ContainsKey(mappedType))
                    return TableMaps[mappedType];

                var table = mappedType.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute
                    ?? new TableAttribute(mappedType.Name);

                TableMaps[mappedType] = table;
                return table;
            }
        }

        /// <summary>
        /// Provides table metadata for the specified table-mapped type.
        /// </summary>
        /// <typeparam name="T">The table-mapped type.</typeparam>
        /// <returns>The table attributes applied to the type.</returns>
        public TableAttribute Table<T>()
            where T : class => Table(typeof(T));

        /// <summary>
        /// Validates that the specified table-mapped type has appropriate attributes applied so it can be stored in its corresponding table.
        /// </summary>
        /// <param name="mappedType">The table-mapped type.</param>
        public void Validate(Type mappedType)
        {
            var columns = Columns(mappedType);
            if (columns.Count(c => c.IsKeyColumn) <= 0)
                throw new NotSupportedException("At leat a key column must be defined");

            if (columns.Count(c => c.IsKeyGenerated) > 1)
                throw new NotSupportedException("Only a single generated column is suppoted in the schema");

            if (columns.Count(c => c.IsKeyGenerated && !c.IsKeyColumn) > 0)
                throw new NotSupportedException("Only agenerated columns must participate in the primary key set");

            if (columns.Count(c => c.IsNullable && c.IsKeyColumn) > 0)
                throw new NotSupportedException("Key columns must not be nullable");

            // TODO: add more validation rules
        }

        /// <summary>
        /// Validates that the specified table-mapped type has appropriate attributes applied so it can be stored in its corresponding table.
        /// </summary>
        /// <typeparam name="T">The table-mapped type.</typeparam>
        public void Validate<T>()
            where T : class => Validate(typeof(T));

        /// <summary>
        /// Gets the column metadata.
        /// </summary>
        /// <param name="mappedType">The table-mapped type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The column metadata.</returns>
        internal ColumnMetadata GetColumnMetadata(Type mappedType, string propertyName)
        {
            lock (SyncLock)
            {
                var currentList = Columns(mappedType);
                var currentItemIndex = -1;

                for (var i = 0; i < currentList.Count; i++)
                {
                    if (currentList[i].Property.Name != propertyName)
                        continue;

                    currentItemIndex = i;
                    break;
                }

                return currentItemIndex >= 0 ? currentList[currentItemIndex] : null;
            }
        }
    }
}

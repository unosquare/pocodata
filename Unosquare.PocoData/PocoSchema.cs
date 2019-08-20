namespace Unosquare.PocoData
{
    using Annotations;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

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
            if(mappedType == null)
            {
                throw new ArgumentException(string.Empty);
            }

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

                    PopulateProperty(property, result);
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
            if(mappedType == null)
            {
                throw new ArgumentException(string.Empty);
            }

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
            if (!columns.Any(c => c.IsKeyColumn))
                throw new NotSupportedException("At least a key column must be defined");

            if (columns.Count(c => c.IsKeyGenerated) > 1)
                throw new NotSupportedException("Only a single generated column is supported in the schema");

            if (columns.Any(c => c.IsKeyGenerated && !c.IsKeyColumn))
                throw new NotSupportedException("Only generated columns must participate in the primary key set");

            if (columns.Any(c => c.IsNullable && c.IsKeyColumn))
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

        private static void PopulateProperty(PropertyInfo property, List<ColumnMetadata> result)
        {
            // we don't want properties that can't be mapped
            var propType = property.PropertyType;
            var propTypeNullable = Nullable.GetUnderlyingType(propType);
            propType = propTypeNullable ?? propType;

            if (propType != typeof(string) && !propType.IsEnum && !StandardValueTypes.Contains(propType))
                return;

            var columnName = property.Name;
            var isKey = false;
            var isNullable = propTypeNullable != null || propType == typeof(string);
            var isGenerated = false;
            var length = 255;

            // extract column properties from attributes
            var attribs = property.GetCustomAttributes(true).ToArray();

            foreach (var attrib in attribs)
            {
                switch (attrib)
                {
                    case ColumnAttribute columnAttrib:
                        columnName = columnAttrib.Name;
                        break;
                    case KeyAttribute liteKey:
                        isKey = true;
                        isGenerated = liteKey.IsGenerated;
                        break;
                    case StringLengthAttribute liteLen:
                        length = liteLen.Length;
                        break;
                    case NotMappedAttribute _:
                        return;
                    case RequiredAttribute _:
                        {
                            if (propType == typeof(string))
                                isNullable = false;
                            break;
                        }
                }
            }

            result.Add(new ColumnMetadata(property, columnName, length, isNullable, isKey, isGenerated));
        }
    }
}

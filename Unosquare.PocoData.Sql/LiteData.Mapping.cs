namespace Unosquare.PocoData.Sql
{
    using Annotations;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static partial class LiteData
    {
        private static readonly object SyncLock = new object();
        private static readonly PropertyTypeCache TypeCache = new PropertyTypeCache();
        private static readonly Dictionary<Type, List<ColumnMetadata>> ColumnMaps = new Dictionary<Type, List<ColumnMetadata>>(32);
        private static readonly Dictionary<Type, TableAttribute> TableMaps = new Dictionary<Type, TableAttribute>(32);

        private static IReadOnlyList<ColumnMetadata> GetColumnMap(Type T)
        {
            lock (SyncLock)
            {
                if (ColumnMaps.ContainsKey(T))
                    return ColumnMaps[T];

                var properties = TypeCache.RetrieveAllProperties(T).ToArray();
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

                    if (propType != typeof(string) && !propType.IsEnum && !PersistableValueTypes.Contains(propType))
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

                ColumnMaps[T] = result;
                return result;
            }
        }

        private static IReadOnlyList<ColumnMetadata> GetColumnMap<T>() => GetColumnMap(typeof(T));

        private static TableAttribute GetTableMap(Type T)
        {
            lock (SyncLock)
            {
                if (TableMaps.ContainsKey(T))
                    return TableMaps[T];

                var table = T.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute
                    ?? new TableAttribute(T.Name);

                TableMaps[T] = table;
                return table;
            }
        }

        private static TableAttribute GetTableMap<T>() => GetTableMap(typeof(T));
    }
}

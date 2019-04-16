namespace Unosquare.PocoData
{
    using Annotations;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PocoSchema
    {
        private readonly object SyncLock = new object();

        private readonly PropertyTypeCache TypeCache = new PropertyTypeCache();
        private readonly Dictionary<Type, IReadOnlyList<ColumnMetadata>> ColumnMaps = new Dictionary<Type, IReadOnlyList<ColumnMetadata>>(32);
        private readonly Dictionary<Type, TableAttribute> TableMaps = new Dictionary<Type, TableAttribute>(32);

        private PocoSchema()
        {
            // placeholder
        }

        public static PocoSchema Instance { get; } = new PocoSchema();

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

        public IReadOnlyList<ColumnMetadata> Columns(Type T)
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

                ColumnMaps[T] = result;
                return result;
            }
        }

        public IReadOnlyList<ColumnMetadata> Columns<T>() where T : class => Columns(typeof(T));

        public TableAttribute Table(Type T)
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

        public TableAttribute Table<T>() where T : class => Table(typeof(T));

        public void Validate(Type T)
        {
            var columns = Columns(T);
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

        public void Validate<T>() where T : class => Validate(typeof(T));
    }
}

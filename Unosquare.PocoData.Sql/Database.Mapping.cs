namespace Unosquare.PocoData.Sql
{
    using Annotations;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static partial class Database
    {
        private static readonly object SyncLock = new object();

        private static readonly PropertyTypeCache TypeCache = new PropertyTypeCache();
        private static readonly Dictionary<Type, IReadOnlyList<ColumnMetadata>> ColumnMaps = new Dictionary<Type, IReadOnlyList<ColumnMetadata>>(32);
        private static readonly Dictionary<Type, TableAttribute> TableMaps = new Dictionary<Type, TableAttribute>(32);

        private static readonly Dictionary<Type, string> CreateCommandText = new Dictionary<Type, string>(32);
        private static readonly Dictionary<Type, string> UpdateCommandText = new Dictionary<Type, string>(32);
        private static readonly Dictionary<Type, string> DeleteCommandText = new Dictionary<Type, string>(32);
        private static readonly Dictionary<Type, string> RetrieveCommandText = new Dictionary<Type, string>(32);

        public static string GetRetrieveCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (RetrieveCommandText.ContainsKey(T))
                    return RetrieveCommandText[T];

                var table = GetTableMap(T);
                var columns = GetColumnMap(T);

                var columnNames = columns.Select(c => c.QualifiedName);
                var parameterNames = columns.Where(c => c.IsKeyColumn).Select(c => $"{c.QualifiedName} = {c.ParameterName}");

                var commandText =
                    $"SELECT {string.Join(", ", columnNames)} FROM {table.QualifiedName} WHERE {string.Join(" AND ", parameterNames)}";

                RetrieveCommandText[T] = commandText;
                return commandText;
            }
        }

        public static string GetCreateCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (CreateCommandText.ContainsKey(T))
                    return CreateCommandText[T];

                var table = GetTableMap(T);
                var columns = GetColumnMap(T).Where(c => c.IsGenerated == false);

                var columnNames = columns.Select(c => c.QualifiedName);
                var parameterNames = columns.Select(c => c.ParameterName);

                var commandText =
                    $"INSERT INTO {table.QualifiedName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", parameterNames)}) SELECT SCOPE_IDENTITY()";

                CreateCommandText[T] = commandText;
                return commandText;
            }
        }

        public static string GetCreateCommandText<T>() => GetCreateCommandText(typeof(T));

        public static string GetUpdateCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (UpdateCommandText.ContainsKey(T))
                    return UpdateCommandText[T];

                var table = GetTableMap(T);
                var setColumns = GetColumnMap(T).Where(c => c.IsGenerated == false && c.IsKeyColumn == false);
                var keyColumns = GetColumnMap(T).Where(c => c.IsKeyColumn);

                var setArgument = setColumns.Select(col => $"{col.QualifiedName} = {col.ParameterName}");
                var keyArgument = keyColumns.Select(col => $"{col.QualifiedName} = {col.ParameterName}");

                var commandText =
                    $"UPDATE {table.QualifiedName} SET {string.Join(", ", setArgument)} WHERE {string.Join(" AND ", keyArgument)}";

                UpdateCommandText[T] = commandText;
                return commandText;
            }
        }

        public static string GetUpdateCommandText<T>() => GetUpdateCommandText(typeof(T));

        public static string GetDeleteCommandText(Type T)
        {
            lock (SyncLock)
            {
                if (DeleteCommandText.ContainsKey(T))
                    return DeleteCommandText[T];

                var table = GetTableMap(T);
                var keyColumns = GetColumnMap(T).Where(c => c.IsKeyColumn);
                var keyArgument = keyColumns.Select(col => $"{col.QualifiedName} = {col.ParameterName}");

                var commandText =
                    $"DELETE FROM {table.QualifiedName} WHERE {string.Join(" AND ", keyArgument)}";

                DeleteCommandText[T] = commandText;
                return commandText;
            }
        }

        public static string GetDeleteCommandText<T>() => GetDeleteCommandText(typeof(T));

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
    }
}

namespace Unosquare.PocoData.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// Provides type-mapping facilities between CLR and SQL Server types
    /// </summary>
    internal static class DbTypes
    {
        private static readonly IReadOnlyDictionary<Type, SqlDbType> SqlTypes = new Dictionary<Type, SqlDbType>()
        {
            { typeof(string), SqlDbType.NVarChar },
            { typeof(sbyte), SqlDbType.SmallInt },
            { typeof(short), SqlDbType.SmallInt },
            { typeof(int), SqlDbType.Int },
            { typeof(long), SqlDbType.BigInt },
            { typeof(byte), SqlDbType.TinyInt },
            { typeof(ushort), SqlDbType.Int },
            { typeof(uint), SqlDbType.BigInt },
            { typeof(ulong), SqlDbType.Float },
            { typeof(char), SqlDbType.NChar },
            { typeof(float), SqlDbType.Float },
            { typeof(double), SqlDbType.Float },
            { typeof(decimal), SqlDbType.Money },
            { typeof(bool), SqlDbType.Bit },
            { typeof(Guid), SqlDbType.UniqueIdentifier },
            { typeof(DateTime), SqlDbType.DateTime },
        };

        private static readonly IReadOnlyDictionary<SqlDbType, Type> ClrTypes = new Dictionary<SqlDbType, Type>()
        {
            { SqlDbType.NVarChar, typeof(string) },
            { SqlDbType.SmallInt, typeof(short) },
            { SqlDbType.Int, typeof(int) },
            { SqlDbType.BigInt, typeof(long) },
            { SqlDbType.TinyInt, typeof(byte) },
            { SqlDbType.NChar, typeof(char) },
            { SqlDbType.Float, typeof(double) },
            { SqlDbType.Money, typeof(decimal) },
            { SqlDbType.Bit, typeof(bool) },
            { SqlDbType.UniqueIdentifier, typeof(Guid) },
            { SqlDbType.DateTime, typeof(DateTime) },
        };

        /// <summary>
        /// Maps the specified CLR type to a SqlDbType.
        /// </summary>
        /// <param name="clrType">The CLR type.</param>
        /// <returns>The SqlDbType</returns>
        public static SqlDbType Map(Type clrType) => SqlTypes[clrType];

        /// <summary>
        /// Maps the specified SqlDbType to a CLR type.
        /// </summary>
        /// <param name="sqlType">The SqlDbType.</param>
        /// <returns>The CLR type</returns>
        public static Type Map(SqlDbType sqlType) => ClrTypes[sqlType];

        /// <summary>
        /// Determines whether the CLR type can be mapped to a SqlDbType.
        /// </summary>
        /// <param name="clrType">The CLR Type.</param>
        /// <returns>
        ///   <c>true</c> if the types can be mapped.
        /// </returns>
        public static bool CanMap(Type clrType) => SqlTypes.ContainsKey(clrType);

        /// <summary>
        /// Determines whether the SqlDbType can be mapped to a CLR type.
        /// </summary>
        /// <param name="sqlType">The SqlDbType.</param>
        /// <returns>
        ///   <c>true</c> if the types can be mapped.
        /// </returns>
        public static bool CanMap(SqlDbType sqlType) => ClrTypes.ContainsKey(sqlType);
    }
}

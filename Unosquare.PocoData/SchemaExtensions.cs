﻿namespace Unosquare.PocoData
{
    using Annotations;
    using System;

    /// <summary>
    /// Provides extension methods to configure schema mappings with a fluent API and without the need for attibutes.
    /// Examples:
    /// typeof(Employee).Table().ToTable(nameof(Employee));
    /// typeof(Employee).Column(nameof(Employee.Children)).IsNullable();.
    /// </summary>
    public static class SchemaExtensions
    {
        /// <summary>
        /// Gets the table metadata for the given type.
        /// </summary>
        /// <param name="mappedType">The table-mapped type.</param>
        /// <returns>The table metadata.</returns>
        public static TableAttribute Table(this Type mappedType) => PocoSchema.Instance.Table(mappedType);

        /// <summary>
        /// Maps a type to the specified table name and schema.
        /// </summary>
        /// <param name="table">The table metadata.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <returns>The table attribute.</returns>
        public static TableAttribute ToTable(this TableAttribute table, string tableName, string schemaName)
        {
            if (table == null)
            {
                throw new ArgumentException(string.Empty);
            }

            table.Name = tableName;
            table.Schema = schemaName;
            return table;
        }

        /// <summary>
        /// Maps a type to the specified table name and schema.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>The table attribute.</returns>
        public static TableAttribute ToTable(this TableAttribute table, string tableName) => table.ToTable(tableName, null);

        /// <summary>
        /// Gets the complumn metadata for the given type and property name.
        /// </summary>
        /// <param name="mappedType">The table-mapped type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The column metadata.</returns>
        public static ColumnMetadata Column(this Type mappedType, string propertyName) =>
            PocoSchema.Instance.GetColumnMetadata(mappedType, propertyName);

        /// <summary>
        /// Updates column metadata with a mapping to a column name.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="name">The name of the column to map to.</param>
        /// <returns>The column metadata.</returns>
        public static ColumnMetadata HasColumnName(this ColumnMetadata column, string name)
        {
            if(column == null)
            {
                throw new ArgumentNullException(string.Empty);
            }

            column.ColumnName = name;
            return column;
        }

        /// <summary>
        /// Updates column metadata letting the mapper know the property represents a key column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>The column metadata.</returns>
        public static ColumnMetadata IsKeyColumn(this ColumnMetadata column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(string.Empty);
            }

            column.IsKeyColumn = true;
            column.IsKeyGenerated = false;
            return column;
        }

        /// <summary>
        /// Updates column metadata letting the mapper know the property represents a key column whose values are generated by the data store.
        /// </summary>>
        /// <param name="column">The column.</param>
        /// <returns>The column metadata.</returns>
        public static ColumnMetadata IsGeneratedKeyColumn(this ColumnMetadata column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(string.Empty);
            }

            column.IsKeyColumn = true;
            column.IsKeyGenerated = true;
            return column;
        }

        /// <summary>
        /// Updates column metadata letting the mapper know the property can contain null values.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>The column metadata.</returns>
        public static ColumnMetadata IsNullable(this ColumnMetadata column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(string.Empty);
            }

            column.IsNullable = true;
            return column;
        }

        /// <summary>
        /// Updates column metadata letting the mapper know the maximum string length of the property.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="stringLength">Length of the string.</param>
        /// <returns>The column metadata.</returns>
        public static ColumnMetadata HasStringLength(this ColumnMetadata column, int stringLength)
        {
            if (column == null)
            {
                throw new ArgumentNullException(string.Empty);
            }

            column.StringLength = stringLength;
            return column;
        }

        /// <summary>
        /// Updates column metadata letting the mapper know it should ignroe the property.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>The column metadata.</returns>
        public static ColumnMetadata NotMapped(this ColumnMetadata column)
        {
            throw new NotImplementedException();
        }
    }
}

namespace Unosquare.PocoData.Annotations
{
    using System;

    /// <summary>
    /// Maps a property to a column name
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        public ColumnAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        public string Name { get; }
    }
}

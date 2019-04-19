namespace Unosquare.PocoData.Annotations
{
    using System;

    /// <summary>
    /// When applied to a class, it specifies the table it maps to.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TableAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the table.</param>
        /// <param name="schema">The name of the schema.</param>
        public TableAttribute(string name, string schema)
        {
            Name = name;
            Schema = schema;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableAttribute"/> class with a default empty schema.
        /// </summary>
        /// <param name="name">The name.</param>
        public TableAttribute(string name)
            : this(name, string.Empty)
        {
            // placeholder
        }

        /// <summary>
        /// Gets the name of the table the class maps to.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the schema of the table this class maps to.
        /// </summary>
        public string Schema { get; internal set; }

        /// <summary>
        /// Gets the qualified name of the table including schema.
        /// </summary>
        public string QualifiedName => string.IsNullOrWhiteSpace(Schema) ? $"[{Name}]" : $"[{Schema}].[{Name}]";
    }
}

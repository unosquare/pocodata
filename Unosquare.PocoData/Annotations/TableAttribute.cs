namespace Unosquare.PocoData.Annotations
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TableAttribute : Attribute
    {
        public TableAttribute(string name, string schema)
        {
            Name = name;
            Schema = schema;
        }

        public TableAttribute(string name)
            : this (name, "dbo")
        {
            // placeholder
        }

        public string Name { get; }

        public string Schema { get; }

        public string QualifiedName => $"[{Schema}].[{Name}]";
    }
}

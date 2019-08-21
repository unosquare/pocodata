namespace Unosquare.PocoData.Annotations
{
    using System;

    /// <summary>
    /// When applied to a property it ignores it for mapping it to a column.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NotMappedAttribute : Attribute
    {
        // placeholder
    }
}

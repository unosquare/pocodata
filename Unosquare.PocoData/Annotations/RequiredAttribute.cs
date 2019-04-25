namespace Unosquare.PocoData.Annotations
{
    using System;

    /// <summary>
    /// When applied to a property, it tells the underlying table the value cannot be null.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RequiredAttribute : Attribute
    {
        // placeholder
    }
}

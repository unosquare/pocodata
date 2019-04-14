namespace Unosquare.PocoData.Annotations
{
    using System;

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class NotMappedAttribute : Attribute
    {
    }
}

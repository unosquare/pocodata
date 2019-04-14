namespace Unosquare.PocoData.Annotations
{
    using System;

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class KeyAttribute : Attribute
    {
        public KeyAttribute(bool isGenerated)
        {
            IsGenerated = isGenerated;
        }

        public bool IsGenerated { get; }
    }
}

namespace Unosquare.PocoData.Annotations
{
    using System;

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class StringLengthAttribute : Attribute
    {
        public StringLengthAttribute(int length)
        {
            Length = length;
        }

        public int Length { get; }
    }
}

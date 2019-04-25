namespace Unosquare.PocoData.Annotations
{
    using System;

    /// <summary>
    /// When applied to a string typed property, it specifies the length of the string to store in the column definition.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public class StringLengthAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringLengthAttribute"/> class.
        /// </summary>
        /// <param name="length">The string length.</param>
        public StringLengthAttribute(int length)
        {
            Length = length;
        }

        /// <summary>
        /// Gets the specified string length.
        /// </summary>
        public int Length { get; }
    }
}

﻿namespace Unosquare.PocoData.Annotations
{
    using System;

    /// <summary>
    /// Specifies that that the property this attribute is applied to is a key column
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class KeyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyAttribute"/> class.
        /// </summary>
        /// <param name="isGenerated">if set to <c>true</c> the value is generated by the database.</param>
        public KeyAttribute(bool isGenerated)
        {
            IsGenerated = isGenerated;
        }

        /// <summary>
        /// Gets a value indicating whether the column value is generated by the database.
        /// </summary>
        public bool IsGenerated { get; }
    }
}

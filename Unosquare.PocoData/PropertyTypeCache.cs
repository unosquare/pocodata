namespace Unosquare.PocoData
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// A very simple class that extracts and stores property info for types.
    /// </summary>
    internal sealed class PropertyTypeCache
    {
        private readonly object SyncLock = new object();
        private readonly Dictionary<Type, IReadOnlyList<PropertyInfo>> Properties = new Dictionary<Type, IReadOnlyList<PropertyInfo>>(64);

        /// <summary>
        /// Retrieves all properties of the given type.
        /// </summary>
        /// <param name="T">The type to retrieve properties from.</param>
        /// <returns>A list of property information objects.</returns>
        public IReadOnlyList<PropertyInfo> RetrieveAllProperties(Type T)
        {
            lock (SyncLock)
            {
                if (Properties.ContainsKey(T)) return Properties[T];

                var p = T.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Properties[T] = p;
                return p;
            }
        }
    }
}

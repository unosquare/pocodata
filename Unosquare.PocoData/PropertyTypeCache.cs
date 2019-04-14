namespace Unosquare.PocoData
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public sealed class PropertyTypeCache
    {
        private readonly object SyncLock = new object();
        private readonly Dictionary<Type, IReadOnlyList<PropertyInfo>> Properties = new Dictionary<Type, IReadOnlyList<PropertyInfo>>(64);

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

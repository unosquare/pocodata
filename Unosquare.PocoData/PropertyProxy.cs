namespace Unosquare.PocoData
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a generic class to store getters and setters.
    /// </summary>
    /// <typeparam name="TClass">The type of the class.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <seealso cref="IPropertyProxy" />
    internal sealed class PropertyProxy<TClass, TProperty> : IPropertyProxy
        where TClass : class
    {
        private readonly Func<TClass, TProperty> Getter;
        private readonly Action<TClass, TProperty> Setter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyProxy{TClass, TProperty}"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        public PropertyProxy(PropertyInfo property)
        {
            var getterInfo = property.GetGetMethod(false);
            if (getterInfo != null)
                Getter = (Func<TClass, TProperty>)Delegate.CreateDelegate(typeof(Func<TClass, TProperty>), getterInfo);

            var setterInfo = property.GetSetMethod(false);
            if (setterInfo != null)
                Setter = (Action<TClass, TProperty>)Delegate.CreateDelegate(typeof(Action<TClass, TProperty>), setterInfo);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object IPropertyProxy.GetValue(object instance) =>
            Getter(instance as TClass);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IPropertyProxy.SetValue(object instance, object value) =>
            Setter(instance as TClass, (TProperty)value);
    }
}

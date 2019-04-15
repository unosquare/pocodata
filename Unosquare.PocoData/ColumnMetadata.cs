namespace Unosquare.PocoData
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public sealed class ColumnMetadata
    {
        private readonly IPropertyProxy Proxy;

        internal ColumnMetadata(PropertyInfo property, string columnName, int length, bool isNullable, bool isKey, bool isGenerated)
        {
            Property = property;
            Length = length;
            ColumnName = columnName;
            IsKeyColumn = isKey;
            IsGenerated = isGenerated;
            IsNullable = isNullable;
            PropertyNativeType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            if (PropertyNativeType.IsEnum)
                PropertyNativeType = PropertyNativeType.GetEnumUnderlyingType();

            Proxy = CreatePropertyProxy(property);
        }

        public PropertyInfo Property { get; }

        public string PropertyName => Property.Name;

        public Type PropertyNativeType { get; }

        public int Length { get; }

        public bool IsKeyColumn { get; }

        public bool IsGenerated { get; }

        public bool IsNullable { get; }

        public string ColumnName { get; }

        public string QualifiedName => $"[{ColumnName}]";

        public string ParameterName => $"@{PropertyName}";

        public object GetDefault() => GetDefault(Property.PropertyType);

        public object GetValue(object instance) => Proxy.GetValue(instance);

        public void SetValue(object instance, object value) => Proxy.SetValue(instance, value);

        private static object GetDefault(Type type) =>
            type.IsValueType ? Activator.CreateInstance(type) : null;

        private static IPropertyProxy CreatePropertyProxy(PropertyInfo propertyInfo)
        {
            var genericType = typeof(PropertyProxy<,>)
                .MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return Activator.CreateInstance(genericType, propertyInfo) as IPropertyProxy;
        }

        private interface IPropertyProxy
        {
            object GetValue(object instance);

            void SetValue(object instance, object value);
        }

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
}

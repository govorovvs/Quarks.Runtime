using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Quarks.Runtime
{
    /// <summary>
    /// Customizes the creation algorithm for a single object.
    /// </summary>
    /// <typeparam name="T">The type of object for which the algorithm should be customized.</typeparam>
    public class Builder<T> where T : class
    {
        private readonly T _instance;

        /// <summary>
        /// Initializes a new instance of <see cref="Builder{T}"/> class.
        /// </summary>
        public Builder()
        {
            _instance = CreateInstance();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Builder{T}"/> class with exsiting instance.
        /// </summary>
        /// <param name="instance">The instance</param>
        public Builder(T instance)
        {
            _instance = instance;
        }

        /// <summary>
        /// Registers that a writable property or field should be assigned a specific value.
        /// </summary>
        /// <typeparam name="TProperty">
        /// The type of the property of field.</typeparam>
        /// <param name="propertyPicker"> 
        /// An expression that identifies the property or field that will have 
        /// <paramref name="value" /> assigned.</param>
        /// <param name="value">The value to assign to the property or field identified by
        /// <paramref name="propertyPicker" />.</param>
        /// <returns></returns>
        public Builder<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, TProperty value)
        {
            if (_instance == null)
                return this;

            List<MemberInfo> members = new List<MemberInfo>();

            Expression exp = propertyPicker.Body;

            while (exp != null)
            {
                MemberExpression mi = exp as MemberExpression;

                if (mi != null)
                {
                    members.Add(mi.Member);
                    exp = mi.Expression;
                }
                else
                {
                    ParameterExpression pe = exp as ParameterExpression;
                    if (pe == null)
                        throw new InvalidOperationException("Only property and field can be populated via With");

                    break;
                }
            }

            if (members.Count == 0)
            {
                throw new NotSupportedException();
            }

            object targetObject = _instance;

            for (int i = members.Count - 1; i >= 1; i--)
            {
                PropertyInfo pi = members[i] as PropertyInfo;

                if (pi != null)
                {
                    targetObject = pi.GetValue(targetObject);
                }
                else
                {
                    FieldInfo fi = (FieldInfo)members[i];
                    targetObject = fi.GetValue(targetObject);
                }
            }

            {
                PropertyInfo pi = members[0] as PropertyInfo;

                if (pi != null)
                {
                    SetValue(targetObject, pi, value);
                }
                else
                {
                    FieldInfo fi = (FieldInfo)members[0];
                    SetValue(targetObject, fi, value);
                }
            }

            return this;
        }

        /// <summary>
        /// Creates an anonymous variable of <typeparamref name="T"/> type.
        /// </summary>
        /// <returns>Created instance.</returns>
        public T Create()
        {
            return _instance;
        }

        private void SetValue(object targetObject, PropertyInfo pi, object value)
        {
            pi = targetObject.GetType().GetTypeInfo().GetProperty(pi.Name) ?? pi;
            if (pi.DeclaringType != pi.PropertyType)
            {
                pi = pi.DeclaringType.GetTypeInfo().GetProperty(pi.Name);
            }

            pi.SetValue(targetObject, value);
        }

        private void SetValue(object targetObject, FieldInfo fi, object value)
        {
            fi = targetObject.GetType().GetTypeInfo().GetField(fi.Name) ?? fi;
            if (fi.DeclaringType != fi.FieldType)
            {
                fi = fi.DeclaringType.GetTypeInfo().GetField(fi.Name);
            }

            fi.SetValue(targetObject, value);
        }

        private T CreateInstance()
        {
            ConstructorInfo parameterlessConstructor =
                typeof(T).GetTypeInfo().DeclaredConstructors
                    .SingleOrDefault(constructor => constructor.GetParameters().Any() == false);
            if (parameterlessConstructor == null)
            {
                throw new MissingMethodException($"Type {typeof(T).Name} doesn't contain parameterless constructor");
            }

            return (T)parameterlessConstructor.Invoke(new object[0]);
        }
    }
}
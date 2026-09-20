using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DisplayMagician.UserAgent.Runtime
{
    /// <summary>
    /// Restores null collection, array, and string values in persisted display-library
    /// configuration objects so older profile files can be used safely.
    /// </summary>
    internal static class DisplayConfigurationNormalizer
    {
        public static void Normalize(ProfileItem profile)
        {
            var nvidiaDisplayConfig = profile.NVIDIADisplayConfig;
            NormalizeValue(ref nvidiaDisplayConfig);
            profile.NVIDIADisplayConfig = nvidiaDisplayConfig;

            var amdDisplayConfig = profile.AMDDisplayConfig;
            NormalizeValue(ref amdDisplayConfig);
            profile.AMDDisplayConfig = amdDisplayConfig;

            var intelDisplayConfig = profile.IntelDisplayConfig;
            NormalizeValue(ref intelDisplayConfig);
            profile.IntelDisplayConfig = intelDisplayConfig;

            var windowsDisplayConfig = profile.WindowsDisplayConfig;
            NormalizeValue(ref windowsDisplayConfig);
            profile.WindowsDisplayConfig = windowsDisplayConfig;
        }

        private static void NormalizeValue<T>(ref T value)
        {
            object boxedValue = value;
            NormalizeObject(boxedValue, new HashSet<object>());
            value = (T)boxedValue;
        }

        private static void NormalizeObject(object value, HashSet<object> visited)
        {
            if (value == null)
                return;

            Type type = value.GetType();
            if (value is string || type.IsPrimitive || type.IsEnum || type == typeof(decimal))
                return;

            if (!visited.Add(value))
                return;

            if (value is IDictionary dictionary)
            {
                foreach (object key in dictionary.Keys.Cast<object>().ToList())
                {
                    object item = dictionary[key];
                    if (item == null)
                        continue;

                    NormalizeObject(item, visited);
                    if (item.GetType().IsValueType)
                        dictionary[key] = item;
                }
                return;
            }

            if (value is IList list)
            {
                for (int index = 0; index < list.Count; index++)
                {
                    object item = list[index];
                    if (item == null)
                        continue;

                    NormalizeObject(item, visited);
                    if (item.GetType().IsValueType)
                        list[index] = item;
                }
                return;
            }

            if (value is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                {
                    if (item != null)
                        NormalizeObject(item, visited);
                }
                return;
            }

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object fieldValue = field.GetValue(value);
                if (fieldValue == null)
                {
                    SetEmptyValue(field.FieldType, emptyValue => field.SetValue(value, emptyValue));
                }
                else
                {
                    NormalizeObject(fieldValue, visited);
                    if (field.FieldType.IsValueType)
                        field.SetValue(value, fieldValue);
                }
            }

            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0))
            {
                object propertyValue;
                try
                {
                    propertyValue = property.GetValue(value);
                }
                catch (Exception)
                {
                    continue;
                }

                if (propertyValue == null)
                {
                    SetEmptyValue(property.PropertyType, emptyValue => property.SetValue(value, emptyValue));
                }
                else
                {
                    NormalizeObject(propertyValue, visited);
                    if (property.PropertyType.IsValueType)
                    {
                        try
                        {
                            property.SetValue(value, propertyValue);
                        }
                        catch (Exception)
                        {
                            // Some persisted types expose an inaccessible setter; leave that member unchanged.
                        }
                    }
                }
            }
        }

        private static void SetEmptyValue(Type type, Action<object> setValue)
        {
            object emptyValue = null;
            if (type == typeof(string))
            {
                emptyValue = String.Empty;
            }
            else if (type.IsArray && type.GetElementType() != null)
            {
                emptyValue = Array.CreateInstance(type.GetElementType(), 0);
            }
            else if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                Type[] typeArguments = type.GenericTypeArguments;
                if (genericType == typeof(List<>) || genericType == typeof(IList<>) || genericType == typeof(ICollection<>) || genericType == typeof(IEnumerable<>))
                    emptyValue = Activator.CreateInstance(typeof(List<>).MakeGenericType(typeArguments[0]));
                else if (genericType == typeof(HashSet<>) || genericType == typeof(ISet<>))
                    emptyValue = Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(typeArguments[0]));
                else if (genericType == typeof(Dictionary<,>) || genericType == typeof(IDictionary<,>))
                    emptyValue = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeArguments[0], typeArguments[1]));
            }
            else if (typeof(IList).IsAssignableFrom(type) || typeof(IDictionary).IsAssignableFrom(type))
            {
                try
                {
                    emptyValue = Activator.CreateInstance(type);
                }
                catch (Exception)
                {
                    // Interfaces and abstract collection types cannot be directly instantiated.
                }
            }

            if (emptyValue != null)
            {
                try
                {
                    setValue(emptyValue);
                }
                catch (Exception)
                {
                    // Some persisted types expose an inaccessible setter; leave that member unchanged.
                }
            }
        }
    }
}

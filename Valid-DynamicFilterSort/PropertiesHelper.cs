using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using valid.error.management.exceptions;

[assembly: InternalsVisibleTo("UnitTests")]

namespace Valid_DynamicFilterSort
{
    internal static class PropertiesHelper
    {
        /// <summary>
        /// Returns case sensitive Property Name from string separated by periods.  DictionaryKeys are just returned as lower case
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="type"></param>
        /// <param name="prefixSoFar"></param>
        /// <returns></returns>
        public static string GetCaseSensitivePropertyNameForModelProperty(string propertyName, Type type,
            string prefixSoFar = "")
        {
            var map = new Dictionary<string, PropertyInfo>(type.MapProperties(0),
                StringComparer.InvariantCultureIgnoreCase);

            var splitName = SplitPropertyName(propertyName);
            var hasProperty = map.ContainsKey(splitName[0]);

            var properName = hasProperty == true
                ? map.Keys.FirstOrDefault(x => x.Equals(splitName[0], StringComparison.InvariantCultureIgnoreCase))
                : string.Empty;

            if (splitName.Length == 2 && !string.IsNullOrEmpty(properName) && hasProperty == true)
            {
                var childType = map[properName].PropertyType;
                properName += ".";
                properName +=
                    $"{GetCaseSensitivePropertyNameForModelProperty(splitName[1], childType, $"{prefixSoFar}{properName}")}";
            }
            else if (type.IsDictionary())
            {
                properName += string.Join(".", propertyName.Split('.').Select(x => x.ToLower()).ToArray());
            }

            if (string.IsNullOrEmpty(properName))
                throw new ValidBadRequestException(Errors.BAD_FIELD_VALUE_QUERY.Message,
                    Errors.BAD_FIELD_VALUE_QUERY.Code);

            return properName;
        }

        /// <summary>
        /// Gets property value of object.  Can return null if key doesn't exist.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object GetPropValue(this object obj, string name)
        {
            if (obj == null)
            {
                return null;
            }

            foreach (var part in name.Split('.'))
            {
                var type = obj.GetType();
                var info = type.GetProperty(part,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (info == null)
                {
                    if (!type.IsDictionary())
                        return null;

                    var dict = obj as Dictionary<string, object>;

                    if (!dict.Any() || !dict.ContainsKey(part))
                        return null;

                    obj = dict[part];
                }
                else
                {
                    obj = info.GetValue(obj, null);
                }
            }

            return obj;
        }

        /// <summary>
        /// Creates a Dictionary where property name is key, and propertyinfo is value.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="maxRecursionLevel"></param>
        /// <returns></returns>
        public static Dictionary<string, PropertyInfo> MapProperties(this Type me, int maxRecursionLevel = 6)
        {
            return _MapProperties(me, maxRecursionLevel);
        }

        /// <summary>
        /// Creates a Dictionary where property name is key, and propertyinfo is value.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="maxRecursionLevel"></param>
        /// <returns></returns>
        public static Dictionary<string, PropertyInfo> MapProperties(this Object me, int maxRecursionLevel = 6)
        {
            var type = me.GetType();
            return type.MapProperties(maxRecursionLevel);
        }

        /// <summary>
        /// Attempts to parse a value that's a string, to a type.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object TryParse(this string me, Type type)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            return converter.ConvertFromString(null, CultureInfo.InvariantCulture, me);
        }

        /// <summary>
        /// Returns Type for property name string separated by periods
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Type GetPropertyType(string propertyName, Type type)
        {
            while (true)
            {
                var instance = Activator.CreateInstance(type ?? throw new ArgumentNullException(nameof(type)));
                var properName = GetCaseSensitivePropertyNameForModelProperty(propertyName, type);
                var splitName = SplitPropertyName(propertyName);

                if (splitName.Length == 2 && !string.IsNullOrEmpty(properName))
                {
                    var childPropertyInfo = instance.GetType()
                        .GetProperty(splitName[0],
                            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance);

                    var childType = childPropertyInfo?.PropertyType;
  
                    if (childType.IsDictionary()) return typeof(Dictionary<string, object>);
                    propertyName = splitName[1];
                    type = childType;
                    continue;
                }

                var map = new Dictionary<string, PropertyInfo>(type.MapProperties(0),
                    StringComparer.InvariantCultureIgnoreCase);

                return map[splitName[0]].PropertyType;
            }
        }

        internal static bool UsesJsonExtensionDataAttribute(string propertyName, Type type, out string jsonExtensionDataAttributeName)
        {
            jsonExtensionDataAttributeName = string.Empty;
            while (true)
            {
                var instance = Activator.CreateInstance(type ?? throw new ArgumentNullException(nameof(type)));
                var properName = GetCaseSensitivePropertyNameForModelProperty(propertyName, type);
                var splitName = SplitPropertyName(propertyName);

                if (splitName.Length == 2 && !string.IsNullOrEmpty(properName))
                {
                    var childPropertyInfo = instance.GetType()
                        .GetProperty(splitName[0],
                            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance);

                    var childType = childPropertyInfo?.PropertyType;
                    
                    if (Attribute.IsDefined(childPropertyInfo, typeof(JsonExtensionDataAttribute)))
                    {
                        jsonExtensionDataAttributeName = splitName[0];
                        return true;
                    }

                    propertyName = splitName[1];
                    type = childType;
                    continue;
                }

                var map = new Dictionary<string, PropertyInfo>(type.MapProperties(0),
                    StringComparer.InvariantCultureIgnoreCase);

                if(Attribute.IsDefined(map[splitName[0]], typeof(JsonExtensionDataAttribute)))
                {
                    propertyName = splitName[0];
                    return true;
                }

                return false;
                
            }
        }

        /// <summary>
        /// Determines if Type is Generic and has dictionary definition
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        internal static bool IsDictionary(this Type me)
        {
            return me.IsGenericType && (me.GetGenericTypeDefinition() == typeof(IDictionary<,>) ||
                                        me.GetGenericTypeDefinition() == typeof(Dictionary<,>));
        }

        /// <summary>
        /// Determines if Type is Numeric, or Boolean
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        internal static bool IsNumericOrBoolean(this Type me)
        {
            var type = Nullable.GetUnderlyingType(me) ?? me;
            
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                case TypeCode.Boolean:
                case TypeCode.DateTime:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Tries to add key value pair to dictionary, optional bool parameter to udate if exists
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="me"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="update"></param>
        internal static void TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> me, TKey key, TValue value,
            bool update = false)
        {
            if (!me.ContainsKey(key))
                me.Add(key, value);
            else if (update)
                me[key] = value;
        }

        /// <summary>
        /// Creates a Dictionary where property name is key, and propertyinfo is value.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="maxRecursionLevel"></param>
        /// <param name="recursionLevel"></param>
        /// <param name="propertyPathPrefix"></param>
        /// <returns></returns>
        private static Dictionary<string, PropertyInfo> _MapProperties(this Type me, int maxRecursionLevel,
            int recursionLevel = 0, string propertyPathPrefix = "")
        {
            recursionLevel++;
            var output =
                new Dictionary<string, PropertyInfo>(StringComparer.InvariantCultureIgnoreCase);

            var meProperties = me.GetProperties(
                BindingFlags.IgnoreCase |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance);

            foreach (var info in meProperties)
            {
                var key =
                    $"{propertyPathPrefix}{(!string.IsNullOrEmpty(propertyPathPrefix) ? "." : string.Empty)}{info.Name}";

                if (!info.PropertyType.IsSimple() && !(info.PropertyType.Namespace??string.Empty).StartsWith("System"))
                {
                    if (recursionLevel < maxRecursionLevel)
                    {
                        var subInfo =
                            info.PropertyType._MapProperties(maxRecursionLevel, recursionLevel, key);
                        foreach (var sub in subInfo)
                            output.TryAdd(sub.Key, sub.Value);
                    }
                }

                output.TryAdd(key, info);
            }

            return output;
        }

        /// <summary>
        /// Returns if type is primitive, or primitive-ish
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        private static bool IsSimple(this Type me)
        {
            //primitive == bool, byte, sbyte, int16,32,64,ptr, uint16,32,64,ptr, char, double, single
            return me.IsPrimitive ||
                   me.IsValueType ||
                   Convert.GetTypeCode(me) != TypeCode.Object ||
                   new Type[]
                   {
                       typeof(decimal),
                       typeof(string),
                       typeof(DateTime),
                       typeof(TimeSpan),
                       typeof(DateTimeOffset)
                   }.Contains(me);
        }

        /// <summary>
        /// Splits property name into two strings at the first occurrence of a period
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static string[] SplitPropertyName(string propertyName)
        {
            return propertyName.Split(new char[] {'.'}, 2);
        }
    }
}
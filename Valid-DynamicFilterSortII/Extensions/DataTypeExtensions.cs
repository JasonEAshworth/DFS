using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Valid_DynamicFilterSort.Extensions
{
    public static class DataTypeExtensions
    {
        #region is_type
            public static bool IsDateTime(this Type t)
            {
                var type = t.GetUnderlyingType();
                return (Type.GetTypeCode(type) == TypeCode.DateTime);
            }
            
            public static bool IsNumeric(this Type t)
            {
                var type = t.GetUnderlyingType();
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
                        return true;
                    default:
                        return false;
                }
            }
            
            public static bool IsNullable(this Type t)
            {
                return Nullable.GetUnderlyingType(t) != null;
            }
            
            public static bool IsString(this Type t)
            {
                var type = t.GetUnderlyingType();
                return (type == typeof(string));
            }
            
            public static bool IsCharArr(this Type t)
            {
                var type = t.GetUnderlyingType();
                return (type == typeof(char[]));
            }
            
            public static bool IsChar(this Type t)
            {
                var type = t.GetUnderlyingType();
                return (Type.GetTypeCode(type) == TypeCode.Char);
            }
            
            public static bool IsGuid(this Type t)
            {
                var type = t.GetUnderlyingType();
                return type == typeof(Guid);
            }
            
            public static bool IsBoolean(this Type t)
            {
                var type = t.GetUnderlyingType();
                return (Type.GetTypeCode(type) == TypeCode.Boolean);
            }
            
            public static bool IsEnum(this Type t)
            {
                return t.IsEnum;
            }
        
            public static bool IsEnumerable(this Type t)
            {
                var type = t.GetUnderlyingType();
                return type != typeof(string) && 
                       type.GetInterface(nameof(IEnumerable)) != null;
            }

            public static Type GetUnderlyingType(this Type t)
            {
                var type = Nullable.GetUnderlyingType(t) ?? t;
                return type;
            }
            
            internal static bool IsDictionary(this Type t)
            {
                var type = t.GetUnderlyingType();
                
                if (type.IsGenericType)
                {
                    return t.GetGenericTypeDefinition() == typeof(IDictionary<,>) ||
                           t.GetGenericTypeDefinition() == typeof(Dictionary<,>);
                }

                var typeInterfaces = type.GetInterfaces();
                var hasDictionaryInterface =  typeInterfaces
                    .Any(a => a == typeof(IDictionary) || a.IsDictionary());

                return hasDictionaryInterface;
            }
            #endregion is_type
        
        public static bool IsJson(this string value)
        {
            value = value.Trim();
            if ((!value.StartsWith("{") || !value.EndsWith("}")) &&
                (!value.StartsWith("[") || !value.EndsWith("]")))
            {
                return false;
            }
            
            try
            {
                JToken.Parse(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
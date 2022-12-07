using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Valid_DynamicFilterSort.Enums;
using Valid_DynamicFilterSort.Extensions;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Utilities;

namespace Valid_DynamicFilterSort.Defaults
{
    public class DefaultDataTypeValueHandler : IDataTypeValueHandler
    {
        private ILogger _logger;
        private readonly bool _loggerNeedsDisposed;

        public DefaultDataTypeValueHandler()
        {
            _logger = ServiceLocator.GetService<ILogger>();
            _loggerNeedsDisposed = true;
        }
        
        public DefaultDataTypeValueHandler(ILogger logger)
        {
            _logger = logger ?? ServiceLocator.GetService<ILogger>();
            _loggerNeedsDisposed = logger == null;
        }
        
        public bool Validate(string value, DataTypeEnum dataType)
        {
            return Validate(value, dataType, false);
        }

        public bool Validate<T>(string value)
        {
            return Validate(value, typeof(T));
        }

        public bool Validate(string value, Type type)
        {
            try
            {
                var converted = ConvertString(value, type);
                return string.IsNullOrWhiteSpace(value) || converted != null;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidateAndConvert(string value, Type type, out object result)
        {
            try
            {
                if (type == typeof(object))
                {
                    result = value;
                    return true;
                }
                
                result = ConvertString(value, type);
                return string.IsNullOrWhiteSpace(value) || result != null;
            }
            catch (Exception e)
            {
                result = default;
                return false;
            }
        }

        public bool ValidateAndConvert<T>(string value, out T result)
        {
            var success = ValidateAndConvert(value, typeof(T), out var objResult);
            result = success ? (T) objResult : default;
            return success;
        }
        
        private bool Validate(string value, DataTypeEnum dataType, bool allowEnumerables)
        {
            
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return true;
                }

                if (value.Contains(',') && dataType != DataTypeEnum.Json && dataType != DataTypeEnum.Text)
                {
                    return allowEnumerables && 
                           TryConvertEnumerable(typeof(string), value, out var enumerable) &&
                           enumerable.Count == enumerable.Count(x => Validate(x.ToString().Trim(), dataType));
                }
                 
                switch (dataType)
                {
                    case DataTypeEnum.Text:
                        return true;
                    case DataTypeEnum.Number:
                        return double.TryParse(value, out var dbl);
                    case DataTypeEnum.DateTime:
                        return DateTime.TryParse(value, out var dt);
                    case DataTypeEnum.Boolean:
                        return bool.TryParse(value, out var boo);
                    case DataTypeEnum.Guid:
                        return Guid.TryParse(value, out var guid);
                    case DataTypeEnum.Json when value.IsJson():
                        try
                        {
                            JsonConvert.DeserializeObject(value, JsonSerializerExtensions.SERIALIZER_SETTINGS);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("error validating data type value", e);
                return false;
            }
            finally
            {
                
            }
            
            
        }
        
        public bool ValidateAndConvert<T>(string value, DataTypeEnum dataType, out T result)
        {
            
            result = default(T);
            try
            {
                if (!DataTypeEnumMatchesType(typeof(T),value,dataType) || 
                    !Validate(value,dataType,typeof(T).IsEnumerable()))
                {
                    return false;
                }
                
                result = ConvertString<T>(value);
                
            }
            catch (Exception e)
            {
                _logger.LogError("error validating and converting data type value", e);
                return false;
            }
            finally
            {
                
            }

            return true;
        }

        public T ConvertString<T>(string value)
        {
            var type = typeof(T);
            return (T) ConvertString(value, type);
        }

        public object ConvertString(string value, Type type)
        {
            var t = type.GetUnderlyingType();
            object tempResult = null;
            var exc = new InvalidCastException($"Unable to cast value '{value}' to type {t.Name}");
            
            if (type.IsNullable() && string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (type == typeof(object))
            {
                return value;
            }

            if (t.IsChar())
            {
                tempResult = value.Length == 0 
                    ? throw exc 
                    : value[0];
            } 
            else if (t.IsCharArr())
            {
                tempResult = value.ToCharArray();
            } 
            else if (t.IsString())
            {
                tempResult = value;
                if (string.Equals("string.empty", value.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    tempResult = string.Empty;
                }
            }
            else if (t.IsNumeric() && double.TryParse(value, out var dbl))
            {
                tempResult = dbl;
            }
            else if (t.IsDateTime() && DateTime.TryParse(value, out var dt))
            {
                tempResult = dt;
            }
            else if (t.IsBoolean() && bool.TryParse(value, out var boo))
            {
                tempResult = boo;
            }
            else if (t.IsGuid() && Guid.TryParse(value, out var guid))
            {
                tempResult = guid;
            }
            else if (t.IsEnum() && TryConvertEnum(value,t, out var e))
            {
                tempResult = e;
            }
            else if (value.IsJson() && TryConvertJson(value,t, out var json))
            {
                tempResult = json;
            } 
            else if (type.IsEnumerable() && TryConvertEnumerable(type, value, out var values))
            {
                tempResult = values;
            }
            
            if(!TryConvert(type, tempResult, out var resultObject))
            {
                throw exc;
            }

            return resultObject;
        }

        private static bool TryConvertJson(string value, Type type, out object result)
        {
            result = null;
            if (!value.IsJson())
            {
                return false;
            }

            try
            {
                result = type.IsDictionary()
                    ? value.ToJsonDictionary()
                    : JsonConvert.DeserializeObject(value, type);
            }
            catch
            {
                return false;
            }

            return true;
        }
        
        private static bool TryConvertEnum(string value, Type type, out object result)
        {
            result = null;

            try
            {
                result = Enum.Parse(type, value, true);
            }
            catch
            {
                return false;
            }

            return true;
        }
        
        private bool DataTypeEnumMatchesType(Type type, string value, DataTypeEnum dataType)
        {
            if (type.IsNullable() && string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            switch (dataType)
            {
                case DataTypeEnum.Text when type.IsChar():
                case DataTypeEnum.Text when type.IsCharArr():
                case DataTypeEnum.Text when type.IsString():
                case DataTypeEnum.Number when type.IsNumeric():
                case DataTypeEnum.DateTime when type.IsDateTime():
                case DataTypeEnum.Boolean when type.IsBoolean():
                case DataTypeEnum.Guid when type.IsGuid():
                    return true;
                case DataTypeEnum.Json when value.IsJson():
                    try
                    {
                        JsonConvert.DeserializeObject(value, type, JsonSerializerExtensions.SERIALIZER_SETTINGS);
                    }
                    catch
                    {
                        return false;
                    }
                    return true;
                default:
                    return type.IsEnumerable() && TryConvertEnumerable(type, value, out var values);
            }
        }

        private static bool TryConvert(Type type, object value, out object result)
        {
            var underlyingType = type.GetUnderlyingType();
            var valueType = type == typeof(object) 
                ? typeof(object) 
                : value?.GetType().GetUnderlyingType() ?? value?.GetType();
            
            if (underlyingType == valueType)
            {
                result = value;
                return true;
            }
            
            try
            {
                result = Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? type);
                return true;
            }
            catch
            {
                //do nothing;
            }

            try
            {
                result = TypeDescriptor.GetConverter(type).ConvertFrom(value);
                return true;
            }
            catch
            {
                //do nothing;
            }

            try
            {
                result = type.IsEnumerable()
                    ? JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value), type)
                    : value;

                return true;
            }
            catch
            {
                result = Activator.CreateInstance(type);
                return false;
            }
        }
        
        private static Type GetTypeOfEnumerable(Type t)
        {
            var enumerableOf = t.GetInterfaces()
                .Where(x => x.IsGenericType &&
                            x.GetInterfaces().Contains(typeof(IEnumerable)) &&
                            x.GetInterfaces().Any(i => i.Name != nameof(IEnumerable)))
                .SelectMany(s => s.GetGenericArguments())
                .Distinct()
                .ToArray()
                .FirstOrDefault();

            return enumerableOf??typeof(string);
        }

        public virtual DataTypeEnum TypeToDataTypeEnum(Type t, string value = "")
        {
            while (true)
            {
                if (t.IsEnumerable())
                {
                    t = GetTypeOfEnumerable(t);
                    continue;
                }

                if (t.IsBoolean())
                {
                    return DataTypeEnum.Boolean;
                }

                if (t.IsNumeric())
                {
                    return DataTypeEnum.Number;
                }

                if (t.IsDateTime())
                {
                    return DataTypeEnum.DateTime;
                }

                if (t.IsGuid())
                {
                    return DataTypeEnum.Guid;
                }

                if (!string.IsNullOrWhiteSpace(value) && value.IsJson())
                {
                    return DataTypeEnum.Json;
                }

                return DataTypeEnum.Text;
            }
        }

        private bool TryConvertEnumerable(Type t, string v, out List<object> result)
        {
            result = new List<object>();
            var enumerableType = GetTypeOfEnumerable(t);

            var dtEnum = TypeToDataTypeEnum(enumerableType, v);
            
            var split = v.Split(',').Where(x => !string.IsNullOrWhiteSpace(x.Trim())).ToArray();
            var splitCount = split.Count();
            var converts = split
                .Where(x => Validate(x, dtEnum))
                .Select(x => ConvertString(x, enumerableType)).ToList();

            if (splitCount != converts.Count)
            {
                return false;
            }
            
            result = converts;
            return true;

        }

        public void Dispose()
        {
            if (_loggerNeedsDisposed)
            {
                _logger = null;
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Models;
using Valid_DynamicFilterSort.Utilities;

namespace Valid_DynamicFilterSort.Extensions
{
    public static class PropertyExtension
    {
        public static void CreateDefaultValueIfNotExist(this object entity, DFSPropertyInfo propertyInfo)
        {
            var path = new List<DFSPropertyInfo>(propertyInfo.PathHistory) {propertyInfo};
            using var dataTypeValueHandler = ServiceLocator.GetService<IDataTypeValueHandler>();
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance;
            var currentEntity = entity;
            var pathNumber = 0;
            foreach (var part in path)
            {
                pathNumber++;

                if (!TryGetValueOrDefault(currentEntity, part.Key, out var value))
                {
                    throw new SystemException($"current value for {part.Key} could not be retrieved");
                }

                try
                {
                    if (currentEntity.GetType().IsDictionary())
                    {
                        var dictArgs = currentEntity.GetType().GetDictionaryArguments();
                        var dictKey = dataTypeValueHandler.ConvertString(part.Key, dictArgs[0]);
                        var method = typeof(PropertyExtension).GetMethod(nameof(SetDictionaryValue), BindingFlags.NonPublic | BindingFlags.Static);
                        var genericMethod = method.MakeGenericMethod(dictArgs);
                        value = pathNumber < path.Count && !value.GetType().IsDictionary() ? new Dictionary<object, object>() : value;
                        genericMethod.Invoke(null, new object[] {currentEntity, dictKey, value});
                    }
                    else
                    {
                        var pi = currentEntity.GetType().GetProperty(part.Key,
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase |
                            BindingFlags.Instance);
                        pi?.SetValue(currentEntity, value);
                    }

                    currentEntity = value;
                }
                catch (Exception e)
                {
                    throw new SystemException($"value could not be set for {currentEntity.GetType().Name}:{part.Key}", e);
                }
            }
        }
        
        private static bool TryGetValueOrDefault(this object entity, string key, out object result)
        {
            result = null;
            using var dataTypeValueHandler = ServiceLocator.GetService<IDataTypeValueHandler>();
            var myType = entity.GetType();
            
            // find prop or null
            var pi = myType.GetProperty(key, 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance);
            
            // check dictionary things
            if (myType.IsDictionary())
            {
                var dictArgs = GetDictionaryArguments(myType);
                var keyTypeMatch = dataTypeValueHandler.ValidateAndConvert(key, dictArgs[0], out var dictKey);
                if (!keyTypeMatch)
                {
                    return false;
                }

                // get value or make one up
                var method =typeof(PropertyExtension).GetMethod(nameof(GetDictionaryValue), BindingFlags.NonPublic | BindingFlags.Static);
                var genericMethod = method.MakeGenericMethod(dictArgs);
                result = pi == null
                    ? genericMethod.Invoke(null, new object[] {entity, dictKey}) ??
                      FormatterServices.GetSafeUninitializedObject(dictArgs[1])
                    : pi.GetValue(entity,
                          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance,
                          null, null, CultureInfo.InvariantCulture) ??
                      FormatterServices.GetSafeUninitializedObject(pi.PropertyType);
                
                return true;
            }

            // property into doesn't exist
            if (pi == null)
            {
                return false;
            }

            result = pi.GetValue(entity,
                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance,
                         null, null, CultureInfo.InvariantCulture) ??
                     FormatterServices.GetSafeUninitializedObject(pi.PropertyType);

            return true;
        }
        
        private static TValue GetDictionaryValue<TKey, TValue>(IDictionary<TKey, TValue> data, TKey key)
        {
            if (data.ContainsKey(key))
            {
                return data[key];
            }

            return default;
        }
        
        private static void SetDictionaryValue<TKey, TValue>(IDictionary<TKey, TValue> data, TKey key, object value)
        {
            try
            {
                data[key] = (TValue) value;
            }
            catch
            {
                try
                {
                    data[key] = (TValue) FormatterServices.GetSafeUninitializedObject(typeof(TValue));
                }
                catch
                {
                    data[key] = (TValue) new object();
                }
            }
        }
        
        public static DFSPropertyInfo GetPropertyInformation<TEntity>(string propertyName,
            params string[] propertyDelimiter) where TEntity : class, new()
        {
            // get delimiters to split property name (parameter key)
            var delimiters = propertyDelimiter == null || propertyDelimiter.Length == 0
                ? new[] {"."}
                : propertyDelimiter;

            var defaultDelimiter = delimiters[0];
            
            // split property name (parameter key) into many parts -- goal is to get information about the final property listed
            var propertyParts = propertyName.Split(delimiters
                .OrderByDescending(o => o.Length)
                .ThenByDescending(t => t)
                .ToArray(), StringSplitOptions.RemoveEmptyEntries);
            
            var pathHistory = new List<DFSPropertyInfo>();

            for (var i = 0; i < propertyParts.Length; i++)
            {
                var part = propertyParts[i].Trim().ToLowerInvariant();
                var parentEntityType = i == 0 ? typeof(TEntity) : pathHistory[i - 1].Type;
                var partPropertyInformation = GetPartInformation(part, parentEntityType, defaultDelimiter, ref pathHistory);
                pathHistory.Add(partPropertyInformation);
            }

            var result = pathHistory.Last();
            return result;
        }

        private static DFSPropertyInfo GetPartInformation(string part, Type parentEntityType, string defaultDelimiter, ref List<DFSPropertyInfo> pathHistory)
        {
            try
            {
                var result = new DFSPropertyInfo();
            
                // gets PropertyInfo from the parent entity type
                var partPropertyInfo = parentEntityType.GetProperty(part, 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance);

                var isDictionary = parentEntityType.IsDictionary();
                
                // if there is not property info for this part, and the parent isn't a dictionary,
                // and there have been no dictionaries in the past, we have to presume this in an invalid property since it
                // cannot be found on the parent type, and the parent type isn't a dictionary
                if (partPropertyInfo == null && !isDictionary &&
                    pathHistory.All(a => !a.Type.IsDictionary()))
                {
                    throw DynamicFilterSortErrors.PROPERTY_NAME_INVALID($"'{part}' could not be found in {parentEntityType.Name}");
                }

                // if it's a dictionary, we should ensure the key matches the key type
                if (isDictionary)
                {
                    using var dataTypeValueHandler = ServiceLocator.GetService<IDataTypeValueHandler>();
                    var dictionaryType = GetDictionaryArguments(parentEntityType);
                    var keyType = dictionaryType[0];

                    // validate key type against property name part
                    if (!dataTypeValueHandler.Validate(part, keyType))
                    {
                        throw DynamicFilterSortErrors.PROPERTY_NAME_INVALID($"'{pathHistory.LastOrDefault()?.PathHistory}' section '{part}' does not satisfy type of '{keyType.Name}'");
                    }
                }

                result.Delimiter = defaultDelimiter;
                result.JsonExtensionProperty = partPropertyInfo != null && 
                                               Attribute.IsDefined(partPropertyInfo, typeof(JsonExtensionDataAttribute));
                result.Key = partPropertyInfo?.Name ?? part; // propertyName or passed in value
                result.PathKey = (pathHistory.LastOrDefault()?.PathKey + defaultDelimiter + result.Key) // join with last key
                    .Trim(result.Delimiter.ToCharArray()); // and trim dangling delimiters
                result.Type = partPropertyInfo?.PropertyType ?? // if able to get property info, use it's type
                              (isDictionary // otherwise determine if the parent is a dictionary
                                  ? parentEntityType.GetDictionaryArguments()[1] // if it's a dictionary, get the value's type
                                  : pathHistory.LastOrDefault()?.Type.IsDictionary() ?? false // check if last part was dict
                                      ? pathHistory.Last().Type.GetDictionaryArguments()[1] // if was, get value's type
                                      : typeof(object)); // default to object
                result.TraversalProperty = parentEntityType.IsDictionary() || pathHistory.Any(a => a.TraversalProperty);
                result.PathHistory = new List<DFSPropertyInfo>(pathHistory);

                return result;
            }
            catch (Exception e)
            {
                ServiceLocator.GetService<ILogger>().LogError("error getting property part info {e}", e);
                throw;
            }
        }

        private static Type[] GetDictionaryArguments(this Type type)
        {
            while (true)
            {
                if (type.IsGenericType && type.IsDictionary())
                {
                    return type.GenericTypeArguments;
                }
        
                var dictInterface = type.GetInterfaces()
                    .FirstOrDefault(f => f.IsDictionary());
        
                if (dictInterface == null)
                {
                    return new Type[] { };
                }
                
                type = dictInterface;
            }
        }
    }
}
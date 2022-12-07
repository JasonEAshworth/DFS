using System;
using System.Collections.Generic;

namespace Valid_DynamicFilterSort
{
    public static class JsonExtensions
    {
        public static bool TryGetStringPropertyAndNotNullOrEmpty(this Json fields, string propertyName, out string value)
        {
            var ret = fields.TryGetProperty(propertyName, out var outObj) && outObj is string && !string.IsNullOrEmpty(outObj.ToString());
            value = ret ? outObj.ToString() : string.Empty;
            return ret;
        }

        public static bool TryGetIntPropertyAndNotNull(this Json fields, string propertyName, out int value)
        {
            var ret = fields.TryGetProperty(propertyName, out var outObj) && outObj is int;
            value = ret ? (int)outObj : 0;

            if (outObj is string && int.TryParse(outObj.ToString(), out int parsedValue))
            {
                ret = true;
                value = parsedValue;
            }
            return ret;
        }

        public static bool TryGetBoolProperty(this Json fields, string propertyName)
        {
            var ret = fields.TryGetProperty(propertyName, out var outObj) && outObj as bool? == true;
            if (!ret && outObj is string && bool.TryParse(outObj.ToString(), out ret))
            {
                return ret;
            }
            return ret;
        }
        
        public static Json Add(this Json json, string key, object value)
        {
            if (!json.HasProperty(key))
            {
                json.Data.Add(key.ToLowerInvariant(), value);
            }
            else
            {
                throw new ArgumentException($"Property already exists", key.ToLowerInvariant());
            }

            return json;
        }
        
        public static Json Add(this Json json, KeyValuePair<string, object> keyValuePair)
        {
            var (key, value) = keyValuePair;
            return Add(json, key, value);
        }
        
        public static Json Add(this Json json, Json.Property property)
        {
            return Add(json, property.ToKeyValuePair());
        }
    }
}
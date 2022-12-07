using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Valid_DynamicFilterSort.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string, object> ToJsonDictionary(this object obj)
        {
            var jobj = obj as JObject;
            var jarr = obj as JArray;
            
            if (jobj != null)
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    jobj.ToString(), 
                    JsonSerializerExtensions.SERIALIZER_SETTINGS);
            }
            
            if (jarr != null)
            {
                return jarr.Children<JObject>()
                    .SelectMany(content => content.Properties())
                    .ToDictionary<JProperty, string, object>(
                        prop => prop.Name, 
                        prop => prop.Value);
            }

            var resultData = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(obj, JsonSerializerExtensions.SERIALIZER_SETTINGS), 
                JsonSerializerExtensions.SERIALIZER_SETTINGS);
            
            return new Dictionary<string, object>(resultData, StringComparer.InvariantCultureIgnoreCase);
        }
        
        public static Dictionary<string, object> ToJsonDictionary(this string s)
        {
            var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                s, JsonSerializerExtensions.SERIALIZER_SETTINGS);
            
            var newData = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            
            foreach (var kvp in json)
            {
                if (kvp.Value == null ||
                    kvp.Value != null && kvp.Value.GetType().IsPrimitive ||
                    kvp.Value.GetType().IsDateTime() ||
                    kvp.Value.GetType().IsEnumerable() ||
                    kvp.Value.GetType().IsString())
                {
                    newData.Add(kvp.Key, kvp.Value);
                }
                else if (typeof(JArray) == kvp.Value.GetType())
                {
                    newData.Add(kvp.Key, JsonConvert.DeserializeObject<List<object>>(JsonConvert.SerializeObject(kvp.Value)));
                }
                else
                {
                    newData.Add(kvp.Key, kvp.Value.ToJsonDictionary());
                }
            }
           
            return newData;
        }
    }
}
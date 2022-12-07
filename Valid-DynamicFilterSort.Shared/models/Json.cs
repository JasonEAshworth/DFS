using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Valid_DynamicFilterSort
{
    /// <summary>
    /// A class to store unstructured data
    /// </summary>
    public class Json : IDictionary<string, object>
    {
        #region properties
        /// <summary>
        /// Stores data in the <see cref="Json"/> class
        /// </summary>
        [JsonProperty]
        [JsonExtensionData]
        internal Dictionary<string, object> Data { get; set; }
        #endregion
        
        #region jsonSerializerSettings
        private static readonly JsonSerializerSettings LowerCaseSerializationSettings =
            new JsonSerializerSettings
            {
                ContractResolver = new LowerCasePropertyNameContractResolver(),
                TypeNameHandling = TypeNameHandling.All,
                Converters = new JsonConverter[]
                {
                    new EnsureDictionaryUsageConverter()
                }
            };
        

        private static readonly JsonSerializerSettings LowerCaseAndNoTypeSerializerSettings = new JsonSerializerSettings {
            ContractResolver = new LowerCasePropertyNameContractResolver(),
            TypeNameHandling = TypeNameHandling.None,
            Converters = new JsonConverter[]
            {
                new EnsureDictionaryUsageConverter()
            }
        };
        #endregion jsonSerializerSettings
        
        #region constructors

        public Json() : this(null)
        {
            
        }

        public Json(params Property[] property)
        {
            Data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            AddOrUpdate(property);
        }
        
        #endregion constructors
        
        #region staticCreators
        public static Json FromJsonString(string s)
        {
            var json = JsonConvert.DeserializeObject<Json>(s, LowerCaseSerializationSettings);
            var newData = new Dictionary<string, object>();
            foreach (var kvp in json.Data)
            {
                if (kvp.Value == null ||
                    (kvp.Value != null && kvp.Value.GetType().IsPrimitive) ||
                    (kvp.Value is DateTime) ||
                    (kvp.Value is IEnumerable) ||
                    (kvp.Value is string))
                {
                    newData.Add(kvp.Key, kvp.Value);
                }
                else if (typeof(JArray) == kvp.Value.GetType())
                {
                    newData.Add(kvp.Key, JsonConvert.DeserializeObject<List<object>>(JsonConvert.SerializeObject(kvp.Value)));
                }
                else
                {
                    newData.Add(kvp.Key, FromObject(kvp.Value));
                }
            }
            json.Data = newData;
            return json;
        }
        
        public static Json FromObject(object obj)
        {
            var jobj = obj as JObject;
            var jarr = obj as JArray;
            
            if (jobj != null)
            {
                return JsonConvert.DeserializeObject<Json>(jobj.ToString(), LowerCaseAndNoTypeSerializerSettings);
            }
            if (jarr != null) 
            {
                var json = new Json();
                
                foreach(var content in jarr.Children<JObject>())
                foreach (var prop in content.Properties()) 
                {
                    json.Add(prop.Name, prop.Value);
                }

                return json;
            }

            return JsonConvert.DeserializeObject<Json>(JsonConvert.SerializeObject(obj, LowerCaseAndNoTypeSerializerSettings), LowerCaseAndNoTypeSerializerSettings);
        }
        
        public static implicit operator Json(string jsonString)
        {
            var json = new Json { Data = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString) };
            return json;
        }
        #endregion staticCreators
        
        #region classMethods
        public Json AddOrUpdate(string key, object value)
        {
            if (!HasProperty(key))
            {
                Add(key, value);
            }
            else
            {
                SetProperty(key, value);
            }

            return this;
        }

        public Json AddOrUpdate(params KeyValuePair<string, object>[] keyValuePair)
        {
            if (keyValuePair == null || keyValuePair.Length == 0)
            {
                return this;
            }
            
            foreach(var kvp in keyValuePair)
            {
                AddOrUpdate(new Property(kvp));
            }

            return this;
        }

        public Json AddOrUpdate(params Property[] properties)
        {
            if (properties == null || properties.Length == 0)
            {
                return this;
            }

            foreach (var property in properties)
            {
                AddOrUpdate(property.Key, property.Value);
            }

            return this;
        }

        public object GetProperty(string propertyName)
        {
            if (HasProperty(propertyName, StringComparison.InvariantCultureIgnoreCase))
            {
                return Data[propertyName.ToLowerInvariant()];
            }
            
            throw new KeyNotFoundException($"Property '{propertyName}' does not exist");
        }

        public object GetProperty(string propertyName, StringComparison compareType)
        {
            if (HasProperty(propertyName, compareType))
            {
                var key = Data.Keys.First(x => x.Equals(propertyName, compareType));
                return Data[key];
            }
            else
            {
                throw new KeyNotFoundException($"Property '{propertyName.ToLowerInvariant()}' does not exist");
            }
        }

        public IEnumerable<string> GetPropertyNames()
        {
            return Data.Keys;
        }

        public IEnumerable<object> GetPropertyValues()
        {
            return Data.Values;
        }

        public IEnumerable<string> HasProperties(IEnumerable<string> propertyNames)
        {
            return propertyNames.Where(HasProperty);
        }

        public bool HasProperty(string propertyName)
        {
            return HasProperty(propertyName, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool HasProperty(string propertyName, StringComparison compareType)
        {
            return !string.IsNullOrWhiteSpace(propertyName) && 
                   Data.Keys.Any(x => x.ToLowerInvariant().Equals(propertyName.ToLowerInvariant(), compareType));
        }

        public void SetProperty(string propertyName, object propertyValue)
        {
            SetProperty(propertyName, propertyValue, StringComparison.InvariantCultureIgnoreCase);
        }

        public void SetProperty(string propertyName, object propertyValue, StringComparison compareType)
        {
            if (HasProperty(propertyName, compareType))
            {
                var key = Data.Keys.First(x => x.Equals(propertyName, compareType));
                Data[key] = propertyValue;
            }
            else
            {
                throw new KeyNotFoundException($"Property '{propertyName}' does not exist");
            }
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool includeType)
        {
            return JsonConvert.SerializeObject(Data, includeType ? LowerCaseSerializationSettings : LowerCaseAndNoTypeSerializerSettings);
        }

        public Dictionary<string, object> ToDictionary()
        {
            return Data;
        }

        public bool TryGetProperty(string propertyName, out object value)
        {
            if (!HasProperty(propertyName))
            {
                value = null;
                return false;
            }
            
            value = GetProperty(propertyName);
            return true;
        }
        #endregion classMethods
        
        #region propertySubclass
        /// <summary>
        /// A property of the <see cref="Json"/> class
        /// </summary>
        public class Property
        {
            public string Key { get; set; }
            public object Value { get; set; }

            public Property()
            {
            
            }

            public Property(string key, object value)
            {
                Key = key;
                Value = value;
            }

            public Property(KeyValuePair<string, object> keyValuePair)
            {
                (Key, Value) = keyValuePair;
            }

            public KeyValuePair<string, object> ToKeyValuePair()
            {
                return new KeyValuePair<string, object>(Key, Value);
            }
        }
        #endregion propertySubclass
        
        #region IDictionaryStringObjectImplementation
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            ((IDictionary<string, object>) Data).Add(item);
        }

        public void Clear()
        {
            Data.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return Data.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((IDictionary<string, object>) Data).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>) Data).Remove(item);
        }

        public int Count => Data.Count;
        public bool IsReadOnly => ((IDictionary<string, object>) Data).IsReadOnly;
        public void Add(string key, object value)
        {
            Data.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return Data.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return Data.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return Data.TryGetValue(key, out value);
        }

        public object this[string key]
        {
            get => key.Equals("data", StringComparison.InvariantCultureIgnoreCase) && 
                   !Data.ContainsKey("data") 
                ? Data 
                : Data[key];
            set => Data[key] = value;
        }

        public ICollection<string> Keys => Data.Keys;
        public ICollection<object> Values => Data.Values;

        #endregion IDictionaryStringObjectImplementation
    }

    
}
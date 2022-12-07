using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Valid_DynamicFilterSort.Extensions
{
    public static class JsonSerializerExtensions
    {
        public static readonly JsonSerializerSettings SERIALIZER_SETTINGS = new JsonSerializerSettings
        {
            ContractResolver = new LowerCasePropertyNameContractResolver(),
            Converters = new List<JsonConverter>
            {
                new EnsureDictionaryUsageConverter()
            },
            TypeNameHandling = TypeNameHandling.None
        };
    }
    
    internal class EnsureDictionaryUsageConverter : CustomCreationConverter<Dictionary<string, object>>
    {
        public override Dictionary<string, object> Create(Type objectType)
        {
            return new Dictionary<string, object>();
        }
    }
    internal class LowerCasePropertyNameContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLowerInvariant();
        }
    }
}
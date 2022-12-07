using System;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

namespace Valid_DynamicFilterSort
{
    public class EnsureDictionaryUsageConverter : CustomCreationConverter<Dictionary<string, object>>
    {
        public override Dictionary<string, object> Create(Type objectType)
        {
            return new Dictionary<string, object>();
        }
    }
}
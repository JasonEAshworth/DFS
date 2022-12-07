using Newtonsoft.Json.Serialization;

namespace Valid_DynamicFilterSort
{
    public class LowerCasePropertyNameContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLowerInvariant();
        }
    }
}
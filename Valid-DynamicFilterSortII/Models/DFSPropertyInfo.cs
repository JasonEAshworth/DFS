using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Valid_DynamicFilterSort.Base;

namespace Valid_DynamicFilterSort.Models
{
    public class DFSPropertyInfo : BaseFieldObject
    {
        /// <summary>
        /// Property name on model
        /// </summary>
        public string Key { get; set; }
        
        /// <summary>
        /// Path to property (nested models)
        /// </summary>
        public string PathKey { get; set; }
        
        /// <summary>
        /// Property type
        /// </summary>
        public Type Type { get; set; }
        
        /// <summary>
        /// True if property was accessed through a dictionary or collection
        /// </summary>
        public bool TraversalProperty { get; set; }
        
        /// <summary>
        /// List of property info for each segment in the path key
        /// </summary>
        public List<DFSPropertyInfo> PathHistory { get; set; } = new List<DFSPropertyInfo>();

        /// <summary>
        /// True if attribute is decorated with a [JsonExtensionDataAttribute]
        /// </summary>
        public bool JsonExtensionProperty { get; set; } = false;

        /// <summary>
        /// Delimiter used to separate path key
        /// </summary>
        public string Delimiter { get; set; } = ".";
    }
}
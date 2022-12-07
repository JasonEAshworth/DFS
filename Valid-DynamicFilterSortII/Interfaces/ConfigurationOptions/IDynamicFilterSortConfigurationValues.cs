using System;
using System.Collections.Generic;

namespace Valid_DynamicFilterSort.Interfaces.ConfigurationOptions
{
    public interface IDynamicFilterSortConfigurationValues
    {
        /// <summary>
        /// Collection of ISyntaxParser and ISyntaxParser&lt;T&gt;
        /// </summary>
        public ICollection<Type> SyntaxParsers { get; }
        /// <summary>
        /// Dictionary of default parsers for IParameter classes
        /// </summary>
        public Dictionary<Type, Type> DefaultSyntaxParsers { get; }
        /// <summary>
        /// Dictionary of Data Interface Types (Modules) that DFS Supports
        /// </summary>
        Dictionary<string, Type> DataInterfaceTypes { get; }
    }
}
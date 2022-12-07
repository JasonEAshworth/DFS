using System.Collections.ObjectModel;

namespace Valid_DynamicFilterSort.Interfaces
{
    /// <summary>
    /// Interface for parameters; e.g. FilterParameter, SortParameter
    /// </summary>
    public interface IParameter : IHaveFields
    {
        /// <summary>
        /// Parameter's Key; i.e. Property Name
        /// </summary>
        string Key { get; set; }
        /// <summary>
        /// Parameter's Value as a given type
        /// </summary>
        object Value { get; set; }
        /// <summary>
        /// Order of the parameter in the parameter string
        /// </summary>
        int Order { get; set; }
    }
}
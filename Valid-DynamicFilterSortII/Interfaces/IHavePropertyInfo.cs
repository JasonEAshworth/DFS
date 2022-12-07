using Valid_DynamicFilterSort.Models;

namespace Valid_DynamicFilterSort.Interfaces
{
    /// <summary>
    /// Provides helpful metadata about an IParameter (or possibly another type)
    /// </summary>
    public interface IHavePropertyInfo
    {
        /// <summary>
        /// Helpful metadata about an IParameter (or possibly another type)
        /// </summary>
        DFSPropertyInfo PropertyInfo { get; set; }
    }
}
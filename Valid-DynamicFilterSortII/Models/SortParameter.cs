using Valid_DynamicFilterSort.Base;
using Valid_DynamicFilterSort.Interfaces;

namespace Valid_DynamicFilterSort.Models
{
    public class SortParameter : BaseParameter, IHavePropertyInfo
    {
        /// <inheritdoc/>>
        public DFSPropertyInfo PropertyInfo { get; set; }
    }
}
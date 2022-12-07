using Valid_DynamicFilterSort.Base;
using Valid_DynamicFilterSort.Enums;
using Valid_DynamicFilterSort.Interfaces;

namespace Valid_DynamicFilterSort.Models
{
    public class FilterParameter : BaseParameter, IFilterParameter, IHavePropertyInfo
    {
        /// <inheritdoc/>>
        public OperatorEnum Operator { get; set; } = OperatorEnum.EqualTo;
        /// <inheritdoc/>>
        public OperatorCombinerEnum Combiner { get; set; } = OperatorCombinerEnum.AND;
        /// <inheritdoc/>>
        public ComparisonTypeEnum ComparisonType { get; set; } = ComparisonTypeEnum.Full;
        /// <inheritdoc/>>
        public DFSPropertyInfo PropertyInfo { get; set; }
        /// <summary>
        /// Value passed in from filter string
        /// </summary>
        public string ValueString { get; set; }
        /// <summary>
        /// Key passed in from filter string
        /// </summary>
        public string KeyString { get; set; }
        /// <summary>
        /// Operator passed in from filter string
        /// </summary>
        public string OperatorString { get; set; }
    }
}
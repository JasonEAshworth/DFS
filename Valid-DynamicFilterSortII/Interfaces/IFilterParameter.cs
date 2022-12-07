using Valid_DynamicFilterSort.Enums;

namespace Valid_DynamicFilterSort.Interfaces
{
    /// <summary>
    /// Type of IParameter designed for Filter
    /// </summary>
    public interface IFilterParameter : IParameter
    {
        /// <summary>
        /// Comparison Operator; e.g. EqualTo, GreaterThanOrEqualTo, LessThan
        /// </summary>
        OperatorEnum Operator { get; set; }
        /// <summary>
        /// A Logical Conjunction to combine parameters; e.g. AND, OR
        /// </summary>
        OperatorCombinerEnum Combiner { get; set; }
        /// <summary>
        /// Type of comparison; e.g. Full, StartsWith, EndsWith, Contains
        /// </summary>
        ComparisonTypeEnum ComparisonType { get; set; }
        
    }
}
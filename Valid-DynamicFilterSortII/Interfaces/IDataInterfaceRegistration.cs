using System;
using System.Collections.Generic;

namespace Valid_DynamicFilterSort.Interfaces
{
    /// <summary>
    /// Components needed to register a data interface type
    /// </summary>
    public interface IDataInterfaceRegistration : IHaveDataInterfaceType
    {
        /// <summary>
        /// Collection of registered Data Syntax Builders for a Data Interface Type (e.g. DynamicLinq)
        /// </summary>
        ICollection<Type> DataSyntaxBuilders { get; }
        /// <summary>
        /// Mapped collection of Data Syntax Builders for a specific type of ISyntaxParameter
        /// Key: typeof(ISyntaxParameter), Value: typeof(IDataSyntaxBuilder&lt;TParameter,TDataSyntax&gt;)
        /// </summary>
        Dictionary<Type, Type> ParameterDataSyntaxBuilders { get; }
        /// <summary>
        /// IDataAccessor Service used to access data for this data type using a DataSyntaxBuilder
        /// </summary>
        Type DataAccessor { get; }
    }
}
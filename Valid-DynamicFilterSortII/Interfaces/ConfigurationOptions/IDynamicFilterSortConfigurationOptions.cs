using Valid_DynamicFilterSort.Base;

namespace Valid_DynamicFilterSort.Interfaces.ConfigurationOptions
{
    public interface IDynamicFilterSortConfigurationOptions
    {
        /// <summary>
        /// Registers a syntax parser to parse strings into <see cref="IParameter"/>
        /// </summary>
        /// <typeparam name="TSyntaxParser">Type implementing <see cref="ISyntaxParser"/></typeparam>
        /// <returns>IDynamicFilterSortConfigurationOptions</returns>
        IDynamicFilterSortConfigurationOptions AddSyntaxParser<TSyntaxParser>()
            where TSyntaxParser : class, ISyntaxParser;

        /// <summary>
        /// Registers a syntax parser to parse a string into a specified parameter type.
        /// </summary>
        /// <param name="asDefault">Sets as default parser for </param>
        /// <typeparam name="TSyntaxParser">Type implementing <see cref="ISyntaxParser&lt;TParameter&gt;"/></typeparam>
        /// <typeparam name="TParameter">Parameter Type, e.g. FilterParameter, SortParameter</typeparam>
        /// <returns>IDynamicFilterSortConfigurationOptions</returns>
        IDynamicFilterSortConfigurationOptions AddSyntaxParser<TSyntaxParser, TParameter>(bool asDefault = false)
            where TSyntaxParser : class, ISyntaxParser<TParameter>
            where TParameter : class, IParameter;

        /// <summary>
        /// Registers data type module to dynamic filter sort
        /// </summary>
        /// <typeparam name="TModule">Type implementing <see cref="BaseDataInterfaceRegistration"/></typeparam>
        /// <returns>IDynamicFilterSortConfigurationOptions</returns>
        IDynamicFilterSortConfigurationOptions AddDataTypeModule<TModule>()
            where TModule : BaseDataInterfaceRegistration, new();
    }
}
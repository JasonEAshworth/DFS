using System.Collections.Generic;
using Valid_DynamicFilterSort.Base;

namespace Valid_DynamicFilterSort.Interfaces
{
    /// <summary>
    /// Turns IParameters into a usable format for a data source
    /// </summary>
    public interface IDataSyntaxBuilder : IHaveDataInterfaceType
    {
        /// <summary>
        /// Takes enumerable of TParameter and converts into a usable format for a data source
        /// </summary>
        /// <param name="parameters">Enumerable of Parameters</param>
        /// <typeparam name="TParameter">Type inheriting from IParameter</typeparam>
        /// <returns>object</returns>
        BaseDataSyntaxModel BuildDataSyntax<TParameter>(IEnumerable<TParameter> parameters)
            where TParameter : class, IParameter, new();
    }

    /// <summary>
    /// Turns a type implementing IParameter into a usable format for a data source
    /// </summary>
    /// <typeparam name="TParameter">type that implements <see cref="IParameter"/></typeparam>
    /// <typeparam name="TDataSyntax">type that implements <see cref="BaseDataSyntaxModel"/></typeparam>
    public interface IDataSyntaxBuilder<in TParameter, out TDataSyntax> : IDataSyntaxBuilder
        where TParameter : class, IParameter, new()
        where TDataSyntax : BaseDataSyntaxModel, new()

    {
        /// <summary>
        /// Takes enumerable of TParameter and converts into a usable format for a data source
        /// </summary>
        /// <param name="parameters">Enumerable of Parameters</param>
        /// <returns>TDataSyntax</returns>
        TDataSyntax BuildDataSyntax(IEnumerable<TParameter> parameters);
    }
}
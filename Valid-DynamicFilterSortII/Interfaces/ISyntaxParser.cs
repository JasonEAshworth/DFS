using System;
using System.Collections.Generic;
using Valid_DynamicFilterSort.Enums;

namespace Valid_DynamicFilterSort.Interfaces
{
    /// <summary>
    /// Parses text into a collection of parameters
    /// </summary>
    public interface ISyntaxParser : IDisposable
    {
        /// <summary>
        /// Collection of valid delimiters used for parsing parameters
        /// </summary>
        string[] ParameterKeyDelimiters { get; }
        
        /// <summary>
        /// A dictionary to map operators passed via string, into an OperatorEnum
        /// </summary>
        Dictionary<string, OperatorEnum> OperatorEnumMap { get; }

        /// <summary>
        /// A dictionary to map parameter combiners into an OperatorCombinerEnum
        /// </summary>
        /// <returns></returns>
        Dictionary<string, OperatorCombinerEnum> OperatorCombinerMap { get; }

        /// <summary>
        /// Parse string into IEnumerable&lt;IParameter&gt; using a specified parser
        /// </summary>
        /// <param name="parameters">string of parameters to parse</param>
        /// <typeparam name="TParameter">type of parameter to parse into</typeparam>
        /// <typeparam name="TParser">type of parser </typeparam>
        /// <typeparam name="TEntity">type of entity</typeparam>
        /// <returns>IEnumerable of TParameter</returns>
        IEnumerable<TParameter> ParseParameters<TParameter, TEntity, TParser>(string parameters)
            where TParameter : class, IParameter, new()
            where TEntity : class, new()
            where TParser : class, ISyntaxParser<TParameter>;

        /// <summary>
        /// Parse string into IEnumerable&lt;IParameter&gt; using a specified parser
        /// </summary>
        /// <param name="parameters">string of parameters to parse</param>
        /// <param name="parserType">type of parser</param>
        /// <typeparam name="TParameter">type of parameter to parse into</typeparam>
        /// <typeparam name="TEntity">type of entity</typeparam>
        /// <returns>IEnumerable of TParameter</returns>
        IEnumerable<TParameter> ParseParameters<TParameter, TEntity>(string parameters, Type parserType)
            where TParameter : class, IParameter, new()
            where TEntity : class, new();

        /// <summary>
        /// Parse string into IEnumerable&lt;IParameter&gt; using the default parser
        /// </summary>
        /// <param name="parameters">string of parameters to parse</param>
        /// <typeparam name="TParameter">type of parameter to parse into</typeparam>
        /// <typeparam name="TEntity">type of entity</typeparam>
        /// <returns>IEnumerable of TParameter</returns>
        IEnumerable<TParameter> ParseParameters<TParameter, TEntity>(string parameters)
            where TParameter : class, IParameter, new()
            where TEntity : class, new();
    }

    /// <summary>
    /// Parses text into a collection of parameters
    /// </summary>
    /// <typeparam name="TParameter"></typeparam>
    public interface ISyntaxParser<out TParameter> : ISyntaxParser where TParameter : class, IParameter
    {
        /// <summary>
        /// Parse string into IEnumerable&lt;T&gt; using the default parser
        /// </summary>
        /// <param name="parameters">string of parameters to parse</param>
        /// <typeparam name="TEntity">type of entity</typeparam>
        /// <returns>IEnumerable of TParameter</returns>
        IEnumerable<TParameter> ParseParameters<TEntity>(string parameters)
            where TEntity : class, new();
    }
}
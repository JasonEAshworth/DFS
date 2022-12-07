using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Valid_DynamicFilterSort.Base;
using Valid_DynamicFilterSort.Enums;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Interfaces.ConfigurationOptions;
using Valid_DynamicFilterSort.Models;
using Valid_DynamicFilterSort.Utilities;

namespace Valid_DynamicFilterSort.Defaults.SyntaxParser
{
    public abstract class DefaultSyntaxParserBase : ISyntaxParser
    {
        protected readonly ILogger Logger;
        protected readonly IDynamicFilterSortConfigurationValues Config;

        protected DefaultSyntaxParserBase()
        {

        }

        protected DefaultSyntaxParserBase(ILogger logger, IDynamicFilterSortConfigurationValues config)
        {
            Logger = logger;
            Config = config;
        }

        protected virtual string PartialComparisonDelimiter { get; } = "%";

        public virtual Dictionary<string, OperatorEnum> OperatorEnumMap => 
            new Dictionary<string, OperatorEnum>
            {
                {"=", OperatorEnum.EqualTo},
                {"!=", OperatorEnum.NotEqual},
                {">", OperatorEnum.GreaterThan},
                {">=", OperatorEnum.GreaterThanOrEqualTo},
                {"<", OperatorEnum.LessThan},
                {"<=", OperatorEnum.LessThanOrEqualTo}
            };

        public virtual Dictionary<string, OperatorCombinerEnum> OperatorCombinerMap =>
            new Dictionary<string, OperatorCombinerEnum>
            {
                {",", OperatorCombinerEnum.DEFAULT},
                {"&&", OperatorCombinerEnum.AND},
                {"||", OperatorCombinerEnum.OR}
            };

        public virtual IEnumerable<TParameter> ParseParameters<TParameter, TEntity, TParser>(string parameters) 
            where TParameter : class, IParameter, new()
            where TEntity : class, new()
            where TParser : class, ISyntaxParser<TParameter>
        {
            return ParseParameters<TParameter, TEntity>(parameters, typeof(TParser));
        }

        public virtual IEnumerable<TParameter> ParseParameters<TParameter, TEntity>(string parameters, Type parserType) 
            where TParameter : class, IParameter, new() 
            where TEntity : class, new()
        {
            if (parserType.GetInterfaces().Any(a => a == typeof(ISyntaxParser<TParameter>)))
            {
                throw new ArgumentException("parserType must implement ISyntaxParser<TParameter>");
            }

            if (string.IsNullOrWhiteSpace(parameters))
            {
                return new List<TParameter>();
            }
            
            try
            {
                using var parser = ServiceLocator.GetService<ISyntaxParser<TParameter>>() ??
                                   (ISyntaxParser<TParameter>) Activator.CreateInstance(parserType);

                return parser.ParseParameters<TEntity>(parameters);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error during {nameof(ParseParameters)} of {typeof(TParameter).Name}", e);
                
                if (e is InvalidOperationException || e is FormatException || e is ArgumentException)
                {
                    throw;
                }
                
                throw DynamicFilterSortErrors.PARSE_SYNTAX_GENERAL_ERROR(e.ToString());
            }
            finally
            {
                
            }
        }

        public virtual IEnumerable<TParameter> ParseParameters<TParameter, TEntity>(string parameters)
            where TParameter : class, IParameter, new()
            where TEntity : class, new()
        {
            return ParseParameters<TParameter, TEntity>(parameters, typeof(DefaultSyntaxParser));
        }
        
        protected virtual string[] SortedOperatorsArray => 
            OperatorEnumMap.Keys
                .OrderByDescending(o => o.Length)
                .ThenByDescending(t => t)
                .ToArray();

        protected virtual string SortedOperatorsRegexPattern => $"({string.Join('|', SortedOperatorsArray.Select(Regex.Escape))})";
        protected virtual Regex KeyOperatorRegex => new Regex(@$"[\&\|A-z0-9_\.]+{SortedOperatorsRegexPattern}");
        
        /// <summary>
        /// split parameters into key, operator pairs ... e.g. 'firstName=' or 'createDate>='
        /// this is needed to maintain parsing compatibility with the original dynamic filter sort
        /// in which filter strings are separated by commas, but values may also contain commas 
        /// </summary>
        /// <param name="rawStringOfParameters">Parameter String.  Original dynamic filter sort format example: 'firstname=john,lastname=smith'</param>
        /// <returns>Array of Parameter String Candidates</returns>
        protected virtual string[] SplitParameters(string rawStringOfParameters)
        {
            try
            {
                
                var keyOperators = KeyOperatorRegex.Matches(rawStringOfParameters);
                var rawParametersList = new List<string>();
                var workingStringOfParameters = rawStringOfParameters;
                for (var i = 0; i < keyOperators.Count; i++)
                {
                    var isLast = (i == keyOperators.Count - 1);
                    var parameter = workingStringOfParameters;
                    if (!isLast)
                    {
                        var thisKey = keyOperators[i];
                        var nextKey = keyOperators[i + 1];
                        workingStringOfParameters = workingStringOfParameters.Substring(thisKey.Length);
                        var posOfNextKey = workingStringOfParameters.IndexOf(nextKey.Value, StringComparison.InvariantCultureIgnoreCase);
                        parameter = thisKey.Value + workingStringOfParameters.Substring(0, posOfNextKey - 1 > 0 ? posOfNextKey -1 : 0);
                        workingStringOfParameters = workingStringOfParameters.Substring(parameter.Length + 1 - thisKey.Length);
                    }
                    rawParametersList.Add(parameter);
                }

                if (rawParametersList.Count == 0)
                {
                    throw new Exception("unable to split parameters");
                }

                return rawParametersList.ToArray();
            }
            catch (Exception e)
            {
                Logger.LogError($"Error during {nameof(SplitParameters)}; value provided: {rawStringOfParameters}", e);
                throw DynamicFilterSortErrors.PARSE_SYNTAX_GENERAL_ERROR(e.Message);
            }
            finally
            {
                
            }
        }
        
        protected virtual T CreateParameter<T>(string parameterString, int order = 0) where T: BaseParameter
        {
            try
            {
                var parameterParts = Regex.Split(parameterString, SortedOperatorsRegexPattern);

                if (parameterParts.Length != 3 ||
                    string.IsNullOrEmpty(parameterParts[0]) ||
                    string.IsNullOrEmpty(parameterParts[1]) ||
                    string.IsNullOrEmpty(parameterParts[2]))
                {
                    throw new Exception($"unable to split '{parameterString}' into separate parts");
                }

                var result = Activator.CreateInstance<T>();

                result.AddOrUpdateValue("Order", order);
                result.AddOrUpdateValue("KeyString", parameterParts[0]);
                result.AddOrUpdateValue("OperatorString", parameterParts[1]);
                result.AddOrUpdateValue("ValueString", parameterParts[2]);

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError($"Error during {nameof(CreateParameter)} for {parameterString}", e);
                
                var exception = DynamicFilterSortErrors.PARSE_SYNTAX_SINGLE_ERROR(parameterString);
                throw exception;
            }
            finally
            {
                
            }
        }

        public virtual string[] ParameterKeyDelimiters { get; } = {".", "->", "->>"};

        public virtual void Dispose()
        {
            // nothing to dispose
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Valid_DynamicFilterSort.Enums;
using Valid_DynamicFilterSort.Extensions;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Interfaces.ConfigurationOptions;
using Valid_DynamicFilterSort.Models;

namespace Valid_DynamicFilterSort.Defaults.SyntaxParser
{
    public class DefaultFilterParser : DefaultSyntaxParserBase, ISyntaxParser<FilterParameter>
    {
        private readonly IDataTypeValueHandler _dataTypeValueHandler;

        #region constructors

        public DefaultFilterParser(ILogger logger, IDynamicFilterSortConfigurationValues config, IDataTypeValueHandler dataTypeValueHandler) : base(logger, config)
        {
            _dataTypeValueHandler = dataTypeValueHandler;
        }

        #endregion
        
        #region parseParamatersHelpers
        private string[] OperatorCombinationList => OperatorCombinerMap.Keys
            .OrderByDescending(o => o.Length)
            .ThenByDescending(t => t)
            .ToArray();
        
        private void SetKeyAndCombiner(ref FilterParameter parameter)
        {
            if (!parameter.TryGetValue("KeyString", out var kso) || !(kso is string keyString))
            {
                return;
            }
            
            // determine AND/OR (defaults to DEFAULT)
            parameter.Combiner = GetCombinationOperator(keyString, OperatorCombinationList, out var match);
            parameter.Key = keyString.Substring(match?.Length ?? 0);
        }

        private void SetPartialFieldAndUpdateStringValue(ref FilterParameter parameter)
        {
            if (!parameter.TryGetValue("ValueString", out var vso) || !(vso is string valueString))
            {
                return;
            }
            
            var sw = valueString.EndsWith(PartialComparisonDelimiter);
            var ew = valueString.StartsWith(PartialComparisonDelimiter);

            parameter.ComparisonType =
                (!sw && !ew) ? ComparisonTypeEnum.Full :
                (sw && ew) ? ComparisonTypeEnum.Contains :
                (ew) ? ComparisonTypeEnum.EndsWith :
                ComparisonTypeEnum.StartsWith;

            if (ew)
            {
                valueString = valueString.Substring(1);
            }
            // because string has been modified, will need to check to see if there's
            // still a partial comparison delimiter in the ValueString
            if (valueString.EndsWith(PartialComparisonDelimiter))
            {
                valueString = valueString.Substring(0, valueString.Length - PartialComparisonDelimiter.Length);
            }

            parameter.AddOrUpdateValue("ValueString", valueString);
        }

        public void SetParameterValue(ref FilterParameter parameter)
        {
            var valueString = parameter.ValueString ?? string.Empty;
            
            // if it's an object, the best we can do (for now) is leave it as a string
            parameter.Value = parameter.PropertyInfo.Type == typeof(object) 
                ? valueString
                : _dataTypeValueHandler.ConvertString(valueString, parameter.PropertyInfo.Type);
        }
        
        #endregion parseParamatersHelpers

        public IEnumerable<FilterParameter> ParseParameters<TEntity>(string parameters) where TEntity : class, new()
        {
            var result = new List<FilterParameter>();
            var parameterArray = SplitParameters(parameters.ToLower());
            for (var i = 0; i <= parameterArray.Length - 1; i++)
            {
                var p = parameterArray[i];
                var parameter = CreateParameter<FilterParameter>(p, i);

                // set operator
                parameter.Operator = parameter.TryGetValue("OperatorString", out var osv) &&
                                     osv is string operatorString &&
                                     OperatorEnumMap.ContainsKey(operatorString)
                    ? OperatorEnumMap[operatorString]
                    : throw DynamicFilterSortErrors.PARSE_SYNTAX_INVALID_OPERATOR(parameter.GetValue("OperatorString").ToString());

                // set key and combiner
                SetKeyAndCombiner(ref parameter);

                // determine if partial
                SetPartialFieldAndUpdateStringValue(ref parameter);
                
                // validate key and set property info
                parameter.PropertyInfo =
                    PropertyExtension.GetPropertyInformation<TEntity>(parameter.Key, ParameterKeyDelimiters);

                // set value and validate value input
                SetParameterValue(ref parameter);
                
                result.Add(parameter);
            }

            return result;
        }

        private OperatorCombinerEnum GetCombinationOperator(string key, string[] ocl, out string match)
        {
            match = ocl.FirstOrDefault(key.StartsWith);
            return string.IsNullOrWhiteSpace(match) 
                ? OperatorCombinerEnum.DEFAULT 
                : OperatorCombinerMap[match];
        }
    }
}
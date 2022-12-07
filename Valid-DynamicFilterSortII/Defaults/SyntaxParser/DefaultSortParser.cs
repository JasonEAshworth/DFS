using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Valid_DynamicFilterSort.Enums;
using Valid_DynamicFilterSort.Extensions;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Interfaces.ConfigurationOptions;
using Valid_DynamicFilterSort.Models;

namespace Valid_DynamicFilterSort.Defaults.SyntaxParser
{
    public class DefaultSortParser : DefaultSyntaxParserBase, ISyntaxParser<SortParameter>
    {
        public DefaultSortParser() : base()
        {
        }

        public DefaultSortParser(ILogger logger, IDynamicFilterSortConfigurationValues config) : base(logger, config)
        {
        }

        public IEnumerable<SortParameter> ParseParameters<TEntity>(string parameters) where TEntity : class, new()
        {
            var result = new List<SortParameter>();
            var parameterArray = SplitParameters(parameters);
            for (var i = 0; i <= parameterArray.Length - 1; i++)
            {
                var p = parameterArray[i];
                var parameter = CreateParameter<SortParameter>(p, i);
                
                // for default sort parsing, only equals operator will be accepted
                if (!(parameter.TryGetValue("OperatorString", out var osv) &&
                      osv is string operatorString &&
                      OperatorEnumMap.ContainsKey(operatorString) &&
                      OperatorEnumMap[operatorString] == OperatorEnum.EqualTo))
                {
                    throw DynamicFilterSortErrors.PARSE_SYNTAX_INVALID_OPERATOR(parameter.GetValue("OperatorString").ToString());
                }
                
                // set key
                parameter.Key = parameter.TryGetValue("KeyString", out var kso) && kso is string keyString
                    ? keyString
                    : throw DynamicFilterSortErrors.PARSE_SYNTAX_SINGLE_ERROR(p);
                
                // set value as SORT DIRECTION
                parameter.Value = Enum.TryParse<SortDirectionEnum>(
                    parameter.TryGetValue("ValueString", out var vso) && vso is string valueString
                        ? valueString
                        : string.Empty, 
                    true, 
                    out var sortDirection)
                    ? sortDirection
                    : throw DynamicFilterSortErrors.PARSE_SYNTAX_INVALID_SORT_DIRECTION(p);
                
                // validate parameter key
                parameter.PropertyInfo = PropertyExtension.GetPropertyInformation<TEntity>(parameter.Key, ParameterKeyDelimiters);

                result.Add(parameter);
            }

            return result;
        }
    }
}
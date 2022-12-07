using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Valid_DynamicFilterSort.Base;
using Valid_DynamicFilterSort.Enums;
using Valid_DynamicFilterSort.Extensions;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Models;

namespace Valid_DynamicFilterSort.DynamicLinq
{
    public class DynamicLinqFilterDataSyntaxBuilder : IDataSyntaxBuilder<FilterParameter, DynamicLinqBaseDataSyntaxModel>
    {
        private ILogger _logger;
        private readonly IDataTypeValueHandler _dataTypeValueHandler;

        public DynamicLinqFilterDataSyntaxBuilder()
        {
        }

        public DynamicLinqFilterDataSyntaxBuilder(ILogger logger, IDataTypeValueHandler dataTypeValueHandler)
        {
            _logger = logger;
            _dataTypeValueHandler = dataTypeValueHandler;
        }

        public DynamicLinqBaseDataSyntaxModel BuildDataSyntax(IEnumerable<FilterParameter> parameters)
        {
            try
            {
                
                return (DynamicLinqBaseDataSyntaxModel) BuildDataSyntax<FilterParameter>(parameters);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
            finally
            {
                
            }
        }

        public void Dispose()
        {
            // nothing to dispose
        }

        public string InterfaceType { get; } = nameof(Valid_DynamicFilterSort.DynamicLinq.DynamicLinq);
        
        public BaseDataSyntaxModel BuildDataSyntax<TParameter>(IEnumerable<TParameter> parameters) where TParameter : class, IParameter, new()
        {
            try
            {
                
                const string prefix = "filter";
                var filterParameters = (parameters as IParameter[] ?? parameters.ToArray()).Cast<FilterParameter>().ToArray();

                var serverSideString = string.Empty;
                var clientSideString = string.Empty; // if this class is used as a base for another, best to leave CS/SS filters separated

                var serverSideOrder = 0;
                var clientSideOrder = 0;
            
                #region classic_grouped_parameters
                // group together parameters for automagical conjoining if not explicitly specified
                var groupedParameterList = BuildGroupedParameterList(filterParameters, 
                    p => p.Combiner == OperatorCombinerEnum.DEFAULT);
                for (var j = 0; j < groupedParameterList.Count; j++)
                {
                    var filter = string.Empty;
                    var grp = groupedParameterList[j];
                    var traversedFilter = grp.Value.First().PropertyInfo.TraversalProperty;
                    var groupedParameters = grp.Value;
                
                    for (var i = 0; i < groupedParameters.Count; i++)
                    {
                        var gpi = groupedParameters[i];
                        gpi.AddOrUpdateValue("filter_order", traversedFilter ? clientSideOrder++ : serverSideOrder++);

                        if (i == 0) filter += "((";

                        if (i > 0) //conjunction junction
                            filter += (groupedParameters[i - 1].Operator == gpi.Operator && 
                                       gpi.Operator == OperatorEnum.EqualTo)
                                ? $" {CombinerMap[OperatorCombinerEnum.OR]} "
                                : $") {CombinerMap[OperatorCombinerEnum.AND]} (";

                        filter += $"{BuildFilterComponent(ref gpi, prefix)}";

                        if (i == groupedParameters.Count - 1) filter += "))";
                    }

                    if (j < groupedParameterList.Count() - 1) filter += $" {CombinerMap[OperatorCombinerEnum.AND]} ";

                    if (traversedFilter)
                    {
                        clientSideString += filter;
                    }
                    else
                    {
                        serverSideString += filter;
                    }
                }

                #endregion classic_grouped_parameters

                #region v2

                var v2Parameters = filterParameters
                    .Where(w => w.Combiner != OperatorCombinerEnum.DEFAULT)
                    .OrderBy(o => o.Order)
                    .ToArray();

                for (var k = 0; k < v2Parameters.Length; k++)
                {
                    var parameter = v2Parameters[k];
                    var traversedFilter = parameter.PropertyInfo.TraversalProperty;
                    var needsCombiner = k > 0 || !string.IsNullOrWhiteSpace(traversedFilter ? clientSideString : serverSideString);
                    parameter.AddOrUpdateValue("filter_order", traversedFilter ? clientSideOrder++ : serverSideOrder++);
                    var combiner = needsCombiner ? $" {CombinerMap[parameter.Combiner]} " : string.Empty;
                    var filter = $"{combiner}{BuildFilterComponent(ref parameter, prefix)}";
                
                    if (traversedFilter)
                    {
                        clientSideString += filter;
                    }
                    else
                    {
                        serverSideString += filter;
                    }
                }

                #endregion v2
            
                serverSideString = !string.IsNullOrWhiteSpace(serverSideString)
                    ? $"{prefix} => {serverSideString.TrimEnd($" {CombinerMap[OperatorCombinerEnum.AND]} ".ToCharArray())}"
                    : string.Empty;
            
                clientSideString = !string.IsNullOrWhiteSpace(clientSideString)
                    ? $"{prefix} => {clientSideString.TrimEnd($" {CombinerMap[OperatorCombinerEnum.AND]} ".ToCharArray())}"
                    : string.Empty;

                var result = new DynamicLinqBaseDataSyntaxModel
                {
                    PrimarySortSyntax = serverSideString,
                    SecondarySortSyntax = clientSideString,
                    Parameters = filterParameters
                };

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError($"unable to get build data syntax of {typeof(TParameter).Name}; {e}");
                throw;
            }
            finally
            {
                
            }
        }

        /// <summary>
        /// Creates Part of a Filter String to be used by System.Core.Dynamic.Linq
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        protected virtual string BuildFilterComponent(ref FilterParameter parameter, string prefix)
        {
            try
            {
                
                var order = parameter.Fields["filter_order"];
                var keyModifier = string.Empty;
                var valueModifier = string.Empty;

                var type = parameter.PropertyInfo.Type;
                
                // if there's a json extension property in the path, we need to omit it
                if (parameter.PropertyInfo.PathHistory.Any(a => a.JsonExtensionProperty))
                {
                    parameter.PropertyInfo.PathKey = string.Join(parameter.PropertyInfo.Delimiter, parameter
                                                         .PropertyInfo.PathHistory
                                                         .Where(w => !w.JsonExtensionProperty)
                                                         .Select(s => s.Key)) +
                                                     parameter.PropertyInfo.Delimiter +
                                                     parameter.PropertyInfo.Key;
                }

                if (parameter.ComparisonType != ComparisonTypeEnum.Full)
                {
                    // on partial comparisons, we need to accept whatever value is in the value string as the value
                    parameter.Value = parameter.ValueString.ToLowerInvariant();
                    
                    // what we need to do to the data source value to get it good
                    // with what we have going on in the parameter value
                    keyModifier = type.IsDateTime() 
                        ? ".ToString(\"O\").ToLower()" 
                        : type.IsString() 
                            ? ".ToLower()" 
                            : ".ToString().ToLower()";
                }
                else if (type.IsChar() || type.IsCharArr())
                {
                    keyModifier = ".ToString().ToLower()";
                }
                else if (type.IsString())
                {
                    keyModifier = ".ToLower()";
                }

                if (!string.IsNullOrWhiteSpace(keyModifier))
                {
                    valueModifier = ".ToLower()";
                }

                var inverse = parameter.Operator == OperatorEnum.NotEqual ? "!" : string.Empty;

                if (parameter.PropertyInfo.TraversalProperty)
                {
                    return GetTraversalSyntax(ref parameter, order, prefix);
                }
                
                return parameter.ComparisonType switch
                {
                    ComparisonTypeEnum.Full => $"{prefix}.{parameter.PropertyInfo.PathKey}{keyModifier} {OperatorMap[parameter.Operator]} @{order}{valueModifier}",
                    ComparisonTypeEnum.StartsWith => $"{inverse}{prefix}.{parameter.PropertyInfo.PathKey}{keyModifier}.StartsWith( @{order}{valueModifier} )",
                    ComparisonTypeEnum.EndsWith => $"{inverse}{prefix}.{parameter.PropertyInfo.PathKey}{keyModifier}.EndsWith( @{order}{valueModifier} )",
                    ComparisonTypeEnum.Contains => $"{inverse}{prefix}.{parameter.PropertyInfo.PathKey}{keyModifier}.Contains( @{order}{valueModifier} )",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"unable to build filter component; {e}");
                throw;
            }
            finally
            {
                
            }
        }
        
        /// <summary>
        /// Groups a Parameter List by Key
        /// </summary>
        /// <param name="parameterList">list of IParameter</param>
        /// <param name="predicate">limit grouped elements via linq</param>
        /// <returns></returns>
        protected virtual List<KeyValuePair<string, List<TParameter>>> BuildGroupedParameterList<TParameter>(
            IEnumerable<TParameter> parameterList, Func<TParameter, bool> predicate = null )
            where TParameter : class, IParameter, new()
        {
            return parameterList
                .Where(predicate ?? (w => true))
                .GroupBy(g => g.Key)
                .Select(s => new KeyValuePair<string, List<TParameter>>(
                    s.Key,
                    s.OrderBy(o => o.Order).ToList())
                )
                .OrderBy(o => o.Key)
                .ToList();
        }

        /// <summary>
        /// Typically, if we're traversing a dictionary or json object, it happens client side (if there even is a server side)
        /// and special provisions have to happen to make sure the data works well enough
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="order"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        protected virtual string GetTraversalSyntax(ref FilterParameter parameter, object order, string prefix)
        {
            var operatorValue = string.Empty;
            var inverse = string.Empty;
            var isDateTime = _dataTypeValueHandler.Validate(parameter.Value.ToString().ToLowerInvariant(), DataTypeEnum.DateTime);
            
            var pathKey = isDateTime 
                ? $"(DateTime.Parse({prefix}.{parameter.PropertyInfo.PathKey}.ToString()).ToString(\"O\").Replace(\"Z\",string.Empty))" 
                : $"{prefix}.{parameter.PropertyInfo.PathKey}";

            if (isDateTime)
            { 
                parameter.Value = _dataTypeValueHandler
                        .ConvertString<DateTime>(parameter.ValueString)
                        .ToUniversalTime()
                        .ToString("O")
                        .Replace("Z", string.Empty);
            }
            
            var keyModifier = $".ToString().ToLower()";
            var valueModifier = ".ToLower()";

            if (parameter.Operator == OperatorEnum.EqualTo || parameter.Operator == OperatorEnum.NotEqual)
            {
                operatorValue = $".Equals(@{order}{valueModifier})";
            }

            if (parameter.Operator == OperatorEnum.NotEqual)
            {
                inverse = "!";
            }
            
            var result = $"{inverse}{pathKey}{keyModifier}{operatorValue}";
            return result;
        }
        
        protected virtual Dictionary<OperatorEnum, string> OperatorMap { get; } = new Dictionary<OperatorEnum, string>
        {
            {OperatorEnum.EqualTo, "="},
            {OperatorEnum.NotEqual, "!="},
            {OperatorEnum.GreaterThan, ">"},
            {OperatorEnum.GreaterThanOrEqualTo, ">="},
            {OperatorEnum.LessThan, "<"},
            {OperatorEnum.LessThanOrEqualTo, "<="},
        };
        
        protected virtual Dictionary<OperatorCombinerEnum, string> CombinerMap { get; } = new Dictionary<OperatorCombinerEnum, string>
        {
            {OperatorCombinerEnum.DEFAULT, "&&"},
            {OperatorCombinerEnum.AND, "&&"},
            {OperatorCombinerEnum.OR, "||"}
        };
    }
}
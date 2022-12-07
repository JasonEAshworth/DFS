using Dapper.FluentMap;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using valid.error.management.exceptions;
[assembly:InternalsVisibleTo("UnitTests")]
namespace Valid_DynamicFilterSort
{
    public static class DynamicFilterSort<ModelType> where ModelType : class, new()
    {
        public static bool AllowLocalFilteringFallback { get; set; } = true;

        public static void IgnoreInvalidOperationExceptionWhenFiltering(bool ignore)
        {
            AllowLocalFilteringFallback = ignore;
        }
        
        private static readonly JsonSerializerSettings LowerCaseSerializationSettings =
            new JsonSerializerSettings
            {
                ContractResolver = new LowerCasePropertyNameContractResolver(),
                TypeNameHandling = TypeNameHandling.All,
                Converters = new JsonConverter[]
                {
                    new EnsureDictionaryUsageConverter()
                }
            };

        private static readonly Dictionary<FilterSortType, string> QuoteCharLookup =
                    new Dictionary<FilterSortType, string>
                    {
                        {FilterSortType.DynamicLinq, "\""},
                        {FilterSortType.PostgreSql, "'"},
                        {FilterSortType.PostgreSqlParameterized, string.Empty}
                    };

        public enum FilterSortType
        {
            DynamicLinq,
            PostgreSql,
            PostgreSqlParameterized
        }

        /// <summary>
        /// Apply Filtering and Sorting to IQueryable
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filters"></param>
        /// <param name="sorts"></param>
        public static void ApplyFilteringAndSortingToIQueryable(ref IQueryable<ModelType> data, string filters,
            string sorts)
        {
            if (!string.IsNullOrEmpty(filters))
                ApplyFilteringToIQueryable(ref data, filters);

            if (!string.IsNullOrEmpty(sorts))
                ApplySortingToIQueryable(ref data, sorts);
        }

        /// <summary>
        /// Apply Filtering and Sorting To Pagination Model.  It saves you the trouble of Take/Skip, etc.
        /// </summary>
        /// <param name="paginatorModel"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="filters"></param>
        /// <param name="sorts"></param>
        public static void ApplyFilteringAndSortingToPaginationModel(
            ref IPaginationModel<ModelType> paginatorModel,
            ref IQueryable<ModelType> data,
            int offset,
            int count,
            string filters,
            string sorts)
        {
            ApplyFilteringAndSortingToIQueryable(ref data, filters, sorts);

            paginatorModel.offset = offset + count;
            paginatorModel.total = data.Count();
            paginatorModel.count = (count > paginatorModel.total) ? paginatorModel.total : count;
            paginatorModel.data = data.Skip(offset)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Applies Filtering to IQueryable.  BONUS: if using EF, some of the filtering is offloaded to the database instead of the app.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filters"></param>
        public static void ApplyFilteringToIQueryable(ref IQueryable<ModelType> data, string filters)
        {
            if (string.IsNullOrEmpty(filters))
            {
                return;
            }

            var (primary, secondary) = BuildFilterString(filters, FilterSortType.DynamicLinq);

            if (!string.IsNullOrEmpty(primary.FilterString))
            {
                try
                {
                    //try to see if it will trigger failure by doing a linq thing, we don't care to use the result.
                    data.Where(primary.FilterString).Any();
                    data = data.Where(primary.FilterString);
                }
                catch (InvalidOperationException e) //this is very bad, all data is filtered locally; this is here to maintain compatibility
                {
                    Console.WriteLine(e);
                    if (!AllowLocalFilteringFallback)
                    {
                        throw;
                    }
                    
                    Console.WriteLine("ERROR: HAD TO CONVERT QUERYABLE TO LIST TO FILTER/SORT");
                    data = data.ToList().AsQueryable().Where(primary.FilterString);
                }
            }

            if (!secondary.ParameterList.Any())
            {
                return;
            }

            LimitDataHavingKeysInParameterList(ref data, secondary.ParameterList);

            data = data.Where(secondary.FilterString);
        }

        /// <summary>
        /// OBSOLETE: Apply Filtering and Sorting to IQueryable
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filters"></param>
        /// <param name="sorts"></param>
        [Obsolete("This is depreciated, please use ApplyFilteringAndSortingToIQueryable instead.")]
        public static void ApplySortingAndFilteringToIQueryable(ref IQueryable<ModelType> data, string filters,
            string sorts)
        {
            ApplyFilteringAndSortingToIQueryable(ref data, filters, sorts);
        }

        /// <summary>
        /// OBSOLETE: Apply Filtering and Sorting To Pagination Model.  It saves you the trouble of Take/Skip, etc.
        /// </summary>
        /// <param name="paginatorModel"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="filters"></param>
        /// <param name="sorts"></param>
        [Obsolete("This is depreciated, please use ApplyFilteringAndSortingToPaginationModel instead.")]
        public static void ApplySortingAndFilteringToPaginationModel(
            ref IPaginationModel<ModelType> paginatorModel,
            ref IQueryable<ModelType> data,
            int offset,
            int count,
            string filters,
            string sorts)
        {
            ApplyFilteringAndSortingToPaginationModel(ref paginatorModel,
                ref data,
                offset,
                count,
                filters,
                sorts);
        }

        /// <summary>
        /// Sort an IQueryable
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sorts"></param>
        public static void ApplySortingToIQueryable(ref IQueryable<ModelType> data, string sorts)
        {
            if (string.IsNullOrEmpty(sorts))
            {
                return;
            }

            var sort = BuildSortOrderString(sorts);

            var sortArr = sort.Split(new string[] {", "}, StringSplitOptions.None);
            var sortNum = 0;
            var realSort = sortArr.Aggregate(string.Empty, (current, sa) => current + $"sort{sortNum}=>sort{sortNum++}.{sa}, ").Trim(',', ' ');

            if (!string.IsNullOrEmpty(sort))
            {
                var secondaryParameters = BuildListOfParametersFromString(sorts)
                    .Where(x => x.FilterType == FilterTypeEnum.SECONDARY)
                    .ToList();

                if (secondaryParameters.Any())
                {
                    var groupedList = BuildGroupedParameterList(secondaryParameters);
                    secondaryParameters = null;
                    ConvertModelDictionariesToLowerCaseInvariant(ref data);
                    LimitDataHavingKeysInParameterList(ref data, groupedList);
                }

                data = data.OrderBy(realSort);
            }
            else
            {
                throw new ValidBadRequestException(Errors.BAD_SORT_QUERY.Message, Errors.BAD_SORT_QUERY.Code);
            }
        }

        /// <summary>
        /// Gets filter string in specified format
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetFilterString(string filters, FilterSortType type)
        {
            if (string.IsNullOrWhiteSpace(filters))
            {
                return string.Empty;
            }

            var (primary, secondary) = Tuple.Create(new Filter(), new Filter());

            switch (type)
            {
                case FilterSortType.DynamicLinq:
                    (primary, secondary) = BuildFilterString(filters, type);
                    break;

                case FilterSortType.PostgreSql:
                    (primary, secondary) = GetPostgreSqlFilterString(filters);
                    break;
            }

            var result = primary.FilterString +
                         (primary.ParameterList.Any() && secondary.ParameterList.Any()
                             ? " AND "
                             : string.Empty) +
                         secondary.FilterString;

            return result;
        }

        /// <summary>
        /// returns postgres filter sort string with where and orderby
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="sorts"></param>
        /// <returns></returns>
        public static string GetPostgreSqlFilterSortString(string filters, string sorts, string tableAlias = "")
        {
            var(primary, secondary) = GetPostgreSqlFilterString(filters, tableAlias);
            var filterString = primary.FilterString +
                         (primary.ParameterList.Any() && secondary.ParameterList.Any()
                             ? " AND "
                             : string.Empty) +
                         secondary.FilterString;

            var sortsString = !string.IsNullOrWhiteSpace(sorts)
                ? BuildPostgreSqlSortOrderString(sorts, tableAlias)
                : string.Empty;

            var result = (!string.IsNullOrWhiteSpace(filterString) ? $" WHERE {filterString} " : string.Empty) +
                         (!string.IsNullOrWhiteSpace(sortsString) ? $" ORDER BY {sortsString}" : string.Empty);

            return result;
        }

        /// <summary>
        /// returns sql string and dynamic parameters
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="tableAlias"></param>
        /// <returns></returns>
        public static DapperDynamicQuery GetPostgreSqlParameterizedFilterString(string filters, string tableAlias = "")
        {
            var parameterPrefix = string.IsNullOrWhiteSpace(tableAlias) ? "p" : tableAlias;
            var dp = new DynamicParameters();
            var (primary, secondary) = GetPostgreSqlFilterString(filters, tableAlias, true);
            var pl = new List<Parameter>();
            pl.AddRange(primary.ParameterList.SelectMany(x=>x.Value));
            pl.AddRange(secondary.ParameterList.SelectMany(x=>x.Value));
            pl.ForEach((p) =>
            {
                var val = p.ValueObject;
                if (p.IsPartialComparison())
                {
                    val = ((string) val).Trim('%');
                }
                
                dp.Add($"@{tableAlias}{p.Id}", val);
            });
            
            var sql = primary.FilterString + 
                        (
                          primary.ParameterList.Any() && secondary.ParameterList.Any()
                          ? " AND "
                          : string.Empty
                        ) + 
                      secondary.FilterString;
            
            sql = sql.Replace("@p", $"@{parameterPrefix}");

            return new DapperDynamicQuery
            {
                Sql = sql,
                Parameters = dp
            };
        }

        /// <summary>
        /// returns postgres filter sort string with where and orderby as well as count and offset
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="sorts"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetPostgreSqlFilterSortString(string filters, string sorts, int offset, int count, string tableAlias = "")
        {
            var filterSort = GetPostgreSqlFilterSortString(filters, sorts, tableAlias);

            if (offset < 0 || count < 0)
            {
                throw new Exception("Skip/Take must be greater than 0");
            }

            var result = (!string.IsNullOrEmpty(filterSort) ? filterSort : string.Empty) +
                         ($"LIMIT {count} OFFSET {offset}");

            return result;
        }

        /// <summary>
        /// Gets sort string for FilterSortType
        /// </summary>
        /// <param name="sorts"></param>
        /// <param name="filterSortType"></param>
        /// <returns></returns>
        public static string GetSortString(string sorts, FilterSortType filterSortType)
        {
            var result = string.Empty;
            if (string.IsNullOrWhiteSpace(sorts))
            {
                return result;
            }

            switch (filterSortType)
            {
                case FilterSortType.DynamicLinq:
                    result = BuildSortOrderString(sorts);
                    break;

                case FilterSortType.PostgreSql:
                    result = BuildPostgreSqlSortOrderString(sorts);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Takes a raw string of parameters and returns a primary and secondary filter object
        /// </summary>
        /// <param name="rawStringOfParameters"></param>
        /// <param name="filterSortType"></param>
        /// <returns></returns>
        internal static (Filter primary, Filter secondary) BuildFilterString(string rawStringOfParameters,
            FilterSortType filterSortType)
        {
            var primary = new Filter { FilterType = FilterTypeEnum.PRIMARY };
            var secondary = new Filter { FilterType = FilterTypeEnum.SECONDARY };

            var prefix = string.Empty;

            var parameterList = BuildListOfParametersFromString(rawStringOfParameters);

            if (filterSortType == FilterSortType.DynamicLinq)
            {
                parameterList = parameterList.Where(x => x.IsValidConvert()).ToList();
                prefix = "filter => ";
            }
            
            var groupedParameterList = BuildGroupedParameterList(parameterList);

            for (var j = 0; j < groupedParameterList.Count; j++)
            {
                var filter = string.Empty;
                var parameters = groupedParameterList[j];
                var filterType = parameters.Value.First().FilterType;

                //check for multiple
                if (parameters.Value.Count > 1)
                {
                    var values = parameters.Value;
                    for (var i = 0; i < values.Count; i++)
                    {
                        if (i == 0) filter += "((";

                        if (i > 0) //conjunction junction
                            filter += (values[i - 1].Operator == values[i].Operator && values[i].Operator == "=")
                                ? " OR "
                                : ") AND (";

                        filter += BuildFilterComponent(values[i], filterSortType);

                        if (i == values.Count - 1) filter += "))";
                    }
                }
                else //only one
                {
                    filter += BuildFilterComponent(parameters.Value.First(), filterSortType);
                }

                if (j < groupedParameterList.Count() - 1) filter += " AND ";

                if (filterType == FilterTypeEnum.PRIMARY)
                {
                    primary.ParameterList.Add(parameters);
                    primary.FilterString += filter;
                }
                else
                {
                    secondary.ParameterList.Add(parameters);
                    secondary.FilterString += filter;
                }
            }

            primary.FilterString = !string.IsNullOrWhiteSpace(primary.FilterString)
                ? prefix + primary.FilterString.TrimEnd(" AND ".ToCharArray())
                : string.Empty;

            secondary.FilterString = !string.IsNullOrWhiteSpace(secondary.FilterString)
                ? prefix + secondary.FilterString.TrimEnd(" AND ".ToCharArray())
                : string.Empty;

            return (primary, secondary);
        }

        /// <summary>
        /// Takes a string of parameters and turns it into a format compatible with DynamicLinq
        /// </summary>
        /// <param name="rawStringOfParameters"></param>
        /// <returns></returns>
        internal static string BuildSortOrderString(string rawStringOfParameters)
        {
            var sortOrder = string.Empty;
            string[] validSortOrders = { "ASC", "DESC" };

            var parameterList = BuildListOfParametersFromString(rawStringOfParameters);

            //although technically any operator would work, why would we allow anything but an equal sign for sorting
            if (parameterList.Any(x => x.Operator != "="))
                throw new ValidBadRequestException(Errors.INVALID_OPERATOR.Message, Errors.INVALID_OPERATOR.Code);

            foreach (var parameter in parameterList)
            {
                if (Array.IndexOf(validSortOrders, parameter.Value.ToUpper()) > -1)
                    sortOrder += $"{parameter.Key} {parameter.Value.ToUpper()}, ";
                else
                    throw new ValidBadRequestException(Errors.BAD_SORT_ORDER_QUERY.Message,
                        Errors.BAD_SORT_ORDER_QUERY.Code);
            }

            sortOrder = sortOrder.Substring(0, sortOrder.Length - 2);
            return sortOrder;
        }

        /// <summary>
        /// Takes a parameter list and reduces the IQueryable of data to only items that have those keys
        /// </summary>
        /// <param name="me"></param>
        /// <param name="parameter"></param>
        internal static void ReduceDataHavingParameterKeys(ref IQueryable<ModelType> me, Parameter parameter)
        {
            var output = new ConcurrentBag<ModelType>();

            Parallel.ForEach(me, (m) =>
            {
                var actualValue = m.GetPropValue(parameter.Key);
                if (actualValue != null)
                {
                    output.Add(m);
                }
            });

            me = output.AsQueryable();
        }

        /// <summary>
        /// Takes a Parameter object and turns it into a format compatible with System.Core.Dynamic.Linq
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="filterSortType"></param>
        /// <returns></returns>
        private static string BuildFilterComponent(Parameter parameter, FilterSortType filterSortType)
        {
            var result = string.Empty;
            string quote;
           
            
            switch (filterSortType)
            {
                case FilterSortType.DynamicLinq:
                    quote = parameter.NeedsQuotes() ? QuoteCharLookup[filterSortType] : string.Empty;
                    result = BuildFilterComponentDynamicLinq(parameter, quote);
                    break;

                case FilterSortType.PostgreSql:
                    quote = parameter.NeedsQuotes() || parameter.IsDateTime()
                        ? QuoteCharLookup[filterSortType]
                        : string.Empty;
                    result = BuildFilterComponentPostgreSql(parameter, quote);
                    break;
                
                case FilterSortType.PostgreSqlParameterized:
                    quote = string.Empty;
                    result = BuildFilterComponentPostgreSql(parameter, quote, true);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Builds Filter Component for DynamicLinq
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="quote"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ValidBadRequestException"></exception>
        private static string BuildFilterComponentDynamicLinq(Parameter parameter, string quote)
        {
            var value = parameter.ValueLowerInvariant;
            var improperFormatException = new Exception("DateTime is not in a readable format");

            //if it is a datetime, or datetimeoffset, use UTC datetime string format on partial comparisons
            var toStringModifier = string.Empty;
            if (parameter.IsDateTime())
            {
                if (parameter.IsPartialComparison())
                {
                    var parameterList = new List<string>();
                    Console.WriteLine("!! PARTIAL DATETIME COMPARISON !!");
                    var dateTimeDividers = new[] {'t', ' '};
                    var dateDividers = new[] {'-', '/'};
                    var timeDividers = new[] {':', '.', 'z'};
                    var dateProps = new[] {"Year", "Month", "Day"};
                    var timeProps = new[] {"Hour", "Minute", "Second", "Millisecond"};
                    var props = dateProps.Concat(timeProps).ToArray();
                    
                    var splitOn = dateTimeDividers.Concat(dateDividers).Concat(timeDividers).ToArray();

                    var partialType = parameter.FirstCharValue == '%' && parameter.LastCharValue == '%' 
                        ? "contains" 
                        : parameter.FirstCharValue == '%' 
                            ? "ends with" 
                            : "starts with";

                    if (partialType == "contains")
                    {
                        var datePart = string.Empty;
                        var timePart = string.Empty;
                        var dtSplit = value.Split(dateTimeDividers);
                        if (dtSplit.Length == 2)
                        {
                            datePart = dtSplit[0];
                            timePart = dtSplit[1];
                        }
                        else if(value.Any(x=>dateDividers.Contains(x)))
                        {
                            datePart = value;
                        }
                        else if (value.Any(x => timeDividers.Contains(x)))
                        {
                            timePart = value;
                        }
                        else
                        {
                            parameterList = dateProps.Select(dp => $"{parameter.Key}.{dp}={value}").ToList();
                            return string.Join("||", parameterList);
                        }

                        var dateParameter = string.Empty;
                        var timeParameter = string.Empty;
                        
                        if (!string.IsNullOrWhiteSpace(datePart))
                        {
                            var dateParts = datePart.Split(dateDividers);

                            switch (dateParts.Length)
                            {
                                case 1:
                                    dateParameter = $"({parameter.Key}.Year={dateParts[0]}||{parameter.Key}.Month={dateParts[0]}||{parameter.Key}.Day={dateParts[0]})";
                                    break;
                                case 2:
                                    dateParameter = $"({parameter.Key}.Year={dateParts[0]}&&{parameter.Key}.Month={dateParts[1]})||({parameter.Key}.Month={dateParts[0]}&&{parameter.Key}.Day={dateParts[1]})";
                                    break;
                                case 3:
                                    dateParameter = $"({parameter.Key}.Year={dateParts[0]}&&{parameter.Key}.Month={dateParts[1]}&&{parameter.Key}.Day={dateParts[2]})";
                                    break;
                                default:
                                    throw improperFormatException;    
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(timePart))
                        {
                            var timeParts = timePart.Split(timeDividers);
                            var hasMillisecond = timePart.Contains('.');
                            switch (timeParts.Length)
                            {
                                case 1:
                                    timeParameter = $"({parameter.Key}.Hour={timeParts[0]}||{parameter.Key}.Minute={timeParts[0]}||{parameter.Key}.Second={timeParts[0]}||{parameter.Key}.Millisecond={timeParts[0]})";
                                    break;
                                case 2:
                                    timeParameter = $"({parameter.Key}.Hour={timeParts[0]}&&{parameter.Key}.Minute={timeParts[1]})||({parameter.Key}.Minute={timeParts[0]}&&{parameter.Key}.Second={timeParts[1]})";
                                    if (hasMillisecond)
                                    {
                                        timeParameter +=
                                            $"||({parameter.Key}.Second={timeParts[0]}&&{parameter.Key}.Millisecond={timeParts[1]})";
                                    }
                                    break;
                                case 3:
                                    if (!hasMillisecond)
                                    {
                                        timeParameter = $"({parameter.Key}.Hour={timeParts[0]}&&{parameter.Key}.Minute={timeParts[1]}&&{parameter.Key}.Second={timeParts[2]})";
                                        break;
                                    }
                                    timeParameter = $"({parameter.Key}.Hour={timeParts[0]}&&{parameter.Key}.Minute={timeParts[1]}&&{parameter.Key}.Second={timeParts[2]})||({parameter.Key}.Minute={timeParts[0]}&&{parameter.Key}.Second={timeParts[1]}&&{parameter.Key}.Millisecond={timeParts[2]})";
                                    break;
                                case 4:
                                    timeParameter = $"({parameter.Key}.Hour={timeParts[0]}&&{parameter.Key}.Minute={timeParts[1]}&&{parameter.Key}.Second={timeParts[2]}&&{parameter.Key}.Millisecond={timeParts[3]})";
                                    break;
                                default:
                                    throw improperFormatException;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(dateParameter) && string.IsNullOrWhiteSpace(timeParameter))
                        {
                            throw improperFormatException;
                        }

                        if (string.IsNullOrWhiteSpace(timeParameter))
                        {
                            return dateParameter;
                        }

                        if (string.IsNullOrWhiteSpace(dateParameter))
                        {
                            return timeParameter;
                        }

                        return $"(({dateParameter})&&({timeParameter}))";
                    }
                    
                    var reverse = partialType == "ends with";
                    
                    var splits = value.Trim('%').Split(splitOn);
                    
                    if (reverse)
                    {
                        splits = splits.Reverse().ToArray();
                        props = props.Reverse().ToArray();
                    }

                    for (var i = 0; i < splits.Length; i++)
                    {
                        parameterList.Add($"filter.{parameter.Key}.{props[i]}{parameter.Operator}{splits[i]}");
                    }

                    var parameterString = string.Join("&&", parameterList);

                    return parameterString;
                }
                
                //try to convert user input to date time
                if (DateTime.TryParse(value, out var dt))
                {
                    //because datetime comparison as strings doesn't work for > < >= <=
                    value = $"DateTime.Parse(\"{dt:O}\")";    
                }
                else if(!parameter.IsNullableType || value != "null")
                {
                    throw new Exception("DateTime Format could not be converted into a usable format.");
                }
            }

            var prefix = parameter.Key;

            var needsStringified = parameter.NeedsQuotes() && 
                                   parameter.TruePropertyType != typeof(string) &&
                                   !parameter.TruePropertyType.IsEnum;
            
            var needsLowered = needsStringified || parameter.TruePropertyType == typeof(string);
            
            if (needsStringified)
            {
                prefix += $".ToString({toStringModifier})";
            }
            
            if (needsLowered)
            {
                prefix += ".ToLower()";
            }

            var suffix = $"{parameter.Operator}{quote}{value}{quote}";

            if (parameter.LastCharValue == '%') //starts with (fast, indexed){
                suffix = $".StartsWith({quote}{value.Substring(0, value.Length - 1)}{quote})";

            if (parameter.FirstCharValue == '%') //ends with (slower)
                suffix = $".EndsWith({quote}{value.Substring(1, value.Length - 1)}{quote})";

            if (parameter.FirstCharValue == '%' && parameter.LastCharValue == '%') //contains (slower)
                suffix = $".Contains({quote}{value.Substring(1, ((value.Length < 2) ? 0 : value.Length - 2))}{quote})";

            //only equals and not equals for partial comparisons and booleans
            if (!"= !=".Split(' ').Contains(parameter.Operator) &&
                (parameter.IsPartialComparison() || Type.GetTypeCode(parameter.PropertyType) == TypeCode.Boolean))
            {
                throw new ValidBadRequestException(Errors.INVALID_FILTER_TYPE.Message, Errors.INVALID_FILTER_TYPE.Code);
            }

            prefix = "filter." + prefix;
            
            //inverse startswith/endswith
            if (parameter.Operator == "!=" && parameter.IsPartialComparison())
                prefix = $"!{prefix}";

            return $"({prefix}{suffix})";
        }

        /// <summary>
        /// Builds Filter Component for PostgreSql
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="quote"></param>
        /// <returns></returns>
        /// <exception cref="ValidBadRequestException"></exception>
        /// <exception cref="ValidNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        internal static string BuildFilterComponentPostgreSql(Parameter parameter, string quote, bool parameterized = false)
        {
            var map = FluentMapper.EntityMaps[typeof(ModelType)].PropertyMaps;
            var value = parameterized ? $"@p{parameter.Id}" : parameter.ValueLowerInvariant;
            var mapKey = parameter.Key.Contains('.') ? parameter.Key.Split('.')[0] : parameter.Key;

            if (parameter.IsPartialComparison() && !new[] { "=", "!=" }.Contains(parameter.Operator))
            {
                throw new ValidBadRequestException(Errors.INVALID_OPERATOR.Message, Errors.INVALID_OPERATOR.Code);
            }

            var field = map.FirstOrDefault(x =>
                string.Equals(mapKey, x.PropertyInfo.Name, StringComparison.InvariantCultureIgnoreCase));

            if (field == null)
            {
                throw new ValidNotFoundException(Errors.BAD_FILTER_QUERY.Message, Errors.BAD_FILTER_QUERY.Code);
            }

            var columnName = $"@tableAlias.{field.ColumnName}";
            if (parameter.Key.Contains('.'))
            {
                var k = parameter.Key;
                if (!string.IsNullOrWhiteSpace(parameter.JsonExtensionDataAttributeName))
                {
                    var keyParts = k.Split('.').ToList();
                    keyParts.RemoveAt(keyParts.FindIndex(0, x=>x.Equals(parameter.JsonExtensionDataAttributeName,StringComparison.InvariantCultureIgnoreCase)));
                    k = string.Join(".", keyParts);
                }

                var raw = $"(lower({columnName}::text)::jsonb->" +
                          $"{string.Join("->", k.ToLower().Split(new[] { '.' }, 2)[1].Split('.').Select(x => $"'{x}'"))})";

                var ind = raw.LastIndexOf("->", StringComparison.Ordinal);

                if (ind > -1)
                {
                    columnName = raw.Remove(ind, 2).Insert(ind, "->>");
                }
            }
            else
            {               
                if (parameter.IsPartialComparison() || parameter.PropertyType == typeof(Guid?) || parameter.PropertyType == typeof(Guid))
                {
                    columnName += "::text";

                    if (parameterized)
                    {
                        value = "CONCAT(" +
                                (parameter.FirstCharValue == '%' ? "'%'," : string.Empty) +
                                value +
                                (parameter.LastCharValue == '%' ? ",'%'" : string.Empty) +
                                ")";
                    }
                }
                
                columnName = parameter.ValueObject is string || parameter.ValueObject is Guid 
                    ? $"LOWER({columnName})" 
                    : columnName;
            }

            if (parameter.IsDateTime() && !parameter.IsPartialComparison())
            {
                if (!DateTime.TryParse(parameter.ValueLowerInvariant, out var dt))
                {
                    throw new Exception("DateTime Format could not be converted into a usable format.");
                }

                if (!parameterized)
                {
                    value = dt.ToString("o");
                }
            }

            if (parameterized)
            {
                quote = string.Empty;
            }
            
            var prefix = columnName;
            var oper = parameter.IsPartialComparison()
                ? parameter.Operator == "!=" 
                    ? " NOT ILIKE " 
                    : " ILIKE "
                : parameter.Operator == "!="
                    ? "<>"
                    : parameter.Operator;

            var suffix = $"{quote}{value}{quote}";
            
            return $"{prefix}{oper}{suffix}";
        }

        /// <summary>
        /// Groups a Parameter List by Key
        /// </summary>
        /// <param name="parameterList"></param>
        /// <returns></returns>
        private static List<KeyValuePair<string, List<Parameter>>> BuildGroupedParameterList(
            IEnumerable<Parameter> parameterList)
        {
            return parameterList.AsQueryable()
                .GroupBy(l => l.Key)
                .Select(g => new KeyValuePair<string, List<Parameter>>(
                    g.Key,
                    g.Select(v => v).OrderBy(o => o.Operator).ToList())
                )
                .OrderBy(o => o.Key)
                .ToList();
        }

        static readonly Regex KeyOperatorRegex = new Regex(@"[A-z0-9_\.]+[\<\>\!\=]+");
        
        /// <summary>
        /// Creates a Parameter List from string
        /// </summary>
        /// <param name="rawStringOfParameters"></param>
        /// <returns></returns>
        internal static List<Parameter> BuildListOfParametersFromString(string rawStringOfParameters)
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
                    parameter = thisKey.Value + workingStringOfParameters.Substring(0, posOfNextKey - 1);
                    workingStringOfParameters = workingStringOfParameters.Substring(parameter.Length + 1 - thisKey.Length);
                }
                rawParametersList.Add(parameter);
            }

            var rawParametersArray = rawParametersList.ToArray();
            var parametersList = new List<Parameter>();
        
            for (var i = 0; i < rawParametersArray.Length; i++)
            {
                var parameter = SplitParameterString(rawParametersArray[i]);
                parameter.Id = i.ToString();
                
                if (!string.IsNullOrEmpty(parameter.Key))
                {
                    parametersList.Add(parameter);
                }
                else
                {
                    throw new ValidBadRequestException(Errors.INVALID_FILTER_TYPE.Message, Errors.INVALID_FILTER_TYPE.Code);
                }
            }
        
            return parametersList;
        }
        

        /// <summary>
        /// Takes a string of parameters and turns it into a format compatible with PostgreSql
        /// </summary>
        /// <param name="rawStringOfParameters"></param>
        /// <returns></returns>
        /// <exception cref="ValidBadRequestException"></exception>
        /// <exception cref="ValidNotFoundException"></exception>
        public static string BuildPostgreSqlSortOrderString(string rawStringOfParameters, string tableAlias = "")
        {
            var sortOrder = string.Empty;
            if(string.IsNullOrWhiteSpace(rawStringOfParameters))
            {
                return sortOrder;
            }

            string[] validSortOrders = { "ASC", "DESC" };

            if (!FluentMapper.EntityMaps.ContainsKey(typeof(ModelType)))
            {
                throw new ValidBadRequestException(Errors.BAD_SORT_QUERY.Message, Errors.BAD_SORT_QUERY.Code);
            }

            var map = FluentMapper.EntityMaps[typeof(ModelType)].PropertyMaps;

            var parameterList = BuildListOfParametersFromString(rawStringOfParameters);

            //although technically any operator would work, why would we allow anything but an equal sign for sorting
            if (parameterList.Any(x => x.Operator != "="))
                throw new ValidBadRequestException(Errors.INVALID_OPERATOR.Message, Errors.INVALID_OPERATOR.Code);

            foreach (var parameter in parameterList)
            {
                var mapKey = parameter.Key.Contains('.') ? parameter.Key.Split('.')[0] : parameter.Key;

                var field = map.FirstOrDefault(x =>
                    string.Equals(mapKey, x.PropertyInfo.Name, StringComparison.InvariantCultureIgnoreCase));

                if (field == null)
                {
                    throw new ValidNotFoundException(Errors.BAD_SORT_QUERY.Message, Errors.BAD_SORT_QUERY.Code);
                }

                var columnName = $"{(!string.IsNullOrEmpty(tableAlias) ? tableAlias + "." : string.Empty)}{field.ColumnName}";
                
                if (parameter.Key.Contains('.'))
                {
                    var k = parameter.Key;
                    if (!string.IsNullOrWhiteSpace(parameter.JsonExtensionDataAttributeName))
                    {
                        var keyParts = k.Split('.').ToList();
                        keyParts.RemoveAt(keyParts.FindIndex(0, x => x.Equals(parameter.JsonExtensionDataAttributeName, StringComparison.InvariantCultureIgnoreCase)));
                        k = string.Join(".", keyParts);
                    }

                    var raw = $"(lower({columnName}::text)::jsonb->" +
                              $"{string.Join("->", k.ToLower().Split(new[] { '.' }, 2)[1].Split('.').Select(x => $"'{x}'"))})";

                    var ind = raw.LastIndexOf("->", StringComparison.Ordinal);

                    if (ind > -1)
                    {
                        columnName = raw.Remove(ind, 2).Insert(ind, "->>");
                    }
                }

                if (Array.IndexOf(validSortOrders, parameter.Value.ToUpper()) > -1)
                    sortOrder += $"{columnName} {parameter.Value.ToUpper()}, ";
                else
                    throw new ValidBadRequestException(Errors.BAD_SORT_ORDER_QUERY.Message,
                        Errors.BAD_SORT_ORDER_QUERY.Code);
            }

            sortOrder = sortOrder.Substring(0, sortOrder.Length - 2);
            return sortOrder;
        }

        /// <summary>
        /// Uses Newtonsoft.Json to convert all dictionary keys to lowercase
        /// </summary>
        /// <param name="data"></param>
        private static void ConvertModelDictionariesToLowerCaseInvariant(ref IQueryable<ModelType> data)
        {
            var serializedData = JsonConvert.SerializeObject(data, LowerCaseSerializationSettings);
            data = JsonConvert.DeserializeObject<IEnumerable<ModelType>>(serializedData, LowerCaseSerializationSettings)
                .AsQueryable();
            serializedData = null;
        }

        /// <summary>
        /// Gets BuildFilterString for filters for PostgreSql; Needs FluentMapper EntityMap initialized for entity type.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        /// <exception cref="ValidNotFoundException"></exception>
        private static (Filter primary, Filter secondary) GetPostgreSqlFilterString(string filters, string tableAlias = "", bool parameterized = false)
        {
            if (!FluentMapper.EntityMaps.ContainsKey(typeof(ModelType)))
            {
                throw new ValidNotFoundException(Errors.DAPPER_MAP_NOT_FOUND.Message, Errors.DAPPER_MAP_NOT_FOUND.Code);
            }

            if (string.IsNullOrEmpty(filters))
            {
                return (new Filter(), new Filter());
            }

            var sql = BuildFilterString(filters, parameterized 
                ? FilterSortType.PostgreSqlParameterized 
                : FilterSortType.PostgreSql);

            var alias = !string.IsNullOrWhiteSpace(tableAlias) ? $"{tableAlias}." : string.Empty; 
            sql.primary.FilterString = sql.primary.FilterString.Replace("@tableAlias.", alias);
            sql.secondary.FilterString = sql.secondary.FilterString.Replace("@tableAlias.", alias);

            return sql;
        }

        /// <summary>
        /// Takes a grouped parameter list and reduces the IQueryable of data to only items that have those keys
        /// </summary>
        /// <param name="data"></param>
        /// <param name="parameterList"></param>
        private static void LimitDataHavingKeysInParameterList(ref IQueryable<ModelType> data,
            IEnumerable<KeyValuePair<string, List<Parameter>>> parameterList)
        {
            foreach (var p in parameterList)
            {
                var key = p.Key;
                var values = p.Value;
                foreach (var v in values)
                {
                    ReduceDataHavingParameterKeys(ref data, v);
                }
            }
        }

        private static readonly string[] Operators = {"=", "!=", "<=", ">=", ">", "<"};
        
        /// <summary>
        /// Converts a parameter string into Parameter Object containing Key, Value, Operator, and PropertyType
        /// </summary>
        /// <param name="parameterString"></param>
        /// <returns></returns>
        internal static Parameter SplitParameterString(string parameterString)
        {
            const string pattern = @"([\<\>\!\=]+)";
            var split = Regex.Split(parameterString, pattern);

            if (split.Length != 3 || string.IsNullOrEmpty(split[0]) || string.IsNullOrEmpty(split[1]) ||
                string.IsNullOrEmpty(split[2]))
                throw new ValidBadRequestException(Errors.MISSING_VALUE_QUERY.Message, Errors.MISSING_VALUE_QUERY.Code);

            if (!Operators.Contains(split[1]))
                throw new ValidBadRequestException(Errors.INVALID_OPERATOR.Message, Errors.INVALID_OPERATOR.Code);

            var propertyType = PropertiesHelper.GetPropertyType(split[0].Trim(), typeof(ModelType));
            var key = PropertiesHelper.GetCaseSensitivePropertyNameForModelProperty(split[0].Trim(), typeof(ModelType));
            var value = split[2].Trim();
            var operatorType = split[1].Trim();
            PropertiesHelper.UsesJsonExtensionDataAttribute(split[0].Trim(), typeof(ModelType), 
                out var jsonExtensionDataAttributeName);

            var parameterType = propertyType.IsDictionary() ? FilterTypeEnum.SECONDARY : FilterTypeEnum.PRIMARY;
            var result = new Parameter(key, value, propertyType, operatorType, parameterType)
            {
                JsonExtensionDataAttributeName = jsonExtensionDataAttributeName
            };
            
            return result;
        }

        private class EnsureDictionaryUsageConverter : CustomCreationConverter<Dictionary<string, object>>
        {
            public override Dictionary<string, object> Create(Type objectType)
            {
                return new Dictionary<string, object>();
            }
        }

        private class LowerCasePropertyNameContractResolver : DefaultContractResolver
        {
            protected override string ResolvePropertyName(string propertyName)
            {
                return propertyName.ToLower();
            }
        }
    }
}
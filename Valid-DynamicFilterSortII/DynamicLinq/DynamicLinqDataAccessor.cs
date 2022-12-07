using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Valid_DynamicFilterSort.Base;
using Valid_DynamicFilterSort.Extensions;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Models;

namespace Valid_DynamicFilterSort.DynamicLinq
{
    public class DynamicLinqDataAccessor : IDataAccessor
    {
        private readonly ILogger _logger;
        private readonly IDataSyntaxBuilder _syntaxBuilder;

        public DynamicLinqDataAccessor(IEnumerable<IDataSyntaxBuilder> syntaxBuilders)
        {
            _syntaxBuilder = syntaxBuilders.FirstOrDefault(f => f is DynamicLinqDataSyntaxBuilder);
        }

        public void Dispose()
        {
            // nothing to dispose
        }
        public string InterfaceType { get; } = nameof(DynamicLinq);

        public int GetCount<TEntity>(BaseDataAccessConfiguration<TEntity> config, IEnumerable<IParameter> parameters)
            where TEntity : class, new()
        {
            try
            {
                var q = GetQueryable(config, parameters);
                var c = q.Count();
                return c;
            }
            catch (Exception e)
            {
                _logger.LogError($"unable to get count of {typeof(TEntity).Name}; {e}");
                throw;
            }
            finally
            {
                
            }
        }

        public IEnumerable<TEntity> GetEnumerable<TEntity>(BaseDataAccessConfiguration<TEntity> config, IEnumerable<IParameter> parameters, 
            int count = 10, int offset = 0) where TEntity : class, new()
        {
            try
            {
                
                var q = GetQueryable(config, parameters)
                    .Skip(offset)
                    .Take(count);
                return q.ToList();
            }
            catch (Exception e)
            {
                _logger.LogError($"unable to get enumerable of {typeof(TEntity).Name}; {e}");
                throw;
            }
            finally
            {
                
            }
        }
        
        /// <summary>
        /// When searching in dictionaries / json objects, results must be brought to the client.  If the entity model
        /// doesn't have the key, it must be pre-eliminated from the results for System.Core.Dynamic.Linq to process
        /// without a key error
        /// </summary>
        /// <param name="me"></param>
        /// <param name="parser"></param>
        /// <param name="clientParameters"></param>
        /// <typeparam name="TEntity"></typeparam>
        protected virtual void ReduceDataHavingParameterKeys<TEntity>(ref IQueryable<TEntity> me, IEnumerable<IHavePropertyInfo> clientParameters) where TEntity: class, new()
        {
            var output = new ConcurrentBag<TEntity>();

            var clientParameterArray = clientParameters as IHavePropertyInfo[] ?? clientParameters.ToArray();
            
            var parameters = clientParameterArray
                .Select(s => s.PropertyInfo.PathKey)
                .Distinct()
                .ToArray();

            var delimiters = clientParameterArray.Select(s => s.PropertyInfo.Delimiter).Distinct().ToArray();

            if (!parameters.Any())
            {
                return;
            }
            
            Parallel.ForEach(me, (m) =>
            {
                foreach (var key in parameters)
                {
                    var actualValue = GetPropValue(m, key, delimiters);
                    if (actualValue != null)
                    {
                        output.Add(m);
                    }
                }
            });

            me = output.AsQueryable();
        }
        
        protected virtual void CreatePropertiesIfNotExists<TEntity>(ref IQueryable<TEntity> me, IEnumerable<IHavePropertyInfo> clientParameters) where TEntity: class, new()
        {
            var output = new ConcurrentBag<TEntity>();

            var parameters = clientParameters as IHavePropertyInfo[] ?? clientParameters.ToArray();
            if (!parameters.Any())
            {
                return;
            }
            
            Parallel.ForEach(me, (m) =>
            {
                foreach (var p in parameters)
                {
                    var obj = m;
                    obj.CreateDefaultValueIfNotExist(p.PropertyInfo);
                    output.Add(obj);
                }
            });

            me = output.AsQueryable();
        }

        /// <summary>
        /// Gets property value of object.  Can return null if key doesn't exist.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="keyDelimiter"></param>
        /// <returns></returns>
        protected virtual object GetPropValue(object obj, string name, params string[] keyDelimiter)
        {
            if (obj == null)
            {
                return null;
            }

            var splitOn = keyDelimiter.OrderByDescending(o => o.Length).ThenByDescending(t => t).ToArray();
            foreach (var part in name.Split(splitOn, StringSplitOptions.RemoveEmptyEntries))
            {
                var type = obj.GetType();
                var info = type.GetProperty(part,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (info == null)
                {
                    if (!type.IsDictionary())
                    {
                        return null;
                    }
                    
                    if (!(obj is IDictionary<string, object> dict) || !dict.Any() || !dict.ContainsKey(part))
                        return null;

                    obj = dict[part];
                }
                else
                {
                    obj = info.GetValue(obj, null);
                }
            }

            return obj;
        }

        protected virtual IQueryable<TEntity> GetQueryable<TEntity>(BaseDataAccessConfiguration<TEntity> config, IEnumerable<IParameter> parameters)
            where TEntity : class, new()
        {
            var parameterArray = parameters as IParameter[] ?? parameters.ToArray();
            var filterParameters = parameterArray.Where(w => w is FilterParameter).Cast<FilterParameter>();
            var sortParameters = parameterArray.Where(w => w is SortParameter).Cast<SortParameter>();
            var filterDataSyntax = (DynamicLinqBaseDataSyntaxModel) _syntaxBuilder.BuildDataSyntax(filterParameters);
            var sortDataSyntax = (DynamicLinqBaseDataSyntaxModel) _syntaxBuilder.BuildDataSyntax(sortParameters);

            var queryableData = config.GetValue("DataSource") as IQueryable<TEntity>;
            
            if (!string.IsNullOrWhiteSpace(filterDataSyntax.PrimarySortSyntax))
            {
                var fParameters = filterDataSyntax.Parameters
                    .Cast<FilterParameter>()
                    .Where(w => !w.PropertyInfo.TraversalProperty)
                    .OrderBy(o => o.GetValue("filter_order"))
                    .Select(s => s.Value)
                    .ToArray();
                
                queryableData = queryableData.Where(filterDataSyntax.PrimarySortSyntax, fParameters);
            }

            if (!string.IsNullOrWhiteSpace(sortDataSyntax.PrimarySortSyntax))
            {
                queryableData = queryableData.OrderBy(sortDataSyntax.PrimarySortSyntax);
            }
            
            // System.Linq.Dynamic.Core hates it when all the dictionaries don't have the same keys
            // so this method will use the keys to limit the queryable to only dictionaries where these keys exist
            ReduceDataHavingParameterKeys(ref queryableData, 
                filterDataSyntax.Parameters
                    .Where(w=>
                        w is IHavePropertyInfo pi && 
                        pi.PropertyInfo.TraversalProperty)
                    .Cast<IHavePropertyInfo>()
                    .ToArray());

            if (!string.IsNullOrWhiteSpace(filterDataSyntax.SecondarySortSyntax))
            {
                var fParameters = filterDataSyntax.Parameters
                    .Cast<FilterParameter>()
                    .Where(w => w.PropertyInfo.TraversalProperty)
                    .OrderBy(o => o.GetValue("filter_order"))
                    .Select(s => s.Value)
                    .ToArray();

                queryableData = queryableData.ToList().AsQueryable()
                    .Where(filterDataSyntax.SecondarySortSyntax, fParameters);
            }
            
            // before client side sorting, we'll want to populate dictionary/json objects with empty key values
            // so that there's something to compare and System.Linq.Dynamic.Core doesn't throw a key error
            
            var csSortParams = sortDataSyntax.Parameters
                .Cast<SortParameter>()
                .Where(w => w.PropertyInfo.TraversalProperty)
                .OrderBy(o => o.Order)
                .ToArray();

            CreatePropertiesIfNotExists(ref queryableData, csSortParams);

            if (!string.IsNullOrWhiteSpace(sortDataSyntax.SecondarySortSyntax))
            {
                // sort the data (and potentially redo the primary sort) if needed
                queryableData = !string.IsNullOrWhiteSpace(sortDataSyntax.PrimarySortSyntax) 
                    ? queryableData.OrderBy(sortDataSyntax.PrimarySortSyntax)
                        .ThenBy(sortDataSyntax.SecondarySortSyntax) 
                    : queryableData.OrderBy(sortDataSyntax.SecondarySortSyntax);
            }

            return queryableData;
        }
    }
}
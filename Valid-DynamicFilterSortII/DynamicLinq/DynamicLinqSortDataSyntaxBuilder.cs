using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Valid_DynamicFilterSort.Base;
using Valid_DynamicFilterSort.Enums;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Models;

namespace Valid_DynamicFilterSort.DynamicLinq
{
    public class DynamicLinqSortDataSyntaxBuilder : IDataSyntaxBuilder<SortParameter, DynamicLinqBaseDataSyntaxModel>
    {
        private ILogger _logger;

        public DynamicLinqSortDataSyntaxBuilder()
        {
            
        }

        public DynamicLinqSortDataSyntaxBuilder(ILogger logger)
        {
            _logger = logger;
        }

        public virtual DynamicLinqBaseDataSyntaxModel BuildDataSyntax(IEnumerable<SortParameter> parameters)
        {
            try
            {
                
                return (DynamicLinqBaseDataSyntaxModel) BuildDataSyntax<SortParameter>(parameters);
            }
            catch (Exception e)
            {
                _logger.LogError($"unable to build data syntax for {nameof(SortParameter)}; {e}");
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
        public virtual BaseDataSyntaxModel BuildDataSyntax<TParameter>(IEnumerable<TParameter> parameters) where TParameter : class, IParameter, new()
        {
            try
            {

                var parametersArray = (parameters ?? new TParameter[]{}).Cast<SortParameter>().ToArray();

                foreach (var parameter in parametersArray)
                {
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
                }

                var result = new DynamicLinqBaseDataSyntaxModel
                {
                    SecondarySortSyntax = string.Join(", ", parametersArray
                        .Where(w => w.PropertyInfo.TraversalProperty)
                        .Select(s => $"{s.PropertyInfo.PathKey} {DirectionMap[(SortDirectionEnum) s.Value]}")),
                    PrimarySortSyntax = string.Join(", ", parametersArray
                        .Where(w => !w.PropertyInfo.TraversalProperty)
                        .Select(s => $"{s.PropertyInfo.PathKey} {DirectionMap[(SortDirectionEnum) s.Value]}")),
                    Parameters = parametersArray
                };
                
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError($"unable to build data syntax {typeof(TParameter).Name}; {e}");
                throw;
            }
            finally
            {
                
            }
        }

        protected virtual Dictionary<SortDirectionEnum, string> DirectionMap { get; } = new Dictionary<SortDirectionEnum, string>
        {
            {SortDirectionEnum.Asc, "ASC"},
            {SortDirectionEnum.Desc, "DESC"}
        };
    }
}
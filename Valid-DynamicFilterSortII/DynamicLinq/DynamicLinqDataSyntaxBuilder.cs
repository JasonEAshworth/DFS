using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Valid_DynamicFilterSort.Base;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Utilities;

namespace Valid_DynamicFilterSort.DynamicLinq
{
    public class DynamicLinqDataSyntaxBuilder : IDataSyntaxBuilder
    {
        public void Dispose()
        {
            // nothing to dispose here
        }

        private readonly ILogger _logger;

        public DynamicLinqDataSyntaxBuilder()
        {
            _logger = ServiceLocator.GetService<ILogger>();
        }
        
        public DynamicLinqDataSyntaxBuilder(ILogger logger)
        {
            _logger = logger;
        }

        public string InterfaceType { get; } = nameof(Valid_DynamicFilterSort.DynamicLinq.DynamicLinq);
        
        public virtual BaseDataSyntaxModel BuildDataSyntax<TParameter>(IEnumerable<TParameter> parameters) 
            where TParameter : class, IParameter, new()
        {
            try
            {
                var dsb = ServiceLocator
                        .GetServices<IDataSyntaxBuilder<TParameter, DynamicLinqBaseDataSyntaxModel>>()
                        .FirstOrDefault(f => 
                            f.InterfaceType == nameof(Valid_DynamicFilterSort.DynamicLinq.DynamicLinq));

                if (dsb == null)
                {
                    throw DynamicFilterSortErrors.DATA_SYNTAX_BUILDER_NO_MATCHING_SERVICE();
                }

                return dsb.BuildDataSyntax(parameters);
            }
            catch (Exception e)
            {
                _logger.LogError("error building data syntax object", e);
                throw DynamicFilterSortErrors.DATA_SYNTAX_BUILDER_GENERAL_ERROR(e.Message);
            }
            finally
            {
                
            }
        }
    }
}
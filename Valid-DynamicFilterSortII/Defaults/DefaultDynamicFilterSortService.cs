using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Valid_DynamicFilterSort;
using Valid_DynamicFilterSort.Defaults.SyntaxParser;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Interfaces.ConfigurationOptions;
using Valid_DynamicFilterSort.Models;

namespace Valid_DynamicFilterSort.Defaults
{
    public class DefaultDynamicFilterSortService : IDynamicFilterSortService
    {
        private readonly ILogger _logger;
        private readonly IDataInterfaceRegistration[] _dataInterfaces;
        private readonly IDataAccessor[] _dataAccessors;
        private readonly ISyntaxParser[] _syntaxParsers;
        private readonly IDynamicFilterSortConfigurationValues _dfsConfig;

        public DefaultDynamicFilterSortService(
            ILogger logger, 
            IEnumerable<IDataInterfaceRegistration> dataInterfaces, 
            IEnumerable<IDataAccessor> dataAccessors, 
            IEnumerable<ISyntaxParser> syntaxParsers, 
            IDynamicFilterSortConfigurationValues dfsConfig)
        {
            _logger = logger;
            _dfsConfig = dfsConfig;
            _syntaxParsers = syntaxParsers.ToArray();
            _dataInterfaces = dataInterfaces.ToArray();
            _dataAccessors = dataAccessors.ToArray();
        }

        public IPaginationModel<TEntity> GetPaginationModel<TEntity>(FilterSortRequest<TEntity> request) where TEntity : class, new()
        {
            var accessor = GetDataAccessor(request);
            var parameters = ParseParameters(request).ToArray();
            var data = accessor.GetEnumerable(request.BaseDataAccessConfiguration, parameters, request.Count, request.Offset).ToList();
            var total = accessor.GetCount(request.BaseDataAccessConfiguration, parameters);

            var pm = new PaginationModel<TEntity>
            {
                data = data,
                total = total,
                offset = request.Offset + data.Count > total ? total : request.Offset + data.Count,
                count = data.Count
            };

            return pm;
        }

        public IEnumerable<TEntity> GetEnumerable<TEntity>(FilterSortRequest<TEntity> request) where TEntity : class, new()
        {
            var accessor = GetDataAccessor(request);
            var parameters = ParseParameters(request).ToArray();
            var enumerable = accessor.GetEnumerable(request.BaseDataAccessConfiguration, parameters, request.Count, request.Offset);
            return enumerable;
        }

        public int GetCount<TEntity>(FilterSortRequest<TEntity> request) where TEntity : class, new()
        {
            var accessor = GetDataAccessor(request);
            var parameters = ParseParameters(request).ToArray();
            var count = accessor.GetCount(request.BaseDataAccessConfiguration, parameters);
            return count;
        }

        protected virtual IDataInterfaceRegistration GetDataInterfaceRegistration<TEntity>(FilterSortRequest<TEntity> request) 
            where TEntity : class, new()
        {
            var dataInterface = _dataInterfaces.FirstOrDefault(f => f.InterfaceType == request.BaseDataAccessConfiguration.InterfaceType)
                ?? throw DynamicFilterSortErrors.DATA_TYPE_INTERFACE_NOT_FOUND(request.BaseDataAccessConfiguration.InterfaceType);

            return dataInterface;
        }
        
        protected virtual IDataAccessor GetDataAccessor<TEntity>(FilterSortRequest<TEntity> request) where TEntity : class, new()
        {
            var config = GetDataInterfaceRegistration(request);
            var accessor = config.DataAccessor ?? throw DynamicFilterSortErrors.DATA_ACCESSOR_NOT_FOUND(config.InterfaceType);
            using var service = _dataAccessors.FirstOrDefault(f => f.InterfaceType == config.InterfaceType && f.GetType() == accessor);
            return service;
        }
        
        protected virtual ISyntaxParser GetSyntaxParser<TEntity>(FilterSortRequest<TEntity> request) where TEntity : class, new()
        {
            var syntaxParserCandidate = _syntaxParsers.FirstOrDefault(f => f.GetType() == request.SyntaxParser);
            var syntaxParser = syntaxParserCandidate ??
                               Activator.CreateInstance(request.SyntaxParser) as ISyntaxParser ??
                               new DefaultSyntaxParser(_logger, _dfsConfig);

            return syntaxParser;
        }

        protected virtual IEnumerable<IParameter> ParseParameters<TEntity>(FilterSortRequest<TEntity> request)
            where TEntity : class, new()
        {
            var syntaxParser = GetSyntaxParser(request);

            var filterParameters = 
                syntaxParser.ParseParameters<FilterParameter, TEntity>(request.Filter, request.SyntaxParser);
            var sortParameters =
                syntaxParser.ParseParameters<SortParameter, TEntity>(request.Sort, request.SyntaxParser);

            return filterParameters.Cast<IParameter>().Concat(sortParameters);
        }
        
    }
}
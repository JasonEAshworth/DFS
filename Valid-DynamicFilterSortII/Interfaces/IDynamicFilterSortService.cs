using System;
using System.Collections.Generic;
using Valid_DynamicFilterSort;
using Valid_DynamicFilterSort.Models;

namespace Valid_DynamicFilterSort.Interfaces
{
    public interface IDynamicFilterSortService
    {
        IPaginationModel<TEntity> GetPaginationModel<TEntity>(FilterSortRequest<TEntity> request)
            where TEntity : class, new();

        IEnumerable<TEntity> GetEnumerable<TEntity>(FilterSortRequest<TEntity> request)
            where TEntity : class, new();

        int GetCount<TEntity>(FilterSortRequest<TEntity> request)
            where TEntity : class, new();
    }
}
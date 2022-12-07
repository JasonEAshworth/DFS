using System.Collections.Generic;
using Valid_DynamicFilterSort.Base;
using Valid_DynamicFilterSort.DynamicLinq;
using Valid_DynamicFilterSort.Models;

namespace Valid_DynamicFilterSort.Interfaces
{
    public interface IDataAccessor : IHaveDataInterfaceType
    {
        int GetCount<TEntity>(BaseDataAccessConfiguration<TEntity> config, IEnumerable<IParameter> parameters)
            where TEntity : class, new();
        IEnumerable<TEntity> GetEnumerable<TEntity>(BaseDataAccessConfiguration<TEntity> config, IEnumerable<IParameter> parameters, 
            int count = 10, int offset = 0) where TEntity : class, new ();
    }
}
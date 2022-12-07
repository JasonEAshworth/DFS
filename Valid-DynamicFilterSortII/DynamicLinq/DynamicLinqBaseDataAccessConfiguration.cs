using System.Linq;
using Valid_DynamicFilterSort.Base;
using Valid_DynamicFilterSort.Defaults;

namespace Valid_DynamicFilterSort.DynamicLinq
{
    public class DynamicLinqBaseDataAccessConfiguration<TEntity> : BaseDataAccessConfiguration<TEntity> 
        where TEntity : class, new()
    {
        public DynamicLinqBaseDataAccessConfiguration()
        {
            
        }
        
        public DynamicLinqBaseDataAccessConfiguration(IQueryable<TEntity> dataSource)
        {
            DataSource = dataSource;
        }

        public override void Dispose()
        {
            // not required
        }

        public override string InterfaceType { get; } = nameof(DynamicLinq);

        public IQueryable<TEntity> DataSource { get; set; }
    }
}
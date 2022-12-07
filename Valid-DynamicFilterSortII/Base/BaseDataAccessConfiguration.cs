using Valid_DynamicFilterSort.Interfaces.ConfigurationOptions;

namespace Valid_DynamicFilterSort.Base
{
    public abstract class BaseDataAccessConfiguration<TEntity> : BaseFieldObject, IDataAccessConfiguration<TEntity> 
        where TEntity : class, new()
    {
        public abstract void Dispose();
        public abstract string InterfaceType { get; }
    }
}
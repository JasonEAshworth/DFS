namespace Valid_DynamicFilterSort.Interfaces.ConfigurationOptions
{
    public interface IDataAccessConfiguration<TEntity> : IHaveDataInterfaceType, IHaveFields
        where TEntity : class, new()
    {
        
    }
}
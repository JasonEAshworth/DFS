namespace Valid_DynamicFilterSort.Interfaces
{
    public interface IParameterPropertyValidator
    {
        bool ValidateParameterProperty<TModel>(string inputKey, out string outputKey);
    }
}
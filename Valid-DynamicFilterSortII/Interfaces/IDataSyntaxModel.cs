using System.Collections.Generic;

namespace Valid_DynamicFilterSort.Interfaces
{
    public interface IDataSyntaxModel
    {
        IEnumerable<IParameter> Parameters { get; set; }
    }
}
using System.Collections.Generic;
using Valid_DynamicFilterSort.Interfaces;

namespace Valid_DynamicFilterSort.Base
{
    public abstract class BaseDataSyntaxModel : BaseFieldObject, IDataSyntaxModel
    {
        public abstract IEnumerable<IParameter> Parameters { get; set; }
    }
}
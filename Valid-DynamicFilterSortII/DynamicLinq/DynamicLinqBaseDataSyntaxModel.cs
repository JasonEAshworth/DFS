using System.Collections.Generic;
using Valid_DynamicFilterSort.Base;
using Valid_DynamicFilterSort.Interfaces;

namespace Valid_DynamicFilterSort.DynamicLinq
{
    public class DynamicLinqBaseDataSyntaxModel : BaseDataSyntaxModel
    {
        public string PrimarySortSyntax { get; set; }
        public string SecondarySortSyntax { get; set; }
        public override IEnumerable<IParameter> Parameters { get; set; } = new List<IParameter>();
    }
}
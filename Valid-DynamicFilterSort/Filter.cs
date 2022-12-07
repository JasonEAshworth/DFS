using System;
using System.Collections.Generic;
using System.Text;

namespace Valid_DynamicFilterSort
{
    internal class Filter
    {
        public string FilterString { get; set; } = string.Empty;
        public List<KeyValuePair<string, List<Parameter>>> ParameterList { get; set; } = new List<KeyValuePair<string, List<Parameter>>>();
        public FilterTypeEnum FilterType { get; set; }
    }
}

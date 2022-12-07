using Valid_DynamicFilterSort.Interfaces;

namespace Valid_DynamicFilterSort.Base
{
    public abstract class BaseParameter : BaseFieldObject, IParameter
    {
        /// <summary>
        /// Parameter Key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Parameter Value
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// Order of parameter in parameter string
        /// </summary>
        public int Order { get; set; }
    }

}
using System.Collections.Generic;
// keeping in this namespace as other services rely on this being in this namespace
namespace Valid_DynamicFilterSort
{
    public class PaginationModel<T> : IPaginationModel<T> where T : class
    {
        public PaginationModel()
        {
            data = new List<T>();
        }

        public int count { get; set; }
        public List<T> data { get; set; }
        public int offset { get; set; }
        public int total { get; set; }
    }
}
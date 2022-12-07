using System.Collections.Generic;
// keeping in this namespace as other services rely on this being in this namespace
namespace Valid_DynamicFilterSort
{
    public interface IPaginationModel<T> where T : class
    {
        int count { get; set; }
        List<T> data { get; set; }
        int offset { get; set; }
        int total { get; set; }
    }
}
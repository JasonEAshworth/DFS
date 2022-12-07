using Microsoft.Extensions.DependencyInjection;

namespace Valid_DynamicFilterSort.Extensions
{
    public class DynamicFilterSort
    {
        public static bool IsConfigured()
        {
            return ServiceCollection != null && Configuration != null;
        }
        
        internal static IServiceCollection ServiceCollection;
        internal static DynamicFilterSortConfiguration Configuration;
    }
}
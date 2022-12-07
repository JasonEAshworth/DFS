using Microsoft.Extensions.Logging;
using Valid_DynamicFilterSort.Interfaces.ConfigurationOptions;

namespace Valid_DynamicFilterSort.Defaults.SyntaxParser
{
    public class DefaultSyntaxParser : DefaultSyntaxParserBase
    {
        public DefaultSyntaxParser(ILogger logger, IDynamicFilterSortConfigurationValues config) : base(logger, config)
        {
            
        }
    }
}
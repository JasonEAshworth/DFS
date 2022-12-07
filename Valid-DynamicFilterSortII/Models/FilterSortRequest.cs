using System;
using System.Linq;
using Valid_DynamicFilterSort.Base;
using Valid_DynamicFilterSort.Defaults;
using Valid_DynamicFilterSort.Defaults.SyntaxParser;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Interfaces.ConfigurationOptions;

namespace Valid_DynamicFilterSort.Models
{ 
    public class FilterSortRequest<TEntity> : BaseFieldObject where TEntity : class, new()
    {
        /// <summary>
        /// How many records to return after skipping the number of records specified by the Offset property;
        /// Has default of 10;
        /// </summary>
        public int Count { get; set; } = 10;
        /// <summary>
        /// How many records to skip before returning the number of records specified by the Count property;
        /// Has default of 0;
        /// </summary>
        public int Offset { get; set; } = 0;
        /// <summary>
        /// String of filter(s) to limit the results coming back from the DataAccessor;
        /// Has no default;
        /// </summary>
        public string Filter { get; set; }
        /// <summary>
        /// String of sort(s) to order the results coming back from the DataAccessor;
        /// Has no default;
        /// </summary>
        public string Sort { get; set; }
        /// <summary>
        /// Syntax Parsing Service used to parse filter(s) and sort(s);
        /// Has default of DefaultSyntaxParser
        /// </summary>
        public Type SyntaxParser { get => _syntaxParser; set => SetSyntaxParser(value); }
        /// <summary>
        /// Data Access Configuration
        /// </summary>
        public BaseDataAccessConfiguration<TEntity> BaseDataAccessConfiguration { get; set; }

        public FilterSortRequest()
        {
            
        }

        public FilterSortRequest(int offset, int count, string filter, string sort, BaseDataAccessConfiguration<TEntity> baseDataConfig, Type syntaxParser = null)
        {
            Offset = offset;
            Count = count;
            Filter = filter;
            Sort = sort;
            BaseDataAccessConfiguration = baseDataConfig;
            SyntaxParser = syntaxParser != null ? syntaxParser : SyntaxParser;
        }
        
        
        /// <summary>
        /// private property to store value of syntax parser type
        /// </summary>
        private Type _syntaxParser = typeof(DefaultSyntaxParser);
        
        /// <summary>
        /// private method for setting syntax parser type
        /// </summary>
        /// <param name="type">Type, should implement ISyntaxParser</param>
        /// <exception cref="ArgumentException">Type must implement ISyntaxParser</exception>
        private void SetSyntaxParser(Type type)
        {
            if (type.GetInterfaces().All(a => a != typeof(ISyntaxParser)))
            {
                throw new ArgumentException("Type must implement ISyntaxParser");
            }

            _syntaxParser = type;
        }
    }
}
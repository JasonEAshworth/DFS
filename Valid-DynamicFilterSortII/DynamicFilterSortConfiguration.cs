using System;
using System.Collections.Generic;
using Valid_DynamicFilterSort.Base;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Interfaces.ConfigurationOptions;

namespace Valid_DynamicFilterSort
{
    public class DynamicFilterSortConfiguration : IDynamicFilterSortConfigurationOptions, IDynamicFilterSortConfigurationValues
    {
        public ICollection<Type> SyntaxParsers { get; } = new List<Type>();
        public Dictionary<Type, Type> DefaultSyntaxParsers { get;} = new Dictionary<Type, Type>();
        public Dictionary<string, Type> DataInterfaceTypes { get; } = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

        public virtual IDynamicFilterSortConfigurationOptions AddSyntaxParser<TSyntaxParser>()
            where TSyntaxParser : class, ISyntaxParser
        {
            if (!SyntaxParsers.Contains(typeof(TSyntaxParser)))
            {
                SyntaxParsers.Add(typeof(TSyntaxParser));
            }

            return this;
        }

        public virtual IDynamicFilterSortConfigurationOptions AddSyntaxParser<TSyntaxParser, TParameter>(bool asDefault = false)
            where TSyntaxParser : class, ISyntaxParser<TParameter>
            where TParameter : class, IParameter
        {
            AddSyntaxParser<TSyntaxParser>();

            if (asDefault)
            {
                DefaultSyntaxParsers[typeof(TParameter)] = typeof(TSyntaxParser);
            }

            return this;
        }

        public IDynamicFilterSortConfigurationOptions AddDataTypeModule<TModule>() where TModule : BaseDataInterfaceRegistration, new()
        {
            using var moduleSetting = Activator.CreateInstance<TModule>();
            DataInterfaceTypes.TryAdd(moduleSetting.InterfaceType, typeof(TModule));
            return this;
        }
    }
}
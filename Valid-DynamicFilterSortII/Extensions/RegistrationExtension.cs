using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Valid_DynamicFilterSort.Defaults;
using Valid_DynamicFilterSort.Defaults.SyntaxParser;
using Valid_DynamicFilterSort.DynamicLinq;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Interfaces.ConfigurationOptions;
using Valid_DynamicFilterSort.Models;
using Valid_DynamicFilterSort.Utilities;

namespace Valid_DynamicFilterSort.Extensions
{
   
    public static class RegistrationExtension
    {
        private static bool _hasBeenConfigured;
        
        public static IServiceCollection UseDynamicFilterSort(this object obj, Action<IDynamicFilterSortConfigurationOptions> config = null)
        {
            if (_hasBeenConfigured)
            {
                throw DynamicFilterSortErrors.DFS_ALREADY_CONFIGURED();
            }
            
            // give config a default value if null
            config ??= c => { };
            
            if (obj is IServiceCollection sc)
            {
                DynamicFilterSort.ServiceCollection = sc;
            }
            else
            {
                DynamicFilterSort.ServiceCollection = new ServiceCollection();
            }
            
            // register _logger (providing one hasn't already been registered)
            if (ServiceLocator.GetService<ILogger>() == null)
            {
                var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Warning);
                    builder.AddFilter(nameof(DynamicFilterSort), LogLevel.Trace);
                });

                var logger = loggerFactory.CreateLogger<DynamicFilterSort>();

                DynamicFilterSort.ServiceCollection.TryAddSingleton(logger);
                DynamicFilterSort.ServiceCollection.TryAddSingleton<ILogger>(logger);
            }
            
            // register Data Type Value Handler (providing one hasn't already been registered)
            DynamicFilterSort.ServiceCollection.TryAddScoped<IDataTypeValueHandler, DefaultDataTypeValueHandler>();

            DynamicFilterSort.Configuration = new DynamicFilterSortConfiguration();
            config(DynamicFilterSort.Configuration);
            
            #region syntax_parsers
            
            //add defaults
            DynamicFilterSort.ServiceCollection.TryAddScoped<ISyntaxParser, DefaultSyntaxParser>();
            DynamicFilterSort.ServiceCollection.AddScoped<ISyntaxParser<FilterParameter>, DefaultFilterParser>();
            DynamicFilterSort.ServiceCollection.AddScoped<ISyntaxParser<SortParameter>, DefaultSortParser>();

            if (!DynamicFilterSort.Configuration.DefaultSyntaxParsers.ContainsKey(typeof(FilterParameter)))
            {
                DynamicFilterSort.Configuration.DefaultSyntaxParsers[typeof(FilterParameter)] = typeof(DefaultFilterParser);
            }

            if (!DynamicFilterSort.Configuration.DefaultSyntaxParsers.ContainsKey(typeof(SortParameter)))
            {
                DynamicFilterSort.Configuration.DefaultSyntaxParsers[typeof(SortParameter)] = typeof(DefaultSortParser);
            }

            foreach (var configurationSyntaxParser in DynamicFilterSort.Configuration.SyntaxParsers)
            {
                // add as ISyntaxParser
                DynamicFilterSort.ServiceCollection.AddScoped(typeof(ISyntaxParser), configurationSyntaxParser);
                
                // check (and add) if is a parser for a specific parameter type
                var iSyntaxParserT = configurationSyntaxParser.GetInterfaces().FirstOrDefault(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISyntaxParser<>));
                
                if (iSyntaxParserT != null)
                {
                    DynamicFilterSort.ServiceCollection.AddScoped(iSyntaxParserT, configurationSyntaxParser);
                }
            }
            #endregion syntax_parsers

            // registration values
            DynamicFilterSort.ServiceCollection.AddSingleton<IDynamicFilterSortConfigurationValues>(DynamicFilterSort.Configuration);

            // register data interface type modules
            DynamicFilterSort.Configuration.AddDataTypeModule<DynamicLinq.DynamicLinq>();
            foreach (var dir in DynamicFilterSort.Configuration.DataInterfaceTypes)
            {
                DynamicFilterSort.ServiceCollection.AddScoped(typeof(IDataInterfaceRegistration), dir.Value);
                using var diConfig = (IDataInterfaceRegistration) Activator.CreateInstance(dir.Value);
                HandleModuleRegistration(DynamicFilterSort.ServiceCollection, diConfig);
            }
            
            // register default dfs service
            DynamicFilterSort.ServiceCollection.TryAddScoped<IDynamicFilterSortService, DefaultDynamicFilterSortService>();
            
            _hasBeenConfigured = true;
            
            return DynamicFilterSort.ServiceCollection;
        }

        private static void HandleModuleRegistration(IServiceCollection sc, IDataInterfaceRegistration config)
        {
            
            
            // data accessor
            sc.AddScoped(typeof(IDataAccessor), config.DataAccessor);
            
            // data syntax builders
            foreach (var dsb in config.DataSyntaxBuilders)
            {
                sc.AddScoped(typeof(IDataSyntaxBuilder), dsb);
            }
            // data syntax builders for parameter type
            foreach (var dsbT in config.ParameterDataSyntaxBuilders) //TParameter, IDataSyntaxBuilder<TParameter,TDataSyntax>
            {
                 //<in Parameter, out BaseDataSyntaxModel>
                var makeGenericType = typeof(IDataSyntaxBuilder<,>).MakeGenericType(new [] {dsbT.Key, typeof(DynamicLinqBaseDataSyntaxModel)});
                sc.AddScoped(makeGenericType, dsbT.Value);
            }
        }
        
        public static string GetDataInterfaceType<T>(this T hasDataInterfaceType)
            where T : class, IHaveDataInterfaceType
        {
            if (hasDataInterfaceType != null)
            {
                return hasDataInterfaceType.InterfaceType;
            }
            
            using var hdit = Activator.CreateInstance<T>();
            return hdit.InterfaceType;
        }

        public static bool TryGetDataInterfaceType(this Type type, out string dataInterfaceType)
        {
            if (type.GetInterfaces().Any(a => a == typeof(IHaveDataInterfaceType)))
            {
                using var hdit = (IHaveDataInterfaceType) Activator.CreateInstance(type);
                dataInterfaceType = hdit.GetDataInterfaceType();
                return true;
            }

            dataInterfaceType = null;
            return false;
        }
    }
}
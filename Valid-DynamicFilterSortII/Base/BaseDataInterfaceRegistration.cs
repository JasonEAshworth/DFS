using System;
using System.Collections.Generic;
using System.Linq;
using Valid_DynamicFilterSort.Interfaces;

namespace Valid_DynamicFilterSort.Base
{
    public abstract class BaseDataInterfaceRegistration : BaseFieldObject, IDataInterfaceRegistration
    {
        public virtual void Dispose()
        {
            // empty
        }

        public abstract string InterfaceType { get; }
        public ICollection<Type> DataSyntaxBuilders { get; } = new List<Type>();
        public Dictionary<Type, Type> ParameterDataSyntaxBuilders { get; } = new Dictionary<Type, Type>();
        public Type DataAccessor { get; private set; }

        protected virtual void AddDataSyntaxBuilder<TSyntaxBuilder>()
            where TSyntaxBuilder : class, IDataSyntaxBuilder, new()
        {
            using var sb = new TSyntaxBuilder();
            if (!sb.InterfaceType.Equals(InterfaceType, StringComparison.InvariantCultureIgnoreCase))
            {
                throw DynamicFilterSortErrors.DATA_INTERFACE_CONFIG_TYPE_MISMATCH(sb.InterfaceType + ":" + InterfaceType);
            }

            var syntaxBuilderType = typeof(TSyntaxBuilder);
            var syntaxBuilderInterfaceForParameterType = syntaxBuilderType
                .GetInterfaces()
                .FirstOrDefault(f =>
                f.IsGenericType &&
                f.GetGenericTypeDefinition() == typeof(IDataSyntaxBuilder<,>));

            if (syntaxBuilderInterfaceForParameterType == null)
            {
                DataSyntaxBuilders.Add(typeof(TSyntaxBuilder));
            }
            else
            {
                var tParameter = syntaxBuilderInterfaceForParameterType.GenericTypeArguments[0];
                ParameterDataSyntaxBuilders.TryAdd(tParameter, syntaxBuilderType);
            }
        }

        protected virtual void UseDataAccessor<TDataAccessor>() where TDataAccessor : class, IDataAccessor
        {
            DataAccessor = typeof(TDataAccessor);
        }
    }
}
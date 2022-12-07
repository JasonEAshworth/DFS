using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Valid_DynamicFilterSort.Extensions;

namespace Valid_DynamicFilterSort.Utilities
{
    public static class ServiceLocator
    {
        public static T[] GetServices<T>()
        {
            if (DynamicFilterSort.ServiceCollection == null)
            {
                return new T[]{};
            }
            
            using var provider = DynamicFilterSort.ServiceCollection.BuildServiceProvider();
            var type = typeof(T);
            var services = provider.GetServices(type).Cast<T>();

            if (!services.Any())
            {
                return new T[]{};
            }
            
            var servicesArray = services as T[] 
                                ?? (services
                                    ?? new List<T>()).ToArray();

            return servicesArray;
        }
        
        public static T GetService<T>()
        {
            if (DynamicFilterSort.ServiceCollection == null)
            {
                return default;
            }
            
            using var provider = DynamicFilterSort.ServiceCollection?.BuildServiceProvider();
            var service = provider?.GetService(typeof(T));

            if (service == null)
            {
                return default;
            }
            
            return (T) service;
        }

        public static object GetService(this object me, Type serviceType)
        {
            var method = typeof(ServiceLocator).GetMethods().FirstOrDefault(x => x.IsGenericMethod && x.Name == "GetService");

            if (method == null)
            {
                return null;
            }
            
            var genericMethod = method.MakeGenericMethod(serviceType);

            var result = genericMethod.Invoke(me, new object[]{});

            return result;
        }
    }
}
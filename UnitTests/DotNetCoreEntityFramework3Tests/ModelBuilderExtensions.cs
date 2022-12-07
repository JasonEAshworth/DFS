using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Valid_DynamicFilterSort;

namespace UnitTests.DotNetCoreEntityFramework3Tests.Extensions
{
    public static class JsonModelBuilderExtensions
    {
        public static void AddJsonField<T>(this ModelBuilder modelBuilder, Expression<Func<T, Json>> property)
            where T : class
        {
            modelBuilder.Entity<T>().Property(property).HasConversion(
                v => v.ToString(true),
                v => JsonConvert.DeserializeObject<Json>(v)
            );
        }
    }
}
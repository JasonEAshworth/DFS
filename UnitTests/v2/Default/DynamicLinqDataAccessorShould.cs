using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using UnitTests.v2.Data;
using UnitTests.v2.Models;
using Valid_DynamicFilterSort.DynamicLinq;
using Valid_DynamicFilterSort.Extensions;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Models;
using Valid_DynamicFilterSort.Utilities;
using Xunit;

namespace UnitTests.v2.Default
{
    public class DynamicLinqDataAccessorShould : TestBase
    {
        public DynamicLinqDataAccessorShould()
        {
            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }
        }
        
        [Fact]
        public void AccessAndFilterDataAsEnumerable()
        {
            var dataAccessor = ServiceLocator.GetServices<IDataAccessor>().
                FirstOrDefault(f => f.InterfaceType == nameof(DynamicLinq));
            
            var testData = new TestData();
            
            var dataConfig = new DynamicLinqBaseDataAccessConfiguration<TestGroupModel>
            {
                DataSource = testData.Groups.AsQueryable()
            };

            var parameters = ServiceLocator.GetService<ISyntaxParser<FilterParameter>>().ParseParameters<TestGroupModel>("GroupName=FireNation");

            var data = dataAccessor?.GetEnumerable(dataConfig, parameters).FirstOrDefault();
            
            Assert.Equal(3, data?.Members.Count);
        }

        [Fact]
        public void AccessAndFilterDataAsCount()
        {
            var dataAccessor = ServiceLocator.GetServices<IDataAccessor>().
                FirstOrDefault(f => f.InterfaceType == nameof(DynamicLinq));
            
            var testData = new TestData();
            
            var dataConfig = new DynamicLinqBaseDataAccessConfiguration<TestGroupModel>
            {
                DataSource = testData.Groups.AsQueryable()
            };
            
            var parameters = ServiceLocator.GetService<ISyntaxParser<FilterParameter>>().ParseParameters<TestGroupModel>("GroupName=%Fire%");

            var count = dataAccessor?.GetCount(dataConfig, parameters);
            
            Assert.Equal(2, count);
        }

        [Fact]
        public void SuccessfullyOrderResults()
        {
            var dataAccessor = ServiceLocator.GetServices<IDataAccessor>().
                FirstOrDefault(f => f.InterfaceType == nameof(DynamicLinq));
            
            var testData = new TestData();
            var expected = testData.Groups.Select(s => s.GroupName).OrderBy(o => o).ToArray();
            
            var dataConfig = new DynamicLinqBaseDataAccessConfiguration<TestGroupModel>
            {
                DataSource = testData.Groups.AsQueryable()
            };

            var parameters = ServiceLocator.GetService<ISyntaxParser<SortParameter>>().ParseParameters<TestGroupModel>("GroupName=ASC");

            var actual = dataAccessor?.GetEnumerable(dataConfig, parameters).ToArray();
            Assert.NotNull(actual);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], actual[i].GroupName);
            }
        }
        
        [Fact]
        public void GoIntoSecondTierData()
        {
            var dataAccessor = ServiceLocator.GetServices<IDataAccessor>().
                FirstOrDefault(f => f.InterfaceType == nameof(DynamicLinq));
            
            var testData = new TestData();
            
            var dataConfig = new DynamicLinqBaseDataAccessConfiguration<TestPersonModel>
            {
                DataSource = testData.Persons.AsQueryable()
            };
            
            var parameters = 
                ServiceLocator.GetService<ISyntaxParser<FilterParameter>>().ParseParameters<TestPersonModel>("LastName=string.Empty,&&Fields.bender=false")
                    .Cast<IParameter>()
                    .Concat(ServiceLocator.GetService<ISyntaxParser<SortParameter>>().ParseParameters<TestPersonModel>("FirstName=ASC"));
            
            var actual = dataAccessor?.GetEnumerable(dataConfig, parameters, 100, 0);
            
            Assert.NotNull(actual);
            var testPersonModels = actual as TestPersonModel[] ?? actual.ToArray();
            Assert.Single(testPersonModels);
            Assert.Equal("Sokka", testPersonModels.First().FirstName, StringComparer.InvariantCultureIgnoreCase);
        }
        
        [Fact]
        public void GoIntoSecondTierDataAdvanced()
        {
            var dataAccessor = ServiceLocator.GetServices<IDataAccessor>().
                FirstOrDefault(f => f.InterfaceType == nameof(DynamicLinq));
            
            var testData = new TestData();
            
            DynamicLinqBaseDataAccessConfiguration<TestPersonModel> GetDataConfig()
            {
                return new DynamicLinqBaseDataAccessConfiguration<TestPersonModel>
                {
                    DataSource = testData.Persons.AsQueryable()
                };
            }

            IEnumerable<TestPersonModel> GetResultOrDefault(string filter)
            {
                var parameters = ServiceLocator
                    .GetService<ISyntaxParser<FilterParameter>>()
                    .ParseParameters<TestPersonModel>(filter)
                    .Cast<IParameter>()
                    .Concat(ServiceLocator
                        .GetService<ISyntaxParser<SortParameter>>()
                        .ParseParameters<TestPersonModel>("FirstName=ASC"));
                
                return dataAccessor?.GetEnumerable(GetDataConfig(), parameters, 100 , 0);
            }

            Assert.Single(GetResultOrDefault($"Fields.datetime={DateTime.UnixEpoch:O}"));
            Assert.Single(GetResultOrDefault($"Fields.guid={Guid.Empty}"));
            Assert.Single(GetResultOrDefault($"Fields.int=42"));
            Assert.Single(GetResultOrDefault($"Fields.string=forty-two"));
            Assert.Single(GetResultOrDefault($"Fields.DOUBLE=42"));
            Assert.Single(GetResultOrDefault($"Fields.DECIMAL=42"));
            Assert.Single(GetResultOrDefault($"Fields.float=42"));

        }
        
    }
}
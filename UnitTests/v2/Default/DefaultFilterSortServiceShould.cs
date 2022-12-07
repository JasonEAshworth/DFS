using System.Linq;
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
    public class DefaultFilterSortServiceShould
    {
        public DefaultFilterSortServiceShould()
        {
            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }
        }
        
        private readonly TestData _testData = new TestData();

        private DynamicLinqBaseDataAccessConfiguration<TestPersonModel> GetPersonDataConfig()
        {
            return new DynamicLinqBaseDataAccessConfiguration<TestPersonModel>
            {
                DataSource = _testData.Persons.AsQueryable()
            };
        }

        [Fact]
        public void ReturnResultsWithNoFiltersOrSorts()
        {
            var service = ServiceLocator.GetService<IDynamicFilterSortService>();
            var request = new FilterSortRequest<TestPersonModel>(0, 3, "", "", GetPersonDataConfig());
            
            Assert.Equal(_testData.Persons.Count, service.GetCount(request));
            Assert.Equal(3, service.GetEnumerable(request).Count());
            Assert.Equal(_testData.Persons.Count, service.GetPaginationModel(request).total);
            Assert.Equal(3, service.GetPaginationModel(request).count);
        }

        [Fact]
        public void AcceptsPrimaryFilter()
        {
            var service = ServiceLocator.GetService<IDynamicFilterSortService>();
            var request = new FilterSortRequest<TestPersonModel>(0, 3, "lastname!=string.Empty", "", GetPersonDataConfig());
            var results = service.GetEnumerable(request).ToList();
            Assert.Equal(_testData.Persons.Count(c=>c.LastName != string.Empty), results.Count);
            Assert.Contains("Toph", results.Select(s => s.FirstName));
        }
        
        [Fact]
        public void AcceptsSecondaryFilter()
        {
            var service = ServiceLocator.GetService<IDynamicFilterSortService>();
            var request = new FilterSortRequest<TestPersonModel>(0, 3, "Fields.int=42", "", GetPersonDataConfig());
            var results = service.GetEnumerable(request).ToList();
            Assert.Equal(_testData.Persons.Count(c=>c.LastName != string.Empty), results.Count);
            Assert.Contains("Sokka", results.Select(s => s.FirstName));
        }

        [Fact]
        public void AcceptsPrimarySort()
        {
            var service = ServiceLocator.GetService<IDynamicFilterSortService>();
            var request = new FilterSortRequest<TestPersonModel>(0, 3, "", "firstname=desc", GetPersonDataConfig());
            var results = service.GetEnumerable(request).ToArray();
            var expected = _testData.Persons.OrderByDescending(o => o.FirstName).Take(3).ToArray();
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i].FirstName, results[i].FirstName);
            }
        }
        
        [Fact]
        public void AcceptsSecondarySort()
        {
            var service = ServiceLocator.GetService<IDynamicFilterSortService>();
            var request = new FilterSortRequest<TestPersonModel>(0, 10, "", "fields.order=desc", GetPersonDataConfig());
            var results = service.GetEnumerable(request).ToArray();
            var expected = _testData.Persons.OrderByDescending(x => x.Fields["order"]).ToArray();
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i].Fields["order"], results[i].Fields["order"]);
            }
        }
        
        
    }
}
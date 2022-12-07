using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnitTests.DotNetCoreEntityFramework3Tests.Extensions;
using UnitTests.v2.Data;
using Valid_DynamicFilterSort;
using Valid_DynamicFilterSort.DynamicLinq;
using Valid_DynamicFilterSort.Extensions;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Models;
using Valid_DynamicFilterSort.Utilities;
using Xunit;

namespace UnitTests.DotNetCoreEntityFramework3Tests
{
    public class TestContext : DbContext
    {
        private const string ConnectionString = "Host=localhost;Database=dfs_test;Username=postgres;Password=postgres";
        private DbSet<TestEntity> TestEntities { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            optionsBuilder.UseNpgsql(ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddJsonField<TestEntity>(x => x.Json);
            base.OnModelCreating(modelBuilder);
        }
    }
    
    public class EF3Tests
    {
        private readonly ServiceCollection _services = new ServiceCollection();
        private readonly TestContext _context = new TestContext();

        public EF3Tests()
        {
        }

        private readonly Guid GuidOne = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private void Setup()
        {
            DynamicFilterSort<TestEntity>.AllowLocalFilteringFallback = false;
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            var entities = new List<TestEntity>
            {
                new TestEntity(),
                new TestEntity
                {
                    BoolProp = true,
                    DateTimeProp = DateTime.UtcNow.AddDays(-1),
                    DblProp = 6.022,
                    Id = GuidOne,
                    Integer = 666,
                    Json = new Json(),
                    NullableBoolean = true,
                    NullableDateTime = DateTime.UtcNow.Date,
                    NullableDouble = 4.21,
                    NullableGuid = GuidOne,
                    NullableInteger = 777,
                    Text = "Work"
                }
            };

            _context.AddRangeAsync(entities);
            _context.SaveChanges();
        }

        private void Teardown()
        {
            DynamicFilterSort<TestEntity>.AllowLocalFilteringFallback = true;
            var entities = _context.Set<TestEntity>().ToList();
            _context.Set<TestEntity>().RemoveRange(entities);
            _context.SaveChanges();
        }

        [Fact (Skip = "requires database")]
        public void SetupWorks()
        {
            Setup();
            Assert.Equal(2, _context.Set<TestEntity>().Count());
            Teardown();
        }
        
        [Fact (Skip = "requires database")]
        public void DynamicLinqWorksText()
        {
            Setup();

            var filter = "text=work";

            var fs = DynamicFilterSort<TestEntity>.GetFilterString(filter,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);
            
            var queryable = _context.Set<TestEntity>()
                .Where(fs);
            
            Assert.Equal(1, queryable.Count());
            Teardown();
        }
        
        [Fact (Skip = "requires database")]
        public void DynamicLinqWorksTextV2()
        {
            Setup();

            var filter = "text=work";

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<TestEntity>(0, 100, "text=work", string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(_context.Set<TestEntity>()));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);
            
            Assert.Single(result);
            Teardown();
        }
        
        [Fact(Skip = "requires database")]
        public void DynamicLinqWorksIntegerV2()
        {
            Setup();

            var filter = "integer=42";
            var filter2 = "NullableInteger=null";
            var filter3 = "integer=4%";
            
            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request1 = new FilterSortRequest<TestEntity>(0, 100, filter, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(_context.Set<TestEntity>()));
            
            var request2 = new FilterSortRequest<TestEntity>(0, 100, filter2, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(_context.Set<TestEntity>()));
            
            var request3 = new FilterSortRequest<TestEntity>(0, 100, filter3, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(_context.Set<TestEntity>()));

            var service = ServiceLocator.GetService<IDynamicFilterSortService>();

            Assert.Equal(1, service.GetCount(request1));
            Assert.Equal(1, service.GetCount(request2));
            Assert.Equal(1, service.GetCount(request3));
            Teardown();
        }
        
        [Fact (Skip = "requires databse")]
        public void DynamicLinqWorksInteger()
        {
            Setup();

            var filter = "integer=42";
            var filter2 = "NullableInteger=null";
            var filter3 = "integer=4%";
            
            var fs = DynamicFilterSort<TestEntity>.GetFilterString(filter,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);
            var fs2 = DynamicFilterSort<TestEntity>.GetFilterString(filter2,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);
            var fs3 = DynamicFilterSort<TestEntity>.GetFilterString(filter3,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);

            var queryable = _context.Set<TestEntity>()
                .Where(fs);
            
            var queryable2 = _context.Set<TestEntity>()
                .Where(fs2);
            
            var queryable3 = _context.Set<TestEntity>()
                .Where(fs3);
            
            Assert.Equal(1, queryable.Count());
            Assert.Equal(1, queryable2.Count());
            Assert.Equal(1, queryable3.Count());
            Teardown();
        }
        
        [Fact (Skip = "requires database")]
        public void DynamicLinqWorksBoolean()
        {
            Setup();

            var filter = "BoolProp=true";
            var filter2 = "NullableBoolean=null";
            var filter3 = "boolprop=t%";
            
            var fs = DynamicFilterSort<TestEntity>.GetFilterString(filter,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);
            var fs2 = DynamicFilterSort<TestEntity>.GetFilterString(filter2,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);
            var fs3= DynamicFilterSort<TestEntity>.GetFilterString(filter3,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);

            var queryable = _context.Set<TestEntity>()
                .Where(fs);
            
            var queryable2 = _context.Set<TestEntity>()
                .Where(fs2);
            
            var queryable3 = _context.Set<TestEntity>()
                .Where(fs3);
            
            Assert.Equal(1, queryable.Count());
            Assert.Equal(1, queryable2.Count());
            Assert.Equal(1, queryable3.Count());
            Teardown();
        }
        
        [Fact (Skip = "requires database")]
        public void DynamicLinqWorksBooleanV2()
        {
            Setup();

            var filter = "BoolProp=true";
            var filter2 = "NullableBoolean=null";
            var filter3 = "boolprop=t%";
            
            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request1 = new FilterSortRequest<TestEntity>(0, 100, filter, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(_context.Set<TestEntity>()));
            
            var request2 = new FilterSortRequest<TestEntity>(0, 100, filter2, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(_context.Set<TestEntity>()));
            
            var request3 = new FilterSortRequest<TestEntity>(0, 100, filter3, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(_context.Set<TestEntity>()));

            var service = ServiceLocator.GetService<IDynamicFilterSortService>();
            
            Assert.Equal(1, service.GetCount(request1));
            Assert.Equal(1, service.GetCount(request2));
            Assert.Equal(1, service.GetCount(request3));
            Teardown();
        }
        
        [Fact (Skip = "requires database")]
        public void DynamicLinqWorksDouble()
        {
            Setup();

            var filter = "DblProp=3.14";
            var filter2 = "NullableDouble=null";
            var filter3 = "DblProp>3.14";
            
            var fs = DynamicFilterSort<TestEntity>.GetFilterString(filter,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);
            var fs2 = DynamicFilterSort<TestEntity>.GetFilterString(filter2,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);
            var fs3 = DynamicFilterSort<TestEntity>.GetFilterString(filter3,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);

            var queryable = _context.Set<TestEntity>()
                .Where(fs);
            
            var queryable2 = _context.Set<TestEntity>()
                .Where(fs2);
            
            var queryable3 = _context.Set<TestEntity>()
                .Where(fs3);
            
            Assert.Equal(1, queryable.Count());
            Assert.Equal(1, queryable2.Count());
            Assert.Equal(1, queryable3.Count());
            
            Teardown();
        }
        
        [Fact (Skip = "requires database")]
        public void DynamicLinqWorksDoubleV2()
        {
            Setup();

            var filter = "DblProp=3.14";
            var filter2 = "NullableDouble=null";
            var filter3 = "DblProp>3.14";
            
            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }
            
            var request1 = new FilterSortRequest<TestEntity>(0, 100, filter, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(_context.Set<TestEntity>()));
            
            var request2 = new FilterSortRequest<TestEntity>(0, 100, filter2, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(_context.Set<TestEntity>()));
            
            var request3 = new FilterSortRequest<TestEntity>(0, 100, filter3, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(_context.Set<TestEntity>()));

            var service = ServiceLocator.GetService<IDynamicFilterSortService>();
            
            Assert.Equal(1, service.GetCount(request1));
            Assert.Equal(1, service.GetCount(request2));
            Assert.Equal(1, service.GetCount(request3));
            
            Teardown();
        }
        
        [Fact (Skip = "requires database")] //TODO: REVISIT CASE 3 AND 4
        public void DynamicLinqWorksDateTimeV2()
        {
            Setup();

            var filter = $"DateTimeProp>{DateTime.UtcNow.AddHours(-1):O},DateTimeProp<={DateTime.UtcNow.AddHours(1)}";
            var filter2 = "NullableDateTime=null";
            var filter3 = $"DateTimeProp={DateTime.UtcNow.Year}-{DateTime.UtcNow.Month.ToString().PadLeft(2,'0')}-{DateTime.UtcNow.Day.ToString().PadLeft(2,'0')}%";
            var filter4 = $"DateTimeProp>{DateTime.UtcNow.Year}%";
            
            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }
            
            var request1 = new FilterSortRequest<TestEntity>(0, 100, filter, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(new TestContext().Set<TestEntity>()));
            
            var request2 = new FilterSortRequest<TestEntity>(0, 100, filter2, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(new TestContext().Set<TestEntity>()));
            
            var request3 = new FilterSortRequest<TestEntity>(0, 100, filter3, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(new TestContext().Set<TestEntity>()));
            
            var request4 = new FilterSortRequest<TestEntity>(0, 100, filter4, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(new TestContext().Set<TestEntity>()));

            var service = ServiceLocator.GetService<IDynamicFilterSortService>();

            Assert.Equal(1, service.GetCount(request1));
            Assert.Equal(1, service.GetCount(request2));
            // Assert.Equal(1, service.GetCount(request3));
            // Assert.Equal(1, service.GetCount(request4));
            Teardown();
        }
        
        [Fact (Skip = "requires database")]
        public void DynamicLinqWorksDateTime()
        {
            Setup();

            var filter = $"DateTimeProp>{DateTime.UtcNow.AddHours(-1):O},DateTimeProp<={DateTime.UtcNow.AddHours(1)}";
            var filter2 = "NullableDateTime=null";
            var filter3 = $"DateTimeProp={DateTime.UtcNow.Year}-{DateTime.UtcNow.Month}-{DateTime.UtcNow.Day}%";
            var filter4 = $"DateTimeProp>{DateTime.UtcNow.Year}%";
            
            var fs = DynamicFilterSort<TestEntity>.GetFilterString(filter,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);
            var fs2 = DynamicFilterSort<TestEntity>.GetFilterString(filter2,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);
            var fs3 = DynamicFilterSort<TestEntity>.GetFilterString(filter3,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);
            var fs4 = DynamicFilterSort<TestEntity>.GetFilterString(filter4,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);

            _context.Set<TestEntity>().Add(new TestEntity {DateTimeProp = DateTime.UtcNow.AddYears(1), NullableDateTime = DateTime.UtcNow});
            _context.SaveChanges();
            
            var queryable = _context.Set<TestEntity>()
                .Where(fs);
            var queryable2 = _context.Set<TestEntity>()
                .Where(fs2);
            var queryable3 = _context.Set<TestEntity>()
                .Where(fs3);
            var queryable4 = _context.Set<TestEntity>()
                .Where(fs4);

            Assert.Equal(1, queryable.Count());
            Assert.Equal(1, queryable2.Count());
            Assert.Equal(1, queryable3.Count());
            Assert.Equal(1, queryable4.Count());
            Teardown();
        }
        
        [Fact (Skip = "requires database")]
        public void DynamicLinqWorksGuid()
        {
            Setup();

            var filter = $"Id={GuidOne}";
            var filter2 = "NullableGuid=null";
            var filter3 = $"Id!={GuidOne}%";
            
            var fs = DynamicFilterSort<TestEntity>.GetFilterString(filter,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);
            var fs2 = DynamicFilterSort<TestEntity>.GetFilterString(filter2,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);
            var fs3 = DynamicFilterSort<TestEntity>.GetFilterString(filter3,
                DynamicFilterSort<TestEntity>.FilterSortType.DynamicLinq);

            var queryable = _context.Set<TestEntity>()
                .Where(fs);
            
            var queryable2 = _context.Set<TestEntity>()
                .Where(fs2);
            
            var queryable3 = _context.Set<TestEntity>()
                .Where(fs3);
            
            Assert.Equal(1, queryable.Count());
            Assert.Equal(1, queryable2.Count());
            Assert.Equal(1, queryable3.Count());
            Teardown();
        }
        
        [Fact (Skip = "requires database")]
        public void DynamicLinqWorksGuidV2()
        {
            Setup();

            var filter = $"Id={GuidOne}";
            var filter2 = "NullableGuid=null";
            var filter3 = $"Id!={GuidOne}%";

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var service = ServiceLocator.GetService<IDynamicFilterSortService>();

            var request1 = new FilterSortRequest<TestEntity>(0, 100, filter, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(_context.Set<TestEntity>()));
            
            var request2 = new FilterSortRequest<TestEntity>(0, 100, filter2, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(_context.Set<TestEntity>()));
            
            var request3 = new FilterSortRequest<TestEntity>(0, 100, filter3, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(_context.Set<TestEntity>()));
            
            Assert.Single(service.GetEnumerable(request1));
            Assert.Single(service.GetEnumerable(request2));
            Assert.Single(service.GetEnumerable(request3));
            Teardown();
        }

        [Fact (Skip = "requires database")]
        public void PlayNiceWithSimpleDTOs()
        {
            Setup();
            var filter = $"id={GuidOne}";
            var queryable = _context.Set<TestEntity>().Where(x => x.Id != null).
                Select(x => new TestDto()
                {
                    BoolProp = x.BoolProp,
                    DateTimeProp = x.DateTimeProp,
                    DblProp = x.DblProp,
                    Id = x.Id,
                    Status = x.Status,
                    NullableBoolean = x.NullableBoolean,
                    NullableDouble = x.NullableDouble,
                    NullableGuid = x.NullableGuid ?? x.Id,
                    NullableInteger = x.NullableInteger,
                    NullableDateTime = x.NullableDateTime,
                    Integer = x.Integer,
                    Text = x.Text,
                    Json = x.Json
                })
                .AsQueryable();
            
            var paginatorModel = new PaginationModel<TestDto>();

            DynamicFilterSort<TestDto>.ApplyFilteringAndSortingToIQueryable(ref queryable, filter, string.Empty);
            
            paginatorModel.offset = 0;
            paginatorModel.total = queryable.Count();
            paginatorModel.count = (100 > paginatorModel.total) ? paginatorModel.total : 100;
            paginatorModel.data = queryable.Skip(0)
                .Take(100)
                .ToList();

            Assert.Single(paginatorModel.data);
            
            Teardown();
        }
        
        [Fact (Skip = "requires database")]
        public void PlayNiceWithSimpleDTOsV2()
        {
            Setup();
            var filter = $"id={GuidOne}";
            var queryable = _context.Set<TestEntity>().Where(x => x.Id != null).Select(x => new TestDto()
            {
                BoolProp = x.BoolProp,
                DateTimeProp = x.DateTimeProp,
                DblProp = x.DblProp,
                Id = x.Id,
                Status = x.Status,
                NullableBoolean = x.NullableBoolean,
                NullableDouble = x.NullableDouble,
                NullableGuid = x.NullableGuid ?? x.Id,
                NullableInteger = x.NullableInteger,
                NullableDateTime = x.NullableDateTime,
                Integer = x.Integer,
                Text = x.Text,
                Json = x.Json
            });

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }
            
            var request = new FilterSortRequest<TestDto>(0, 100, filter, string.Empty, 
                new DynamicLinqBaseDataAccessConfiguration<TestDto>(queryable));

            var paginatorModel = ServiceLocator.GetService<IDynamicFilterSortService>().GetPaginationModel(request);

            Assert.Single(paginatorModel.data);
            
            Teardown();
        }

        [Fact (Skip = "requires database")]
        public void PlayNiceWithDtoPopulation()
        {
            Setup();

            var queryable = _context.Set<TestEntity>();

            var c1 = queryable.Count();
            
            _context.RemoveRange(_context.Set<TestEntity>());
            var c2 = _context.SaveChangesAsync().Result;
            
            var dtoQueryable = queryable.Select(x=>TestDto.FromEntity(x));
            var c3 = dtoQueryable.Count();
            
            Assert.Equal(c1,c2);
            Assert.Equal(c1 - c2, c3);
            
            Teardown();
        }

        [Fact (Skip = "requires database")]
        public void DynamicLinqWorksEnum()
        {
            Setup();
            
            _context.AddRange(new List<TestEntity>()
            {
                new TestEntity{EnumValue = TestEnum.WereNoStrangersToLove},
                new TestEntity{EnumValue = TestEnum.YouKnowTheRulesAndSoDoI},
                new TestEntity{EnumValue = TestEnum.AFullCommitmentsWhatImThinkingOf},
                new TestEntity{EnumValue = TestEnum.YouWouldntGetThisFromAnyOtherGuy},
                new TestEntity{EnumValue = TestEnum.IJustWannaTellYouHowImFeeling},
                new TestEntity{EnumValue = TestEnum.GottaMakeYouUnderstand}
            });

            _context.SaveChanges();

            var shouldbeSingle = _context.Set<TestEntity>().AsQueryable();
            var shouldbeAllButOne = _context.Set<TestEntity>().AsQueryable();
            var shouldbeThree = _context.Set<TestEntity>().AsQueryable();

            DynamicFilterSort<TestEntity>.ApplyFilteringToIQueryable(ref shouldbeSingle, "enumvalue=iJustWannaTellYouHowImFeeling");
            DynamicFilterSort<TestEntity>.ApplyFilteringToIQueryable(ref shouldbeAllButOne, "enumvalue!=WereNoStrangersToLove");
            DynamicFilterSort<TestEntity>.ApplyFilteringToIQueryable(ref shouldbeThree, "enumvalue>AFullCommitmentsWhatImThinkingOf");
            
            var singleList = shouldbeSingle.ToList();
            var allButOneList = shouldbeAllButOne.ToList();
            var threeList = shouldbeThree.ToList();
            
            Assert.Single(singleList);
            Assert.Equal(5, allButOneList.Count());
            Assert.Equal(3, threeList.Count());
            
            Teardown();
        }
        
        [Fact (Skip = "requires database")]
        public void DynamicLinqWorksEnumV2()
        {
            Setup();

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var service = ServiceLocator.GetService<IDynamicFilterSortService>();
            
            _context.AddRange(new List<TestEntity>()
            {
                new TestEntity{EnumValue = TestEnum.WereNoStrangersToLove},
                new TestEntity{EnumValue = TestEnum.YouKnowTheRulesAndSoDoI},
                new TestEntity{EnumValue = TestEnum.AFullCommitmentsWhatImThinkingOf},
                new TestEntity{EnumValue = TestEnum.YouWouldntGetThisFromAnyOtherGuy},
                new TestEntity{EnumValue = TestEnum.IJustWannaTellYouHowImFeeling},
                new TestEntity{EnumValue = TestEnum.GottaMakeYouUnderstand}
            });

            _context.SaveChanges();

            var shouldbeSingle = _context.Set<TestEntity>().AsQueryable();
            var shouldbeAllButOne = _context.Set<TestEntity>().AsQueryable();
            var shouldbeThree = _context.Set<TestEntity>().AsQueryable();
            
            var request1 = new FilterSortRequest<TestEntity>(0,100,"enumvalue=iJustWannaTellYouHowImFeeling", string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(shouldbeSingle));
            
            var request2 = new FilterSortRequest<TestEntity>(0,100,"enumvalue!=WereNoStrangersToLove", string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(shouldbeAllButOne));
            
            var request3 = new FilterSortRequest<TestEntity>(0,100,"enumvalue>AFullCommitmentsWhatImThinkingOf", string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<TestEntity>(shouldbeThree));

            var singleList = service.GetEnumerable(request1).ToList();
            var allButOneList = service.GetEnumerable(request2).ToList();
            var threeList = service.GetEnumerable(request3).ToList();
            
            Assert.Single(singleList);
            Assert.Equal(5, allButOneList.Count());
            Assert.Equal(3, threeList.Count());
            
            Teardown();
        }

    }
    
    public class TestEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Integer { get; set; } = 42;
        public double DblProp { get; set; } = 3.14;
        public string Text { get; set; } = "Lorem Ipsum Dolar";
        public DateTime DateTimeProp { get; set; } = DateTime.UtcNow;
        public bool BoolProp { get; set; } = false;
        public Guid? NullableGuid { get; set; } = null;
        public DateTime? NullableDateTime { get; set; } = null;
        public int? NullableInteger { get; set; } = null;
        public double? NullableDouble { get; set; }= null;
        public bool? NullableBoolean { get; set; } = null;
        public Json Json { get; set; } = new Json();
        public string Status { get; set; } = "Cool";
        public TestEnum EnumValue { get; set; }
    }

    public class TestDto
    {
        public Guid Id { get; set; }
        public int Integer { get; set; }
        public double DblProp { get; set; }
        public string Text { get; set; }
        public DateTime DateTimeProp { get; set; }
        public bool BoolProp { get; set; }
        public Guid? NullableGuid { get; set; }
        public DateTime? NullableDateTime { get; set; }
        public int? NullableInteger { get; set; }
        public double? NullableDouble { get; set; }
        public bool? NullableBoolean { get; set; }
        public Json Json { get; set; }
        public string Status { get; set; }

        public TestDto()
        {
            
        }

        public TestDto(TestEntity entity)
        {
            Id = entity.Id;
            Integer = entity.Integer;
            DblProp = entity.DblProp;
            Text = entity.Text;
            DateTimeProp = entity.DateTimeProp;
            BoolProp = entity.BoolProp;
            NullableGuid = entity.NullableGuid;
            NullableDateTime = entity.NullableDateTime;
            NullableInteger = entity.NullableInteger;
            NullableDouble = entity.NullableDouble;
            NullableBoolean = entity.NullableBoolean;
            Json = entity.Json;
            Status = entity.Status;
        }
        
        public static Expression<Func<TestEntity, TestDto>> FromEntity(TestEntity entity)
        {
            return x => new TestDto
            {
                Id = entity.Id,
                Integer = entity.Integer,
                DblProp = entity.DblProp,
                Text = entity.Text,
                DateTimeProp = entity.DateTimeProp,
                BoolProp = entity.BoolProp,
                NullableGuid = entity.NullableGuid,
                NullableDateTime = entity.NullableDateTime,
                NullableInteger = entity.NullableInteger,
                NullableDouble = entity.NullableDouble,
                NullableBoolean = entity.NullableBoolean,
                Json = entity.Json,
                Status = entity.Status,
            };
        }
    }
}
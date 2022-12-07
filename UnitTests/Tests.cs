using System;
using System.Collections.Generic;
using System.Linq;
using Valid_DynamicFilterSort;
using Valid_DynamicFilterSort.DynamicLinq;
using Valid_DynamicFilterSort.Extensions;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Models;
using Valid_DynamicFilterSort.Utilities;
using Xunit;

namespace UnitTests
{
    public class DynamicFilterSortShould : BaseTest
    {
        internal class CodeName
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void makeastring()
        {
            var fs = DynamicFilterSort<CodeName>.BuildFilterString("name=primary",
                DynamicFilterSort<CodeName>.FilterSortType.DynamicLinq);
            Assert.NotEmpty(fs.primary.FilterString);
        }

        [Fact]
        public void SeparateParametersEventWithCommas()
        {
            const string parameterString1 = "name=Attorney General, Office Of,code=AG";
            const string parameterString2 = "code=AG,name=Attorney General, Office Of";
            const string parameterString3 = "name=Attorney General, Office Of,code=AG,id!=5";
            const string parameterString4 = "id!=3";
            const string parameterString5 = "name=fred,code=flintstone,id=4";
            const string parameterString6 =
                "fields.data.stuff.whatEver=Things, other things, and stuff,fields.data.morestuff=thing";
            const string parameterString7 =
                "fields.data.stuff.whatEver=Things, other things, and stuff,fields.data.morestuff=thing,id=4";
            var parameters = DynamicFilterSort<CodeName>.BuildListOfParametersFromString(parameterString1);
            Assert.Equal(2, parameters.Count);
            Assert.Contains(parameters, a =>
                a.Key.Equals("name", StringComparison.InvariantCultureIgnoreCase) &&
                a.Value.Equals("Attorney General, Office Of", StringComparison.InvariantCultureIgnoreCase) &&
                a.Operator.Equals("="));
            Assert.Contains(parameters, a =>
                a.Key.Equals("code", StringComparison.InvariantCultureIgnoreCase) &&
                a.Value.Equals("ag", StringComparison.InvariantCultureIgnoreCase) &&
                a.Operator.Equals("="));
            parameters = DynamicFilterSort<CodeName>.BuildListOfParametersFromString(parameterString2);
            Assert.Equal(2, parameters.Count);
            parameters = DynamicFilterSort<CodeName>.BuildListOfParametersFromString(parameterString3);
            Assert.Equal(3, parameters.Count);
            Assert.Contains(parameters, a =>
                a.Key.Equals("id", StringComparison.InvariantCultureIgnoreCase) &&
                a.Value.Equals("5", StringComparison.InvariantCultureIgnoreCase) &&
                a.Operator.Equals("!="));
            parameters = DynamicFilterSort<CodeName>.BuildListOfParametersFromString(parameterString4);
            Assert.Single(parameters);
            parameters = DynamicFilterSort<CodeName>.BuildListOfParametersFromString(parameterString5);
            Assert.Equal(3, parameters.Count);
            parameters = DynamicFilterSort<ParentModel>.BuildListOfParametersFromString(parameterString6);
            Assert.Equal(2, parameters.Count);
            parameters = DynamicFilterSort<ParentModel>.BuildListOfParametersFromString(parameterString7);
            Assert.Equal(3, parameters.Count);
            Assert.Contains(parameters, a =>
                a.Key.Equals("fields.data.stuff.whatEver", StringComparison.InvariantCultureIgnoreCase) &&
                a.Value.Equals("Things, other things, and stuff", StringComparison.InvariantCultureIgnoreCase) &&
                a.Operator.Equals("="));
        }

        [Fact]
        public void ApplyFilteringToQueryable()
        {
            SetUp();

            var parameters = "parId=10,PARID=1";
            var queryable = children.AsQueryable();

            DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);

            var result = queryable;

            Assert.Equal(4, result.Count());

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryableV2()
        {
            SetUp();

            var parameters = "parId=10,PARID=1";
            var queryable = children.AsQueryable();

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<ChildModel>(0, 10, parameters, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<ChildModel>(queryable));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);

            Assert.Equal(4, result.Count());

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryable_DateTimeField()
        {
            SetUp();

            var todayString = DateTime.UtcNow.ToString("o").Substring(0, 10);
            var parameters = $"par.alpha.created={todayString}%"; //uses StartsWith to find Date

            var queryable = children.AsQueryable();

            DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);

            var expected = children.Where(x => x.par.alpha.created.ToString("o").StartsWith(todayString));
            var result = queryable;

            Assert.Equal(expected.Count(), result.Count());

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryable_DateTimeFieldV2()
        {
            SetUp();

            var todayString = DateTime.UtcNow.ToString("o").Substring(0, 10);
            var parameters = $"par.alpha.created={todayString}%"; //uses StartsWith to find Date

            var queryable = children.AsQueryable();

            var expected = children.Where(x => x.par.alpha.created.ToString("o").StartsWith(todayString));
            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<ChildModel>(0, 10, parameters, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<ChildModel>(queryable));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);

            Assert.Equal(expected.Count(), result.Count());

            TearDown();
        }

        [Fact (Skip = "Broken after changes to json model")]
        public void ApplyFilteringToQueryableDictionary()
        {
            SetUp();

            var parameters = "par.fields.data.numbeR=3";
            var queryable = children.AsQueryable();

            DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);

            var result = queryable.ToList();

            Assert.Equal(2, result.Count());

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryableDictionaryV2()
        {
            SetUp();

            var parameters = "par.fields.data.numbeR=3";
            var queryable = children.AsQueryable();

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<ChildModel>(0, 10, parameters, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<ChildModel>(queryable));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);

            Assert.Equal(2, result.Count());

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryableDictionaryRecursive()
        {
            SetUp();

            var parameters = "par.fields.data.dict.string=strung";
            var queryable = children.AsQueryable();

            DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);

            var result = queryable;

            var q2 = children.AsQueryable();

            DynamicFilterSort<ChildModel>.ReduceDataHavingParameterKeys(ref q2,
                new Parameter("par.fields.data.dict.string", "strung", typeof(string), "=", FilterTypeEnum.SECONDARY));

            Assert.Equal(q2.Count(), result.Count());

            queryable = null;
            result = null;
            q2 = null;

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryableNotEqual()
        {
            SetUp();

            var parameters = "id!=7";
            var queryable = children.AsQueryable();

            DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);

            var result = queryable;

            Assert.Equal(19, result.Count());
            Assert.Equal(0, result.Count(x => x.id == 7));

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryableNotEqualV2()
        {
            SetUp();

            var parameters = "id!=7";
            var queryable = children.AsQueryable();

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<ChildModel>(0, 100, parameters, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<ChildModel>(queryable));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);

            Assert.Equal(19, result.Count());
            Assert.Equal(0, result.Count(x => x.id == 7));

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryableWithChildProperties()
        {
            SetUp();

            var parameters = "par.alpha.alpha=true";
            var queryable = children.AsQueryable();

            DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);

            var result = queryable;

            Assert.Equal(10, result.Count());

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryableWithChildPropertiesV2()
        {
            SetUp();

            var parameters = "par.alpha.alpha=true";
            var queryable = children.AsQueryable();

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<ChildModel>(0, 100, parameters, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<ChildModel>(queryable));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);

            Assert.Equal(10, result.Count());

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryableWithDateRangeInclusive()
        {
            SetUp();

            var tmwString = DateTime.UtcNow.AddDays(1).ToString("o").Substring(0, 10);
            var twoDay = DateTime.UtcNow.AddDays(-2).ToString("o").Substring(0, 10);

            var parameters = $"par.alpha.created<={tmwString},par.alpha.created>={twoDay}";
            var queryable = children.AsQueryable();

            DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);

            var result = queryable;

            Assert.Equal(
                children.Count(x =>
                    x.par.alpha.created <= DateTime.Parse(tmwString) && x.par.alpha.created >= DateTime.Parse(twoDay)),
                result.Count());

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryableWithDateRangeInclusiveV2()
        {
            SetUp();

            var tmwString = DateTime.UtcNow.AddDays(1).ToString("o").Substring(0, 10);
            var twoDay = DateTime.UtcNow.AddDays(-2).ToString("o").Substring(0, 10);

            var parameters = $"par.alpha.created<={tmwString},par.alpha.created>={twoDay}";
            var queryable = children.AsQueryable();

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<ChildModel>(0, 100, parameters, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<ChildModel>(queryable));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);

            Assert.Equal(
                children.Count(x =>
                    x.par.alpha.created <= DateTime.Parse(tmwString) && x.par.alpha.created >= DateTime.Parse(twoDay)),
                result.Count());

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryableWithDateRangeNonInclusive()
        {
            SetUp();

            var tmwString = DateTime.UtcNow.AddDays(1).ToString("o").Substring(0, 10);
            var twoDay = DateTime.UtcNow.AddDays(-2).ToString("o").Substring(0, 10);

            var parameters = $"par.alpha.created<{tmwString},par.alpha.created>{twoDay}";
            var queryable = children.AsQueryable();

            DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);

            var result = queryable;

            Assert.Equal(
                children.Count(x =>
                    x.par.alpha.created < DateTime.Parse(tmwString) && x.par.alpha.created > DateTime.Parse(twoDay)),
                result.Count());

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryableWithDateRangeNonInclusiveV2()
        {
            SetUp();

            var tmwString = DateTime.UtcNow.AddDays(1).ToString("o").Substring(0, 10);
            var twoDay = DateTime.UtcNow.AddDays(-2).ToString("o").Substring(0, 10);

            var parameters = $"par.alpha.created<{tmwString},par.alpha.created>{twoDay}";
            var queryable = children.AsQueryable();

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<ChildModel>(0, 100, parameters, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<ChildModel>(queryable));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);

            Assert.Equal(
                children.Count(x =>
                    x.par.alpha.created < DateTime.Parse(tmwString) && x.par.alpha.created > DateTime.Parse(twoDay)),
                result.Count());

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryableWithRangeInclusive()
        {
            SetUp();

            var parameters = "id>=1,id<=5";
            var queryable = children.AsQueryable();

            DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);

            var result = queryable;

            Assert.Equal(5, result.Count());

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryableWithRangeInclusiveV2()
        {
            SetUp();

            var parameters = "id>=1,id<=5";
            var queryable = children.AsQueryable();

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<ChildModel>(0, 100, parameters, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<ChildModel>(queryable));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);

            Assert.Equal(5, result.Count());

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryableWithRangeNonInclusive()
        {
            SetUp();

            var parameters = "id>1.25,id<5";
            var queryable = children.AsQueryable();

            DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);

            var result = queryable;

            Assert.Equal(children.Count(x => x.id > 1.25 && x.id < 5), result.Count());

            TearDown();
        }

        [Fact]
        public void ApplyFilteringToQueryableWithRangeNonInclusiveV2()
        {
            SetUp();

            var parameters = "id>1.25,id<5";
            var queryable = children.AsQueryable();

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<ChildModel>(0, 100, parameters, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<ChildModel>(queryable));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);

            Assert.Equal(children.Count(x => x.id > 1.25 && x.id < 5), result.Count());

            TearDown();
        }

        [Fact]
        public void ApplySortingAndFilteringToPaginationModel()
        {
            SetUp();

            var todayString = DateTime.UtcNow.ToString("o").Substring(0, 10);
            var filters = $"par.alpha.created={todayString}%"; //uses StartsWith to find Date
            var sorts = "id=asc";

            IPaginationModel<ChildModel> paginator = new PaginationModel<ChildModel>();
            var queryable = children.AsQueryable();

            DynamicFilterSort<ChildModel>.ApplyFilteringAndSortingToPaginationModel(ref paginator, ref queryable, 0, 5,
                filters, sorts);

            Assert.Equal(5, paginator.count);
            Assert.Equal(10, paginator.total);
            Assert.Equal(3, paginator.data.First().id);

            TearDown();
        }

        [Fact]
        public void ApplySortingAndFilteringToPaginationModelV2()
        {
            SetUp();

            var todayString = DateTime.UtcNow.ToString("o").Substring(0, 10);
            var filters = $"par.alpha.created={todayString}%"; //uses StartsWith to find Date
            var sorts = "id=asc";

            var queryable = children.AsQueryable();

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<ChildModel>(0, 5, filters, sorts,
                new DynamicLinqBaseDataAccessConfiguration<ChildModel>(queryable));

            var paginator = ServiceLocator.GetService<IDynamicFilterSortService>().GetPaginationModel(request);

            Assert.Equal(5, paginator.count);
            Assert.Equal(10, paginator.total);
            Assert.Equal(3, paginator.data.First().id);

            TearDown();
        }

        [Fact]
        public void ApplySortingToQueryable()
        {
            SetUp();

            var parameters = "parId=DESC,ID=ASC";
            var queryable = children.AsQueryable();

            DynamicFilterSort<ChildModel>.ApplySortingToIQueryable(ref queryable, parameters);

            var result = queryable;

            Assert.Equal(10, result.First().parId);
            Assert.Equal(19, result.First().id);

            TearDown();
        }

        [Fact]
        public void ApplySortingToQueryableV2()
        {
            SetUp();

            var parameters = "parId=DESC,ID=ASC";
            var queryable = children.AsQueryable();

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<ChildModel>(0, 100, string.Empty, parameters,
                new DynamicLinqBaseDataAccessConfiguration<ChildModel>(queryable));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);

            Assert.Equal(10, result.First().parId);
            Assert.Equal(19, result.First().id);

            TearDown();
        }

        [Fact]
        public void ApplySortingToQueryableBasedOnJson()
        {
            SetUp();

            var parameters = "id=DESC,par.fields.data.dict.expirationDate=DESC";
            var queryable = children.AsQueryable();

            DynamicFilterSort<ChildModel>.ApplySortingToIQueryable(ref queryable, parameters);

            var result = queryable.ToList();

            Assert.Equal(10, result.First().parId);
            Assert.Equal(20, result.First().id);

            TearDown();
        }

        [Fact]
        public void ApplySortingToQueryableBasedOnJsonV2()
        {
            SetUp();

            var parameters = "id=DESC,par.fields.data.dict.expirationDate=DESC";
            var queryable = children.AsQueryable();

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<ChildModel>(0, 100, string.Empty, parameters,
                new DynamicLinqBaseDataAccessConfiguration<ChildModel>(queryable));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);

            Assert.Equal(10, result.First().parId);
            Assert.Equal(20, result.First().id);

            TearDown();
        }

        [Fact]
        public void ApplySortingToQueryableWithChildProperties()
        {
            SetUp();

            var parameters = "par.id=DESC,ID=ASC";
            var queryable = children.AsQueryable();

            DynamicFilterSort<ChildModel>.ApplySortingToIQueryable(ref queryable, parameters);

            var result = queryable;

            Assert.Equal(10, result.First().parId);
            Assert.Equal(19, result.First().id);

            TearDown();
        }

        [Fact]
        public void ApplySortingToQueryableWithChildPropertiesV2()
        {
            SetUp();

            var parameters = "par.id=DESC,ID=ASC";
            var queryable = children.AsQueryable();

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<ChildModel>(0, 100, string.Empty, parameters,
                new DynamicLinqBaseDataAccessConfiguration<ChildModel>(queryable));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);

            Assert.Equal(10, result.First().parId);
            Assert.Equal(19, result.First().id);

            TearDown();
        }

        [Fact]
        public void BuildFilterString()
        {
            var parameters =
                "par.alpha.created>2018-01-01,id=2,id=5,id>1,id<=5,par.alpha.alpha!=false,name=child%,par.alpha.created=2018%,name=%1,name=%child%,id!=1,id!=4";

            var expected =
                "filter => (((filter.id!=1)) AND ((filter.id!=4)) AND ((filter.id<=5)) AND ((filter.id=2) OR (filter.id=5)) AND ((filter.id>1))) AND " + //Numeric are not treated as strings, also inequalities working,
                //uses ORs for multiple of same field equals, and ANDs for multiple of same field for all other operators
                "(((filter.name.ToLower().StartsWith(\"child\")) OR " + //partial matches uses quotes and starts with
                "(filter.name.ToLower().EndsWith(\"1\")) OR " + //partial matches uses quotes and ends with
                "(filter.name.ToLower().Contains(\"child\")))) AND " + //partial matches uses quotes and contains
                "(filter.par.alpha.alpha!=false) AND " + //booleans do not use strings
                "((filter.par.alpha.created.Year=2018) AND " + //dates using an inequality doesn't use quotes unless a partial match
                "((filter.par.alpha.created>DateTime.Parse(\"2018-01-01T00:00:00.0000000\"))))"; //dates using an inequality doesn't use quotes unless a partial match

            var actual = DynamicFilterSort<ChildModel>.BuildFilterString(parameters,
                DynamicFilterSort<ChildModel>.FilterSortType.DynamicLinq);

            Assert.Equal(expected, actual.primary.FilterString);
        }

        [Fact]
        public void IgnorePropertyNameThatIsJsonExtensionData()
        {
            SetUp();
            var filter = "fields.data.color=black";
            var parameter = DynamicFilterSort<ParentModel>.SplitParameterString(filter);
            Assert.Equal("data", parameter.JsonExtensionDataAttributeName.ToLower());
            var component = DynamicFilterSort<ParentModel>.BuildFilterComponentPostgreSql(parameter, "", false);
            Assert.Equal("(lower(@tableAlias.fields::text)::jsonb->>'color')=black", component);
            TearDown();
        }

        [Fact]
        public void BuildPostgresFilterString()
        {
            SetUp();
            var parameters =
                "alphaid=1,alphaid!=2,alphaid=3,description=%red%,name=stuff%,name=%things,name=stuff and things," +
                "created=2019-01-01,active=true,modified>=2019-02-01,modified=%2019%,fields.data.color=red," +
                "guid1=FF45BB4B-D727-4EF3-B2ED-8C9CB9F4D1C6,guid2=FF45BB4B-D727-4EF3-B2ED-8C9CB9F4D1C6";

            var expected = "active=true AND " + //boolean no quotes
                           "((alpha_id<>2) AND (alpha_id=1 OR alpha_id=3)) AND " + //multiple of same field uses ORs,
                           //exception being different operator
                           "created_dt='2019-01-01T00:00:00.0000000' AND " + //datetime uses exact match on equals
                           "LOWER(description::text) ILIKE '%red%' AND " + //uses LIKE and quotes and casts
                           //guids converted to text bc generation is easier this way
                           "LOWER(guid1::text)='ff45bb4b-d727-4ef3-b2ed-8c9cb9f4d1c6' AND LOWER(guid2::text)='ff45bb4b-d727-4ef3-b2ed-8c9cb9f4d1c6' AND " +
                           "((LOWER(modified_dt::text) ILIKE '%2019%') AND " + //casts datetime as text and uses quotes
                           "(modified_dt>='2019-02-01T00:00:00.0000000')) AND " +
                           "((LOWER(name::text) ILIKE 'stuff%' OR LOWER(name::text) ILIKE '%things' OR LOWER(name)='stuff and things')) AND " + //cast to text for partials
                           "(lower(fields::text)::jsonb->>'color')='red'"; //json stuff, converts to lowercase text, then back to json

            var actual = DynamicFilterSort<ParentModel>.GetFilterString(parameters,
                DynamicFilterSort<ParentModel>.FilterSortType.PostgreSql);

            Assert.Equal(expected, actual);
            TearDown();
        }

        [Fact]
        public void BuildPostgresParameterizedFilterString()
        {
            SetUp();
            var parameters =
                "alphaid=1,alphaid!=2,alphaid=3,description=%red%,name=stuff%,name=%things,name=stuff and things," +
                "created=2019-01-01,active=true,modified>=2019-02-01,modified=%2019%,fields.data.color=red";

            var expected = "tbl.active=@tbl8 AND " +
                           "((tbl.alpha_id<>@tbl1) AND (tbl.alpha_id=@tbl0 OR tbl.alpha_id=@tbl2)) AND " +
                           "tbl.created_dt=@tbl7 AND " +
                           "LOWER(tbl.description::text) ILIKE CONCAT('%',@tbl3,'%') AND " +
                           "((LOWER(tbl.modified_dt::text) ILIKE CONCAT('%',@tbl10,'%')) AND " +
                           "(tbl.modified_dt>=@tbl9)) AND " +
                           "((LOWER(tbl.name::text) ILIKE CONCAT(@tbl4,'%') OR LOWER(tbl.name::text) ILIKE CONCAT('%',@tbl5) OR LOWER(tbl.name)=@tbl6)) AND " +
                           "(lower(tbl.fields::text)::jsonb->>'color')=@tbl11";

            var actual = DynamicFilterSort<ParentModel>.GetPostgreSqlParameterizedFilterString(parameters, "tbl");

            Assert.Equal(expected, actual.Sql);
            Assert.Equal(12, actual.Parameters.ParameterNames.Count());
            TearDown();
        }

        [Fact]
        public void BuildPostgreSqlSortOrderString()
        {
            SetUp();

            var parameters = "ID=ASC,Name=ASC,fields.data.color=DESC";
            var expected = "id ASC, name ASC, (lower(fields::text)::jsonb->>'color') DESC";
            var actual = DynamicFilterSort<ParentModel>.GetSortString(parameters,
                DynamicFilterSort<ParentModel>.FilterSortType.PostgreSql);

            TearDown();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BuildPostgreSqlSortOrderStringWithTableAlias()
        {
            SetUp();

            var parameters = "ID=ASC,Name=ASC,fields.data.color=DESC";
            var expected = "tbl.id ASC, tbl.name ASC, (lower(tbl.fields::text)::jsonb->>'color') DESC";
            var actual = DynamicFilterSort<ParentModel>.BuildPostgreSqlSortOrderString(parameters, "tbl");

            TearDown();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BuildSortOrderString()
        {
            var parameters = "ID=ASC,Name=ASC,PAR.NAME=DESC";
            var expected = "id ASC, name ASC, par.name DESC";
            var actual = DynamicFilterSort<ChildModel>.GetSortString(parameters,
                DynamicFilterSort<ChildModel>.FilterSortType.DynamicLinq);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ThrowAnErrorIfBogusSortOrder()
        {
            SetUp();

            var parameters = "name=UP";
            var queryable = children.AsQueryable();

            try
            {
                DynamicFilterSort<ChildModel>.ApplySortingToIQueryable(ref queryable, parameters);
            }
            catch (Exception e)
            {
                Assert.Equal(Errors.BAD_SORT_ORDER_QUERY.Message, e.Message);
            }

            TearDown();
        }

        [Fact]
        public void ThrowAnErrorIfDateTimeIsWayMessedUp()
        {
            SetUp();

            var parameters = "par.alpha.created=1/2/3/4/5";
            var queryable = children.AsQueryable();

            try
            {
                DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);
            }
            catch (Exception e)
            {
                Assert.Equal(Errors.INVALID_DATETIME.Message, e.Message);
            }

            TearDown();
        }

        [Fact]
        public void ThrowAnErrorIfOperatorIsInvalidFiltering()
        {
            SetUp();

            var parameters = "parId=>10,PARID=1";
            var queryable = children.AsQueryable();

            try
            {
                DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);
            }
            catch (Exception e)
            {
                Assert.Equal(Errors.INVALID_OPERATOR.Message, e.Message);
            }

            TearDown();
        }

        [Fact]
        public void ThrowAnErrorIfOperatorIsInvalidSorting()
        {
            SetUp();

            var parameters = "parId>ASC";
            var queryable = children.AsQueryable();

            try
            {
                DynamicFilterSort<ChildModel>.ApplySortingToIQueryable(ref queryable, parameters);
            }
            catch (Exception e)
            {
                Assert.Equal(Errors.INVALID_OPERATOR.Message, e.Message);
            }

            TearDown();
        }

        [Fact]
        public void ThrowAnErrorIfQueryStringHasInvalidField()
        {
            SetUp();

            var parameters = "nombre=pedro";
            var queryable = children.AsQueryable();

            try
            {
                DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);
            }
            catch (Exception e)
            {
                Assert.Equal(Errors.BAD_FIELD_VALUE_QUERY.Message, e.Message);
            }

            TearDown();
        }

        [Fact]
        public void ThrowAnErrorIfQueryStringIsIncomplete()
        {
            SetUp();

            var parameters = "name!=";
            var queryable = children.AsQueryable();

            try
            {
                DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);
            }
            catch (Exception e)
            {
                Assert.Equal(Errors.MISSING_VALUE_QUERY.Message, e.Message);
            }

            TearDown();
        }

        [Fact]
        public void ThrowAnErrorIfUsingBooleanAndInequality()
        {
            SetUp();

            var parameters = "active>false";
            var queryable = children.AsQueryable();

            try
            {
                DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);
            }
            catch (Exception e)
            {
                Assert.Equal(Errors.INVALID_FILTER_TYPE.Message, e.Message);
            }

            TearDown();
        }

        [Fact]
        public void ThrowAnErrorIfUsingContainsAndInequality()
        {
            SetUp();

            var parameters = "name>stuff%";
            var queryable = children.AsQueryable();

            try
            {
                DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);
            }
            catch (Exception e)
            {
                Assert.Equal(Errors.INVALID_FILTER_TYPE.Message, e.Message);
            }

            TearDown();
        }

        [Fact]
        public void IgnoreParametersThatDoNotConvertCorrectly()
        {
            SetUp();

            var queryable = children.AsQueryable();

            var stringToInt = DynamicFilterSort<ChildModel>.GetFilterString("id=string",
                DynamicFilterSort<ChildModel>.FilterSortType.DynamicLinq);

            var stringToGuidInvalid = DynamicFilterSort<ChildModel>.GetFilterString("guid=string",
                DynamicFilterSort<ChildModel>.FilterSortType.DynamicLinq);

            var stringToGuidValid = DynamicFilterSort<ChildModel>.GetFilterString("guid=" + Guid.NewGuid().ToString(),
                DynamicFilterSort<ChildModel>.FilterSortType.DynamicLinq);

            var stringToIntValid = DynamicFilterSort<ChildModel>.GetFilterString("id=5",
                DynamicFilterSort<ChildModel>.FilterSortType.DynamicLinq);

            var stringToDateTimeInvalid = DynamicFilterSort<ChildModel>.GetFilterString("par.created=5",
                DynamicFilterSort<ChildModel>.FilterSortType.DynamicLinq);

            var stringToDateTimeValid = DynamicFilterSort<ChildModel>.GetFilterString("par.created=1990-02-12",
                DynamicFilterSort<ChildModel>.FilterSortType.DynamicLinq);

            var stringToNullableInt = DynamicFilterSort<ChildModel>.GetFilterString("nint=42",
                DynamicFilterSort<ChildModel>.FilterSortType.DynamicLinq);

            var stringToNullableBool = DynamicFilterSort<ChildModel>.GetFilterString("nbool=false",
                DynamicFilterSort<ChildModel>.FilterSortType.DynamicLinq);

            var stringToNullableDateTime = DynamicFilterSort<ChildModel>.GetFilterString("ndt=1990-02-12",
                DynamicFilterSort<ChildModel>.FilterSortType.DynamicLinq);

            Assert.Equal(0, stringToInt.Length);
            Assert.Equal(0, stringToGuidInvalid.Length);
            Assert.Equal(0, stringToDateTimeInvalid.Length);
            Assert.NotEqual(0, stringToGuidValid.Length);
            Assert.NotEqual(0, stringToIntValid.Length);
            Assert.NotEqual(0, stringToDateTimeValid.Length);
            Assert.NotEqual(0, stringToNullableInt.Length);
            Assert.NotEqual(0, stringToNullableBool.Length);
            Assert.NotEqual(0, stringToNullableDateTime.Length);
            TearDown();
        }

        [Fact]
        public void FindDataThatShouldNotBeStringsStoredAsStrings()
        {
            SetUp();
            var guidAsString = "38680661-0c3b-40c5-84c0-5d10a13061d1";
            var parameters = $"guidasstring={guidAsString}";
            var filter = DynamicFilterSort<ChildModel>.GetFilterString(parameters,
                DynamicFilterSort<ChildModel>.FilterSortType.DynamicLinq);

            var queryable = children.AsQueryable();
            DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);

            Assert.NotEqual(0, filter.Length);
            Assert.Equal(2, queryable.Count());

            TearDown();
        }

        [Fact]
        public void FindDataThatShouldNotBeStringsStoredAsStringsV2()
        {
            SetUp();
            var guidAsString = "38680661-0c3b-40c5-84c0-5d10a13061d1";
            var parameters = $"guidasstring={guidAsString}";

            var queryable = children.AsQueryable();

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<ChildModel>(0, 100, parameters, string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<ChildModel>(queryable));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);
            DynamicFilterSort<ChildModel>.ApplyFilteringToIQueryable(ref queryable, parameters);

            Assert.Equal(2, result.Count());

            TearDown();
        }

        [Fact]
        public void PlayNiceWithPotentialTypesAsPropertyNamesInDynamicLinq()
        {
            var uris = new List<ClientUri>
            {
                new ClientUri(1, 1, "https://localhost:5000", "origin", ""),
                new ClientUri(2, 1, "https://cap.dev.valididcloud.com", "origin", ""),
                new ClientUri(3, 1, "https://cap.dev.valididcloud.com/swagger/oauth2-redirect.html", "redirect", ""),
            };

            var urisQueryable = uris.AsQueryable();

            var sortString = DynamicFilterSort<ClientUri>.BuildSortOrderString("uri=asc");

            Assert.Equal("URI ASC", sortString);

            DynamicFilterSort<ClientUri>.ApplySortingToIQueryable(ref urisQueryable, "uri=asc,clientid=desc");

            var sortedArr = urisQueryable.ToArray();

            Assert.Equal(2, sortedArr[0].Id);
            Assert.Equal(3, sortedArr[1].Id);
            Assert.Equal(1, sortedArr[2].Id);

            DynamicFilterSort<ClientUri>.ApplyFilteringToIQueryable(ref urisQueryable, "uri=%5000%,clientid=1");

            Assert.Single(urisQueryable);
            Assert.Equal(1, urisQueryable.First().Id);
        }

        [Fact]
        public void PlayNiceWithPotentialTypesAsPropertyNamesInDynamicLinqV2()
        {
            var uris = new List<ClientUri>
            {
                new ClientUri(1, 1, "https://localhost:5000", "origin", ""),
                new ClientUri(2, 1, "https://cap.dev.valididcloud.com", "origin", ""),
                new ClientUri(3, 1, "https://cap.dev.valididcloud.com/swagger/oauth2-redirect.html", "redirect", ""),
            };

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<ClientUri>(0, 100, string.Empty, "uri=asc,clientid=desc",
                new DynamicLinqBaseDataAccessConfiguration<ClientUri>(uris.AsQueryable()));


            var sortedArr = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request).ToArray();

            Assert.Equal(2, sortedArr[0].Id);
            Assert.Equal(3, sortedArr[1].Id);
            Assert.Equal(1, sortedArr[2].Id);

            request = new FilterSortRequest<ClientUri>(0, 100, "uri=%5000%,clientid=1", string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<ClientUri>(sortedArr.AsQueryable()));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }

        [Fact]
        public void PlayNiceWithEnums()
        {
            var setup = new List<AlphaModel>
            {
                new AlphaModel {enumValue = TestEnum.NeverGonnaGiveYouUp},
                new AlphaModel {enumValue = TestEnum.NeverGonnaLetYouDown},
                new AlphaModel {enumValue = TestEnum.NeverGonnaRunAroundAndDesertYou},
                new AlphaModel {enumValue = TestEnum.NeverGonnaMakeYouCry},
                new AlphaModel {enumValue = TestEnum.NeverGonnaSayGoodbye},
                new AlphaModel {enumValue = TestEnum.NeverGonnaTellALieAndHurtYou}
            };

            var neverGonnaQueryable = setup.AsQueryable();

            DynamicFilterSort<AlphaModel>.ApplyFilteringToIQueryable(ref neverGonnaQueryable,
                "enumValue=NeverGonnaMakeYouCry");
            Assert.Single(neverGonnaQueryable.ToList());

            var filterStr = DynamicFilterSort<AlphaModel>.GetFilterString("enumValue=nev",
                DynamicFilterSort<AlphaModel>.FilterSortType.DynamicLinq);

            Assert.Empty(filterStr);
        }

        [Fact]
        public void PlayNiceWithEnumsV2()
        {
            var setup = new List<AlphaModel>
            {
                new AlphaModel {enumValue = TestEnum.NeverGonnaGiveYouUp},
                new AlphaModel {enumValue = TestEnum.NeverGonnaLetYouDown},
                new AlphaModel {enumValue = TestEnum.NeverGonnaRunAroundAndDesertYou},
                new AlphaModel {enumValue = TestEnum.NeverGonnaMakeYouCry},
                new AlphaModel {enumValue = TestEnum.NeverGonnaSayGoodbye},
                new AlphaModel {enumValue = TestEnum.NeverGonnaTellALieAndHurtYou}
            };

            var neverGonnaQueryable = setup.AsQueryable();

            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            var request = new FilterSortRequest<AlphaModel>(0, 100, "enumValue=NeverGonnaMakeYouCry", string.Empty,
                new DynamicLinqBaseDataAccessConfiguration<AlphaModel>(neverGonnaQueryable));

            var result = ServiceLocator.GetService<IDynamicFilterSortService>().GetEnumerable(request);

            Assert.Single(result);
        }

        private class ClientUri
        {
            public int Id { get; set; }
            public int ClientId { get; set; }
            public string URI { get; set; }
            public Json Client { get; set; } = new Json();
            public string ClientUriTypes { get; set; }
            public string IconClass { get; set; }

            public ClientUri()
            {
            }

            public ClientUri(int id, int clientId, string uri, string uriType, string iconClass)
            {
                Id = id;
                ClientId = clientId;
                URI = uri;
                ClientUriTypes = uriType;
                IconClass = iconClass;
                Client["id"] = clientId;
            }
        }
    }

    public class PropertiesHelperShould : BaseTest
    {
        [Fact]
        public void CanRecognizeAPropertyThatIsADictionaryAndNotCareAboutDictionaryPropertiesFromThere()
        {
            SetUp();

            var name = PropertiesHelper.GetCaseSensitivePropertyNameForModelProperty("par.fields.data.wackadoo.active",
                typeof(ChildModel));

            Assert.Equal("par.fields.Data.wackadoo.active", name);
            var x = new Dictionary<string, object>();
            x = x.ToDictionary(k => k.Key, v => v.Value, StringComparer.InvariantCultureIgnoreCase);
            TearDown();
        }

        [Fact]
        public void GetCaseSensitivePropertyName()
        {
            var propertyName = "DESCRIPTION";
            var expected = "description";
            var actual =
                PropertiesHelper.GetCaseSensitivePropertyNameForModelProperty(propertyName, typeof(ParentModel));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetCaseSensitivePropertyNameRecursive()
        {
            var propertyName = "par.ALPHA.NamE";
            var expected = "par.alpha.name";
            var actual =
                PropertiesHelper.GetCaseSensitivePropertyNameForModelProperty(propertyName, typeof(ChildModel));

            Assert.Equal(expected, actual);
        }
    }
}
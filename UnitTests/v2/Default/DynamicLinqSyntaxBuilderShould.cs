using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.DependencyInjection;
using UnitTests.v2.Models;
using Valid_DynamicFilterSort.DynamicLinq;
using Valid_DynamicFilterSort.Enums;
using Valid_DynamicFilterSort.Extensions;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Models;
using Valid_DynamicFilterSort.Utilities;
using Xunit;

namespace UnitTests.v2.Default
{
    public class DynamicLinqSyntaxBuilderShould : TestBase
    {
        private readonly IDataSyntaxBuilder _dynamicLinqSyntaxBuilder;
        private readonly List<TestPersonModel> _testModels = new List<TestPersonModel>
        {
            new TestPersonModel("Ron", "Weasley"),
            new TestPersonModel("George", "Weasley"),
            new TestPersonModel("Fred", "Weasley"),
            new TestPersonModel("Percy", "Weasley"),
            new TestPersonModel("Ginny", "Weasley")
        };

        public DynamicLinqSyntaxBuilderShould()
        {
            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            _dynamicLinqSyntaxBuilder = ServiceLocator.GetServices<IDataSyntaxBuilder>()
                .FirstOrDefault(x => x.InterfaceType == nameof(DynamicLinq));
        }

        [Fact]
        public void HonorTheOldGods()
        {
           #region setup
            var filters = new List<FilterParameter>
            {
                new FilterParameter
                {
                    Operator = OperatorEnum.EqualTo,
                    OperatorString = "=",
                    Value = "fred",
                    ValueString = "fred",
                    Order = 0,
                    ComparisonType = ComparisonTypeEnum.Full,
                    Combiner = OperatorCombinerEnum.DEFAULT,
                    Key = "firstname",
                    KeyString = "firstname",
                    PropertyInfo = new DFSPropertyInfo
                    {
                        PathHistory = new List<DFSPropertyInfo>(),
                        PathKey = "FirstName",
                        Key = "FirstName",
                        Type = typeof(string),
                        TraversalProperty = false
                    }
                },
                new FilterParameter
                {
                    Operator = OperatorEnum.EqualTo,
                    OperatorString = "=",
                    Value = "george",
                    ValueString = "george",
                    Order = 1,
                    ComparisonType = ComparisonTypeEnum.Full,
                    Combiner = OperatorCombinerEnum.DEFAULT,
                    Key = "firstname",
                    KeyString = "firstname",
                    PropertyInfo = new DFSPropertyInfo
                    {
                        PathHistory = new List<DFSPropertyInfo>(),
                        PathKey = "FirstName",
                        Key = "FirstName",
                        Type = typeof(string),
                        TraversalProperty = false
                    }
                },
                new FilterParameter
                {
                    Operator = OperatorEnum.EqualTo,
                    OperatorString = "=",
                    Value = "percy",
                    ValueString = "percy",
                    Order = 2,
                    ComparisonType = ComparisonTypeEnum.Full,
                    Combiner = OperatorCombinerEnum.DEFAULT,
                    Key = "firstname",
                    KeyString = "firstname",
                    PropertyInfo = new DFSPropertyInfo
                    {
                        PathHistory = new List<DFSPropertyInfo>(),
                        PathKey = "FirstName",
                        Key = "FirstName",
                        Type = typeof(string),
                        TraversalProperty = false
                    }
                },
                new FilterParameter
                {
                    Operator = OperatorEnum.EqualTo,
                    OperatorString = "=",
                    Value = "ron",
                    ValueString = "ron",
                    Order = 3,
                    ComparisonType = ComparisonTypeEnum.Full,
                    Combiner = OperatorCombinerEnum.DEFAULT,
                    Key = "firstname",
                    KeyString = "firstname",
                    PropertyInfo = new DFSPropertyInfo
                    {
                        PathHistory = new List<DFSPropertyInfo>(),
                        PathKey = "FirstName",
                        Key = "FirstName",
                        Type = typeof(string),
                        TraversalProperty = false
                    }
                },
                new FilterParameter
                {
                    Operator = OperatorEnum.EqualTo,
                    OperatorString = "=",
                    Value = "weasley",
                    ValueString = "weasley",
                    Order = 4,
                    ComparisonType = ComparisonTypeEnum.Full,
                    Combiner = OperatorCombinerEnum.DEFAULT,
                    Key = "lastname",
                    KeyString = "lastname",
                    PropertyInfo = new DFSPropertyInfo
                    {
                        PathHistory = new List<DFSPropertyInfo>(),
                        PathKey = "LastName",
                        Key = "LastName",
                        Type = typeof(string),
                        TraversalProperty = false
                    }
                },
                new FilterParameter
                {
                    Operator = OperatorEnum.GreaterThanOrEqualTo,
                    OperatorString = ">=",
                    Value = DateTime.UtcNow.Year.ToString(),
                    ValueString = DateTime.UtcNow.Year.ToString(),
                    Order = 5,
                    ComparisonType = ComparisonTypeEnum.StartsWith,
                    Combiner = OperatorCombinerEnum.DEFAULT,
                    Key = "CreatedTimestamp",
                    KeyString = "createdtimestamp",
                    PropertyInfo = new DFSPropertyInfo
                    {
                        PathHistory = new List<DFSPropertyInfo>(),
                        PathKey = "CreatedTimestamp",
                        Key = "CreatedTimestamp",
                        Type = typeof(DateTime),
                        TraversalProperty = false
                    }
                },
                new FilterParameter
                {
                    Operator = OperatorEnum.GreaterThanOrEqualTo,
                    OperatorString = ">=",
                    Value = 100,
                    ValueString = "100",
                    Order = 6,
                    ComparisonType = ComparisonTypeEnum.Full,
                    Combiner = OperatorCombinerEnum.DEFAULT,
                    Key = "Id",
                    KeyString = "Id",
                    PropertyInfo = new DFSPropertyInfo
                    {
                        PathHistory = new List<DFSPropertyInfo>(),
                        PathKey = "Id",
                        Key = "Id",
                        Type = typeof(int),
                        TraversalProperty = false
                    }
                }
            };
            #endregion setup
            var ds = (DynamicLinqBaseDataSyntaxModel) _dynamicLinqSyntaxBuilder.BuildDataSyntax(filters);
            var expected = "filter => " +
                           "((" +
                                "filter.CreatedTimestamp.ToString(\"O\").ToLower().StartsWith( @0.ToLower() )" +
                           ")) && ((" +
                                "filter.FirstName.ToLower() = @1.ToLower() || " +
                                "filter.FirstName.ToLower() = @2.ToLower() || " +
                                "filter.FirstName.ToLower() = @3.ToLower() || " +
                                "filter.FirstName.ToLower() = @4.ToLower()" +
                           ")) && ((" +
                                "filter.Id >= @5" +
                           ")) && ((" +
                                "filter.LastName.ToLower() = @6.ToLower()" +
                           "))";
            
            Assert.Equal(expected,  ds.PrimarySortSyntax);
            
            var parameters = ds.Parameters
                .Cast<FilterParameter>()
                .Where(w => !w.PropertyInfo.TraversalProperty)
                .OrderBy(o => o.Fields["filter_order"])
                .Select(s => s.Value)
                .ToArray();

            var filtered = _testModels.AsQueryable()
                .Where(ds.PrimarySortSyntax, parameters)
                .ToList();

            var expectedModels = _testModels
                .AsQueryable()
                .Where(w =>
                    ((w.CreatedTimestamp.ToString("O").ToLower().StartsWith(DateTime.UtcNow.Year.ToString()))) &&
                    ((w.FirstName.ToLower() == "fred" ||
                      w.FirstName.ToLower() == "george" ||
                      w.FirstName.ToLower() == "percy" ||
                      w.FirstName.ToLower() == "ron")) &&
                    ((w.Id >= 100)) &&
                    ((w.LastName.ToLower() == "weasley")))
                .ToList();
            
            Assert.Equal(expectedModels.Count(), filtered.Count());
            Assert.Equal(expectedModels.Select(s=>s.Id).ToArray(), filtered.Select(s=>s.Id).ToArray());
        }

        [Fact]
        public void KeepParametersInOrderCombiningAsSpecified()
        {
            #region setup
            var filters = new List<FilterParameter>
            {
                new FilterParameter
                {
                    Operator = OperatorEnum.EqualTo,
                    OperatorString = "=",
                    Value = "fred",
                    ValueString = "fred",
                    Order = 0,
                    ComparisonType = ComparisonTypeEnum.Full,
                    Combiner = OperatorCombinerEnum.OR,
                    Key = "firstname",
                    KeyString = "firstname",
                    PropertyInfo = new DFSPropertyInfo
                    {
                        PathHistory = new List<DFSPropertyInfo>(),
                        PathKey = "FirstName",
                        Key = "FirstName",
                        Type = typeof(string),
                        TraversalProperty = false
                    }
                },
                new FilterParameter
                {
                    Operator = OperatorEnum.EqualTo,
                    OperatorString = "=",
                    Value = "george",
                    ValueString = "george",
                    Order = 1,
                    ComparisonType = ComparisonTypeEnum.Full,
                    Combiner = OperatorCombinerEnum.OR,
                    Key = "firstname",
                    KeyString = "firstname",
                    PropertyInfo = new DFSPropertyInfo
                    {
                        PathHistory = new List<DFSPropertyInfo>(),
                        PathKey = "FirstName",
                        Key = "FirstName",
                        Type = typeof(string),
                        TraversalProperty = false
                    }
                },
                new FilterParameter
                {
                    Operator = OperatorEnum.EqualTo,
                    OperatorString = "=",
                    Value = "weasley",
                    ValueString = "weasley",
                    Order = 2,
                    ComparisonType = ComparisonTypeEnum.Full,
                    Combiner = OperatorCombinerEnum.AND,
                    Key = "lastname",
                    KeyString = "lastname",
                    PropertyInfo = new DFSPropertyInfo
                    {
                        PathHistory = new List<DFSPropertyInfo>(),
                        PathKey = "LastName",
                        Key = "LastName",
                        Type = typeof(string),
                        TraversalProperty = false
                    }
                }
            };
            #endregion setup
            
            var ds = (DynamicLinqBaseDataSyntaxModel) _dynamicLinqSyntaxBuilder.BuildDataSyntax(filters);
            var expected = "filter => filter.FirstName.ToLower() = @0.ToLower() || filter.FirstName.ToLower() = @1.ToLower() && filter.LastName.ToLower() = @2.ToLower()";
            Assert.Equal(expected,  ds.PrimarySortSyntax);
            
            var parameters = ds.Parameters
                .Cast<FilterParameter>()
                .Where(w => !w.PropertyInfo.TraversalProperty)
                .OrderBy(o => o.Fields["filter_order"])
                .Select(s => s.Value)
                .ToArray();

            var filtered = _testModels.AsQueryable()
                .Where(ds.PrimarySortSyntax, parameters);

            var expectedModels = _testModels
                .AsQueryable()
                .Where(w => w.FirstName.ToLower() == "fred" || w.FirstName.ToLower() == "george" && w.LastName.ToLower() == "weasley")
                .ToList();
            
            Assert.Equal(expectedModels.Count(), filtered.Count());
            Assert.Equal(expectedModels.Select(s=>s.Id).ToArray(), filtered.Select(s=>s.Id).ToArray());
        }

        [Fact]
        public void HonorTheOldGodsAndTheNew()
        {
            #region setup
            var filters = new List<FilterParameter>
            {
                new FilterParameter
                {
                    Operator = OperatorEnum.EqualTo,
                    OperatorString = "=",
                    Value = "fred",
                    ValueString = "fred",
                    Order = 0,
                    ComparisonType = ComparisonTypeEnum.Full,
                    Combiner = OperatorCombinerEnum.DEFAULT,
                    Key = "firstname",
                    KeyString = "firstname",
                    PropertyInfo = new DFSPropertyInfo
                    {
                        PathHistory = new List<DFSPropertyInfo>(),
                        PathKey = "FirstName",
                        Key = "FirstName",
                        Type = typeof(string),
                        TraversalProperty = false
                    }
                },
                new FilterParameter
                {
                    Operator = OperatorEnum.EqualTo,
                    OperatorString = "=",
                    Value = "george",
                    ValueString = "george",
                    Order = 1,
                    ComparisonType = ComparisonTypeEnum.Full,
                    Combiner = OperatorCombinerEnum.DEFAULT,
                    Key = "firstname",
                    KeyString = "firstname",
                    PropertyInfo = new DFSPropertyInfo
                    {
                        PathHistory = new List<DFSPropertyInfo>(),
                        PathKey = "FirstName",
                        Key = "FirstName",
                        Type = typeof(string),
                        TraversalProperty = false
                    }
                },
                new FilterParameter
                {
                    Operator = OperatorEnum.EqualTo,
                    OperatorString = "=",
                    Value = "weasley",
                    ValueString = "weasley",
                    Order = 2,
                    ComparisonType = ComparisonTypeEnum.Full,
                    Combiner = OperatorCombinerEnum.AND,
                    Key = "lastname",
                    KeyString = "lastname",
                    PropertyInfo = new DFSPropertyInfo
                    {
                        PathHistory = new List<DFSPropertyInfo>(),
                        PathKey = "LastName",
                        Key = "LastName",
                        Type = typeof(string),
                        TraversalProperty = false
                    }
                }
            };
            #endregion setup
            
            var ds = (DynamicLinqBaseDataSyntaxModel) _dynamicLinqSyntaxBuilder.BuildDataSyntax(filters);
            var expected = "filter => ((filter.FirstName.ToLower() = @0.ToLower() || filter.FirstName.ToLower() = @1.ToLower())) && filter.LastName.ToLower() = @2.ToLower()";
            Assert.Equal(expected,  ds.PrimarySortSyntax);
        }

        [Fact]
        public void HandleSortSyntax()
        {
            #region setup
            var sorts = new List<SortParameter>
            {
                new SortParameter
                {
                    Value = SortDirectionEnum.Asc,
                    Order = 0,
                    Key = "firstname",
                    PropertyInfo = new DFSPropertyInfo
                    {
                        PathHistory = new List<DFSPropertyInfo>(),
                        PathKey = "FirstName",
                        Key = "FirstName",
                        Type = typeof(string),
                        TraversalProperty = false
                    }
                },
                new SortParameter
                {
                    Value = SortDirectionEnum.Desc,
                    Order = 1,
                    Key = "lastname",
                    PropertyInfo = new DFSPropertyInfo
                    {
                        PathHistory = new List<DFSPropertyInfo>(),
                        PathKey = "LastName",
                        Key = "LastName",
                        Type = typeof(string),
                        TraversalProperty = false
                    }
                }
            };
            #endregion setup
            
            var ds = (DynamicLinqBaseDataSyntaxModel) _dynamicLinqSyntaxBuilder.BuildDataSyntax(sorts);
            var expected = "FirstName ASC, LastName DESC";
            Assert.Equal(expected,  ds.PrimarySortSyntax);

            var expectedArr = _testModels
                .OrderBy(o => o.FirstName)
                .ThenByDescending(o => o.LastName)
                .Select(s => s.FirstName)
                .ToArray();
            
            var actual = _testModels.AsQueryable()
                .OrderBy(ds.PrimarySortSyntax)
                .Select(s => s.FirstName)
                .ToArray();
            
            Assert.Equal(expectedArr[0], actual[0]);
            Assert.Equal(expectedArr[1], actual[1]);
            Assert.Equal(expectedArr[2], actual[2]);
            Assert.Equal(expectedArr[3], actual[3]);
            Assert.Equal(expectedArr[4], actual[4]);
        }

        [Fact]
        public void SeparatePrimaryAndSecondaryParameters()
        {
            var filterString = "Fields.House=Hufflepuff,Firstname=Luna";
            var filters = ServiceLocator.GetService<ISyntaxParser>().ParseParameters<FilterParameter, TestPersonModel>(filterString);
            var ds = (DynamicLinqBaseDataSyntaxModel) _dynamicLinqSyntaxBuilder.BuildDataSyntax(filters);
            Assert.NotEmpty(ds.PrimarySortSyntax);
            Assert.NotEmpty(ds.SecondarySortSyntax);
        }
    }
}
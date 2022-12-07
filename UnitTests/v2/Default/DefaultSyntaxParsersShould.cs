using System;
using System.Linq;
using UnitTests.v2.Models;
using Valid_DynamicFilterSort;
using Valid_DynamicFilterSort.Defaults.SyntaxParser;
using Valid_DynamicFilterSort.Enums;
using Valid_DynamicFilterSort.Extensions;
using Valid_DynamicFilterSort.Interfaces;
using Valid_DynamicFilterSort.Models;
using Valid_DynamicFilterSort.Utilities;
using Xunit;

namespace UnitTests.v2.Default
{
    public class DefaultSyntaxParsersShould : TestBase
    {
        private readonly ISyntaxParser _defaultParser;

        public DefaultSyntaxParsersShould()
        {
            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }

            _defaultParser = ServiceLocator.GetServices<ISyntaxParser>().FirstOrDefault(f => f is DefaultSyntaxParser);
        }
        
        [Fact]
        public void ParseManyFilterParameters()
        {
            const string filterString = "id!=6,lastName=smith%,||lastname=jones,createdTimestamp>=1970-01-01%";
            var filterParameters = _defaultParser
                .ParseParameters<FilterParameter, TestPersonModel>(filterString)
                .ToArray();
            
            Assert.Equal(4, filterParameters.Length);

            var zero = filterParameters[0];
            var one = filterParameters[1];
            var two = filterParameters[2];
            var three = filterParameters[3];
            
            Assert.Equal(OperatorEnum.NotEqual, zero.Operator);
            Assert.Equal(OperatorCombinerEnum.DEFAULT, zero.Combiner);
            Assert.True(zero.TryGetValue("ValueString", out var vso));
            Assert.Equal("6", vso.ToString());
            Assert.Equal(0, zero.Order);
            Assert.Equal(OperatorEnum.EqualTo, one.Operator);
            Assert.Equal(ComparisonTypeEnum.StartsWith, one.ComparisonType);
            Assert.Equal(OperatorCombinerEnum.OR,two.Combiner);
            Assert.Equal(2, two.Order);
            Assert.Equal(3, three.Order);
            Assert.Equal(OperatorEnum.GreaterThanOrEqualTo, three.Operator);
            Assert.Equal(typeof(DateTime), three.PropertyInfo.Type);
        }

        [Fact]
        public void ParseManySortParameters()
        {
            const string sortString = "createdtimestamp=desc,lastname=asc,firstname=ascending";
            var sortParameters = _defaultParser
                .ParseParameters<SortParameter, TestPersonModel>(sortString)
                .ToArray();
            
            Assert.Equal(sortString.Split(',').Length, sortParameters.Length);

            var zero = sortParameters[0];
            var one = sortParameters[1];
            var two = sortParameters[2];
            
            Assert.Equal("createdtimestamp",zero.Key);
            Assert.Equal(SortDirectionEnum.Descending, zero.Value);
            
            Assert.Equal("lastname",one.Key);
            Assert.Equal(SortDirectionEnum.Ascending, one.Value);
            
            Assert.Equal("firstname",two.Key);
            Assert.Equal(SortDirectionEnum.Asc, two.Value);
        }

        [Fact]
        public void ThrowOnInvalidSyntax()
        {
            var filterException = Assert.Throws<System.FormatException>(() =>
            {
                _defaultParser.ParseParameters<FilterParameter, TestPersonModel>("firstname|equals|bob");
            });

            Assert.Contains(DynamicFilterSortErrors.PARSE_SYNTAX_GENERAL_ERROR().Message, filterException.Message);

            var sortException = Assert.Throws<System.FormatException>(() =>
            {
                _defaultParser.ParseParameters<SortParameter, TestPersonModel>("lastlogindate desc");
            });
            
            Assert.Contains(DynamicFilterSortErrors.PARSE_SYNTAX_GENERAL_ERROR().Message, sortException.Message);
        }
    }
}
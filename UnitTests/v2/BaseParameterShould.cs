using System;
using Valid_DynamicFilterSort.Enums;
using Valid_DynamicFilterSort.Models;
using Xunit;

namespace UnitTests.v2
{
    public class BaseParameterShould : TestBase
    {
        [Fact]
        public void SetPropertyViaFieldsIfExistsAndTypeMatches()
        {
            var filterParameter = new FilterParameter();
            filterParameter.AddOrUpdateValue("operator", OperatorEnum.EqualTo);
            
            Assert.Equal(OperatorEnum.EqualTo, filterParameter.Operator);
            Assert.Empty(filterParameter.Fields);
        }

        [Fact]
        public void NotSetPropertyViaFieldsAndThrowWhenTypeDoesNotMatch()
        {
            var filterParameter = new FilterParameter();
            var exc = Assert.Throws<InvalidCastException>(() => filterParameter.AddOrUpdateValue("operator", "water buffalo"));
            
            Assert.Contains("operator", exc.Message);
        }

        [Fact]
        public void AddAndUpdateFieldIfNotAProperty()
        {
            var filterParameter = new FilterParameter();
            filterParameter.AddOrUpdateValue("test", 1);
            
            Assert.True(filterParameter.TryGetValue("test", out var value1));
            Assert.Equal(1, value1);

            filterParameter.AddOrUpdateValue("test", "42");

            Assert.True(filterParameter.TryGetValue("test", out var value42));
            Assert.Equal("42", value42);
        }

        [Fact]
        public void ReturnFalseWhenGettingAFieldThatDoesNotExist()
        {
            var filterParameter = new FilterParameter();
            
            Assert.False(filterParameter.TryGetValue("doesNotExist", out var shouldBeNull));
            Assert.Null(shouldBeNull);
        }

        [Fact]
        public void SuccessfullyRemoveAFieldByKey()
        {
            var filterParameter = new FilterParameter();

            filterParameter.AddOrUpdateValue("test", true);

            Assert.Contains("test", filterParameter.Fields.Keys);

            filterParameter.RemoveField("test");
            
            Assert.DoesNotContain("test", filterParameter.Fields.Keys);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnitTests.v2.Data;
using UnitTests.v2.Models;
using Valid_DynamicFilterSort;
using Valid_DynamicFilterSort.Extensions;
using Xunit;

namespace UnitTests.v2
{
    public class PropertyExtensionShould : TestBase
    {
        public PropertyExtensionShould()
        {
            if (!DynamicFilterSort.IsConfigured())
            {
                this.UseDynamicFilterSort();
            }
        }
        
        [Fact]
        public void FindCorrectPathTopLevel()
        {
            var actual = nameof(TestGroupModel.GroupName);
            var info = PropertyExtension.GetPropertyInformation<TestGroupModel>(actual.ToLower());
            Assert.Equal(actual, info.Key);
            Assert.Equal(actual, info.PathKey);
            Assert.Equal(typeof(string), info.Type);
        }
        
        [Fact]
        public void ThrowWhenTopLevelPathDoesNotExist()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                PropertyExtension.GetPropertyInformation<TestGroupModel>("ThisPropertyDoesNotExist"));
            
            Assert.Contains(DynamicFilterSortErrors.PROPERTY_NAME_INVALID().Message, exception.Message);
        }

        [Fact]
        public void FindCorrectPathNested()
        {
            var actual = $"{nameof(TestGroupModel.Owner)}.{nameof(TestPersonModel.Id)}";
            var info = PropertyExtension.GetPropertyInformation<TestGroupModel>(actual.ToLower());
            
            Assert.Equal(nameof(TestPersonModel.Id), info.Key);
            Assert.Equal(actual, info.PathKey);
            Assert.Equal(typeof(int), info.Type);
        }
        
        [Fact]
        public void ThrowWhenNestedPathDoesNotExist()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                PropertyExtension.GetPropertyInformation<TestGroupModel>($"{nameof(TestGroupModel.Owner)}.ThisPropertyDoesNotExist"));
            
            Assert.Contains(DynamicFilterSortErrors.PROPERTY_NAME_INVALID().Message, exception.Message);
        }

        [Fact]
        public void FindCorrectPathWithDictionary()
        {
            var actual = $"{nameof(TestGroupModel.Owner)}.{nameof(TestPersonModel.GroupMemberships)}.42";
            var info = PropertyExtension.GetPropertyInformation<TestGroupModel>(actual.ToLower());
            
            Assert.Equal("42", info.Key);
            Assert.Equal(actual, info.PathKey);
            Assert.Equal(typeof(TestGroupModel), info.Type);
        }
        
        [Fact]
        public void ThrowIfDictionaryKeyIsInvalidType()
        {
            var path = $"{nameof(TestGroupModel.Owner)}.{nameof(TestPersonModel.GroupMemberships)}.FortyTwo";
            var exception = Assert.Throws<ArgumentException>(() => 
                PropertyExtension.GetPropertyInformation<TestGroupModel>(path.ToLower()));
            
            Assert.Contains(DynamicFilterSortErrors.PROPERTY_NAME_INVALID().Message, exception.Message);
        }
        
        [Fact]
        public void FindCorrectPathWithDictionaryTraversal()
        {
            var actual = $"{nameof(TestGroupModel.Owner)}.{nameof(TestPersonModel.GroupMemberships)}.42.{nameof(TestGroupModel.GroupName)}";
            var info = PropertyExtension.GetPropertyInformation<TestGroupModel>(actual.ToLower());
            
            Assert.Equal(nameof(TestGroupModel.GroupName), info.Key);
            Assert.Equal(actual, info.PathKey);
            Assert.Equal(typeof(string), info.Type);
        }
        
        [Fact]
        public void SupportsMultipleDelimiters()
        {
            var input = $"{nameof(TestGroupModel.Owner)}.{nameof(TestPersonModel.GroupMemberships)}->42->>{nameof(TestGroupModel.GroupName)}";
            var info = PropertyExtension.GetPropertyInformation<TestGroupModel>(input.ToLower(), "->","->>",".");
            
            Assert.Equal(nameof(TestGroupModel.GroupName), info.Key);
            Assert.Equal($"{nameof(TestGroupModel.Owner)}->{nameof(TestPersonModel.GroupMemberships)}->42->{nameof(TestGroupModel.GroupName)}", info.PathKey);
            Assert.Equal(typeof(string), info.Type);
        }

        [Fact]
        public void CreateDefaultValue()
        {
            var data = new TestData();
            var model = data.Groups.First();
            var propInfo = PropertyExtension.GetPropertyInformation<TestGroupModel>(
                $"{nameof(TestGroupModel.Owner)}.{nameof(TestPersonModel.Fields)}.thisPropertyDoesNotExist.NeitherDoesThisOne");
            
            model.CreateDefaultValueIfNotExist(propInfo);
            
            Assert.Contains("thispropertydoesnotexist", model.Owner.Fields.Keys);
            Assert.Contains("neitherdoesthisone", ((Dictionary<object, object>) model.Owner.Fields["thispropertydoesnotexist"]).Keys);
        }
        
        [Fact]
        public void CreateDefaultValueAdvanced()
        {
            var data = new BaseTest();
            data.SetUp();
            var model = data.children.First(f => !f.par.fields.ContainsKey("expirationdate"));
            var propInfo = PropertyExtension.GetPropertyInformation<ChildModel>("par.fields.data.dict.expirationDate");
            
            model.CreateDefaultValueIfNotExist(propInfo);
            
            Assert.Contains("expirationdate", ((Dictionary<string, object>)model.par.fields["dict"]).Keys);
            data.TearDown();
        }
    }
}
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using Valid_DynamicFilterSort;
using Valid_DynamicFilterSort.Defaults;
using Valid_DynamicFilterSort.Enums;
using Valid_DynamicFilterSort.Interfaces;
using Xunit;

namespace UnitTests.v2.Default
{
    public class DefaultDataTypeValueHandlerShould : TestBase
    {
        #region setup
        private readonly IDataTypeValueHandler _service;

        private readonly string[] _inputs;

        private class JsonTest1
        {
            public string Name { get; set; }
            public int Age { get; set; }

            public JsonTest1()
            {
                
            }
        }

        private class JsonTest2
        {
            public string Name { get; set; }
            public List<string> Friends { get; set; } = new List<string>();
            public Json Fields { get; set; } = new Json();

            public JsonTest2()
            {
                
            }
        }

        public DefaultDataTypeValueHandlerShould()
        {
            var logger = new Mock<ILogger>();

            _service = new DefaultDataTypeValueHandler(logger.Object);
            _inputs = new[]
            {
                "",
                "this is a string",
                "also a string",
                "42",
                "-6",
                "2019-01-01 13:15:16.171",
                "01/01/2019",
                "2019-01-01",
                "F37A417D-3707-4260-8C16-582A31B131EA",
                "{\"name\":\"John\",\"age\": 48}",
                "{\"name\":\"John\",\"friends\":[\"Jack\",\"Kate\"],\"fields\":{\"gender\":\"M\",\"occupation\":\"Office Worker\",\"luckynumbers\":[4,8,15,16,23,42]}}",
                "true",
                "false"
                
            };
        }
        #endregion setup
        private (int successCount, int failureCount) ValidateInputs(DataTypeEnum dataType)
        {
            var successes = new List<string>();
            var failures = new List<string>();

            foreach (var i in _inputs)
            {
                if (_service.Validate(i, dataType))
                {
                    successes.Add(i);
                }
                else
                {
                    failures.Add(i);
                }
            }

            return (successes.Count, failures.Count);
        }

        [Fact]
        public void Convert()
        {
            Assert.Equal(DataTypeEnum.Text,_service.ConvertString<DataTypeEnum>("text"));
            Assert.Equal(42,_service.ConvertString<long>("42"));
            Assert.Equal(42,_service.ConvertString<double>("42"));
            Assert.Equal(42,_service.ConvertString<float>("42"));
            Assert.Equal("42",_service.ConvertString<string>("42"));
            Assert.Equal("42".ToCharArray(),_service.ConvertString<char[]>("42"));
            Assert.Equal('4',_service.ConvertString<char>("42"));
            Assert.True(_service.ConvertString<bool>("true"));
            Assert.False(_service.ConvertString<bool>("false"));
            Assert.Null(_service.ConvertString<bool?>(""));
            Assert.Equal(new DateTime(2019,01,02,03,04,05,678), _service.ConvertString<DateTime>("2019-01-02 03:04:05.678"));
            Assert.Null(_service.ConvertString<DateTime?>(""));
            Assert.Equal(Guid.Parse("A8AAE16B-78DC-44B5-99FA-3E6409F15B02"),_service.ConvertString<Guid?>("A8AAE16B-78DC-44B5-99FA-3E6409F15B02"));
            Assert.Null(_service.ConvertString<Guid?>(""));
            Assert.Equal((long) 48, _service.ConvertString<Json>("{\"name\":\"John\",\"age\": 48}")["age"]);
            Assert.Equal("John",_service.ConvertString<JsonTest1>("{\"name\":\"John\",\"age\": 48}").Name);
            Assert.Equal("John", _service.ConvertString<Dictionary<string,object>>("{\"name\":\"John\",\"age\": 48}")["name"]);
        }
        
        [Fact]
        public void ValidateString()
        {
            var (successCount, failureCount) = ValidateInputs(DataTypeEnum.Text);
            Assert.Equal(13,successCount);
            Assert.Equal(0,failureCount);
            Assert.True(_service.Validate("yes,no,maybe",DataTypeEnum.Text));
            
            Assert.True(_service.ValidateAndConvert<char>("string", DataTypeEnum.Text, out var ch));
            Assert.Equal('s', ch);

            Assert.True(_service.ValidateAndConvert<char[]>("string", DataTypeEnum.Text, out var charr));
            Assert.Equal("string".ToCharArray(), charr);

            Assert.True(_service.ValidateAndConvert<string>("string", DataTypeEnum.Text, out var str));
            Assert.Equal("string", str);
            
            Assert.True(_service.ValidateAndConvert<string[]>("string,strung", DataTypeEnum.Text, out var strarr));
            Assert.Equal(2, strarr.Length);
            
            Assert.True(_service.ValidateAndConvert<List<string>>("string,strung", DataTypeEnum.Text, out var strlist));
            Assert.Equal(2, strlist.Count);
        }
        
        [Fact]
        public void ValidateNumber()
        {
            var (successCount, failureCount) = ValidateInputs(DataTypeEnum.Number);
            Assert.Equal(3,successCount);
            Assert.Equal(10,failureCount);
            Assert.False(_service.Validate("-1,1,2,3,5,6.75",DataTypeEnum.Number));
            
            Assert.True(_service.ValidateAndConvert<int>("42.1", DataTypeEnum.Number, out var integ));
            Assert.Equal((int)42, integ);
            
            Assert.True(_service.ValidateAndConvert<double>("-42.1", DataTypeEnum.Number, out var dbl));
            Assert.Equal((double)-42.1, dbl);
            
            Assert.True(_service.ValidateAndConvert<float>("42.1", DataTypeEnum.Number, out var flt));
            Assert.Equal((float)42.1f, flt);
            
            Assert.True(_service.ValidateAndConvert<long>("42", DataTypeEnum.Number, out var lng));
            Assert.Equal((long)42, lng);
            
            Assert.True(_service.ValidateAndConvert<int?>(" ", DataTypeEnum.Number, out var ninteg));
            Assert.Null(ninteg);
            
            Assert.True(_service.ValidateAndConvert<List<int>>("4,8,15,16,23,42", DataTypeEnum.Number, out var nums));
            Assert.Equal(6, nums.Count);
            
            Assert.False(_service.ValidateAndConvert<int>("obviously a string",DataTypeEnum.Number, out var str));
        }
        
        [Fact]
        public void ValidateDateTime()
        {
            var (successCount, failureCount) = ValidateInputs(DataTypeEnum.DateTime);
            Assert.Equal(4,successCount);
            Assert.Equal(9,failureCount);
            Assert.False(_service.Validate("2019-01-01,2019-01-02,01/03/2019",DataTypeEnum.DateTime));
            
            Assert.True(_service.ValidateAndConvert<DateTime>("2019-01-01",DataTypeEnum.DateTime, out var dt1));
            Assert.Equal(new DateTime(2019, 01, 01), dt1);
            
            Assert.True(_service.ValidateAndConvert<DateTime>("2019-01-01 01:23:45.678",DataTypeEnum.DateTime, out var dt2));
            Assert.Equal(new DateTime(2019, 01, 01,01,23,45,678), dt2);
            
            Assert.True(_service.ValidateAndConvert<DateTime>("01-01-2019",DataTypeEnum.DateTime, out var dt3));
            Assert.Equal(new DateTime(2019, 01, 01), dt3);
            
            Assert.True(_service.ValidateAndConvert<DateTime>("01/01/2019",DataTypeEnum.DateTime, out var dt4));
            Assert.Equal(new DateTime(2019, 01, 01), dt4);
            
            Assert.True(_service.ValidateAndConvert<DateTime?>("",DataTypeEnum.DateTime, out var dt5));
            Assert.Null(dt5);
            
            Assert.True(_service.ValidateAndConvert<List<DateTime>>("2019-01-01,2019-01-02,2019-01-03", DataTypeEnum.DateTime, out var dt6));
            Assert.Equal(3, dt6.Count);
            
            Assert.False(_service.ValidateAndConvert<DateTime>("obviously a string",DataTypeEnum.Number, out var str));
        }
        
        [Fact]
        public void ValidateBoolean()
        {
            var (successCount, failureCount) = ValidateInputs(DataTypeEnum.Boolean);
            Assert.Equal(3,successCount);
            Assert.Equal(10,failureCount);
            Assert.False(_service.Validate("true,false,true",DataTypeEnum.Boolean));

            Assert.True(_service.ValidateAndConvert<bool>("true", DataTypeEnum.Boolean, out var b1));
            Assert.True(b1);
            
            Assert.True(_service.ValidateAndConvert<bool>("false", DataTypeEnum.Boolean, out var b2));
            Assert.False(b2);
            
            Assert.True(_service.ValidateAndConvert<bool?>("", DataTypeEnum.Boolean, out var b3));
            Assert.Null(b3);
            
            Assert.False(_service.ValidateAndConvert<bool>("obviously a string",DataTypeEnum.Number, out var str));
        }

        [Fact]
        public void ValidateGuid()
        {
            var (successCount, failureCount) = ValidateInputs(DataTypeEnum.Guid);
            Assert.Equal(2,successCount);
            Assert.Equal(11,failureCount);
            Assert.False(_service.Validate("A8AAE16B-78DC-44B5-99FA-3E6409F15B02,A8AAE16B-78DC-44B5-99FA-3E6409F15B03",DataTypeEnum.Guid));
            
            Assert.True(_service.ValidateAndConvert<Guid>("A8AAE16B-78DC-44B5-99FA-3E6409F15B02", DataTypeEnum.Guid, out var guid1));
            Assert.Equal(Guid.Parse("A8AAE16B-78DC-44B5-99FA-3E6409F15B02"), guid1);
            
            Assert.True(_service.ValidateAndConvert<Guid?>("A8AAE16B-78DC-44B5-99FA-3E6409F15B02", DataTypeEnum.Guid, out var guid2));
            Assert.Equal(Guid.Parse("A8AAE16B-78DC-44B5-99FA-3E6409F15B02"), guid2);
            
            Assert.True(_service.ValidateAndConvert<Guid?>("                                    ", DataTypeEnum.Guid, out var guid3));
            Assert.Null(guid3);
            
            Assert.True(_service.ValidateAndConvert<Guid[]>("A8AAE16B-78DC-44B5-99FA-3E6409F15B02,A8AAE16B-78DC-44B5-99FA-3E6409F15B03", DataTypeEnum.Guid, out var guid4));
            Assert.Equal(2, guid4.Length);
            
            
            Assert.False(_service.ValidateAndConvert<Guid>("obviously a string",DataTypeEnum.Number, out var str));
        }

        [Fact]
        public void ValidateJson()
        {
            var (successCount, failureCount) = ValidateInputs(DataTypeEnum.Json);
            Assert.Equal(3,successCount);
            Assert.Equal(10,failureCount);

            var json1 = "{\"name\":\"John\",\"age\": 48}";
            var json2 = "{\"name\":\"John\",\"friends\":[\"Jack\",\"Kate\"],\"fields\":{\"gender\":\"M\",\"occupation\":\"Office Worker\",\"luckynumbers\":[4,8,15,16,23,42]}}";
            
            Assert.True(_service.ValidateAndConvert<Json>(json1,DataTypeEnum.Json,out var j1));
            Assert.Equal("John", j1["name"]);
            Assert.Equal((long) 48, j1["age"]);
            
            Assert.True(_service.ValidateAndConvert<JsonTest1>(json1,DataTypeEnum.Json,out var jt1));
            Assert.Equal("John", jt1.Name);
            Assert.Equal(48, jt1.Age);

            Assert.True(_service.ValidateAndConvert<Json>(json2,DataTypeEnum.Json,out var j2));
            Assert.Equal("John", j2["name"]);
            Assert.Equal(2, ((JArray)j2["friends"]).Count);
            
            Assert.True(_service.ValidateAndConvert<JsonTest2>(json2,DataTypeEnum.Json,out var jt2));
            Assert.Equal("John", jt2.Name);
            Assert.Equal(2, jt2.Friends.Count);
            Assert.Equal(6, ((JArray)jt2.Fields["luckynumbers"]).Count);
            Assert.Equal("M", jt2.Fields["gender"]);
        }
    }
}
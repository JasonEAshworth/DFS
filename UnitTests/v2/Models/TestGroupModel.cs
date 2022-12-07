using System;
using System.Collections.Generic;
using Valid_DynamicFilterSort;

namespace UnitTests.v2.Models
{
    public class TestGroupModel
    {
        public int Id { get; set; } = new Random(DateTime.UtcNow.GetHashCode()).Next(100, int.MaxValue);
        public string GroupName { get; set; }
        public Dictionary<int, TestPersonModel> Members { get; set; } = new Dictionary<int, TestPersonModel>();
        public Json Fields { get; set; }
        public TestPersonModel Owner { get; set; }
        public int OwnerId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
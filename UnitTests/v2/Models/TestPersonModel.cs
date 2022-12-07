using System;
using System.Collections.Generic;
using Valid_DynamicFilterSort;
using static System.Int32;

namespace UnitTests.v2.Models
{
    public class TestPersonModel
    {
        public int Id { get; set; } = new Random(DateTime.UtcNow.GetHashCode()).Next(100, MaxValue);
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<int, TestGroupModel> GroupMemberships { get; set; } = new Dictionary<int, TestGroupModel>();
        public Json Fields { get; set; } = new Json();

        public TestPersonModel()
        {
            
        }

        public TestPersonModel(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public TestPersonModel(string firstName, string lastName, params KeyValuePair<string, object>[] fields) : this(firstName, lastName)
        {
            if (fields == null) return;
            foreach (var f in fields)
            {
                Fields[f.Key] = f.Value;
            }
        }
    }
}
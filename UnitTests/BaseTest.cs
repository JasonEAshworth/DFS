using Dapper.FluentMap.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.FluentMap;
using Valid_DynamicFilterSort;

namespace UnitTests
{
    public enum TestEnum
    {
        WereNoStrangersToLove,
        YouKnowTheRulesAndSoDoI,
        AFullCommitmentsWhatImThinkingOf,
        YouWouldntGetThisFromAnyOtherGuy,
        IJustWannaTellYouHowImFeeling,
        GottaMakeYouUnderstand,
        NeverGonnaGiveYouUp,
        NeverGonnaLetYouDown,
        NeverGonnaRunAroundAndDesertYou,
        NeverGonnaMakeYouCry,
        NeverGonnaSayGoodbye,
        NeverGonnaTellALieAndHurtYou
    }
    
    public class AlphaModel
    {
        public bool alpha { get; set; }
        public DateTime created { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public TestEnum enumValue { get; set; }
    }

    public class BaseTest
    {
        public List<AlphaModel> alphas = new List<AlphaModel>();
        public List<ChildModel> children = new List<ChildModel>();
        public List<ParentModel> parents = new List<ParentModel>();

        public void SetUp()
        {
            FluentMapper.Initialize(config =>
            {
                if (!FluentMapper.EntityMaps.ContainsKey(typeof(ParentModel)))
                {
                    config.AddMap(new ParentModelMap());    
                }
            });
            
            int childId = 1;

            for (int i = 1; i <= 10; i++)
            {
                var alpha = new AlphaModel()
                {
                    id = i,
                    name = $"Alpha{i}",
                    alpha = i % 2 == 0,
                    created = i % 2 == 0 ? DateTime.UtcNow : DateTime.UtcNow.AddDays(i * -1)
                };

                alphas.Add(alpha);

                var parent = new ParentModel()
                {
                    id = i,
                    name = $"Parent{i}",
                    description = RandomString(100),
                    alpha = alpha,
                    alphaId = alpha.id,
                    fields = new Json()
                };

                parent.fields.Add("string", "thing");
                parent.fields.Add("bool", true);
                parent.fields.Add("number", i);
                parent.fields.Add("date", DateTime.UtcNow);
                parent.fields.Add("dict", new Dictionary<string, dynamic>());

                if (i % 2 == 0)
                {
                    ((Dictionary<string, dynamic>)parent.fields["dict"]).Add("string", "strung");
                    ((Dictionary<string, dynamic>)parent.fields["dict"]).Add("parentId", i);
                    ((Dictionary<string, dynamic>)parent.fields["dict"]).Add("expirationDate", DateTime.UtcNow.AddDays(i));
                }

                for (int j = 1; j <= 2; j++)
                {
                    var child = new ChildModel()
                    {
                        id = childId,
                        name = $"ChildOfParent{i}",
                        description = RandomString(50),
                        parId = i,
                        par = parent,
                        guid = Guid.NewGuid(),
                        nint = j == 1 ? null : (int?)42,
                        dt = DateTime.UtcNow,
                        ndt = j == 1 ? null : (DateTime?)DateTime.UtcNow,
                        nbool = j == 1 ? null : (bool?)true,
                        guidasstring = "38680661-0c3b-40c5-84c0-5d10a13061d" + i
                    };

                    children.Add(child);

                    childId++;
                }
            }
        }

        public void TearDown()
        {
            parents = null;
            children = null;
        }

        private string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class ChildModel
    {
        public bool active { get; set; }
        public string description { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public ParentModel par { get; set; }
        public int parId { get; set; }
        public Guid guid { get; set; }
        public int? nint { get; set; }
        public bool? nbool { get; set; }
        public DateTime dt { get; set; }
        public DateTime? ndt { get; set; }

        public string guidasstring { get; set; }
    }

    // public class Json : IDictionary<string, object>
    // {
    //     [JsonProperty] 
    //     [JsonExtensionData] 
    //     internal IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
    //
    //     public Json()
    //     {
    //         
    //     }
    //
    //     public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    //     {
    //         return Data.GetEnumerator();
    //     }
    //
    //     IEnumerator IEnumerable.GetEnumerator()
    //     {
    //         return GetEnumerator();
    //     }
    //
    //     public void Add(KeyValuePair<string, object> item)
    //     {
    //         Data.Add(item);
    //     }
    //
    //     public void Clear()
    //     {
    //         Data.Clear();
    //     }
    //
    //     public bool Contains(KeyValuePair<string, object> item)
    //     {
    //         return Data.Contains(item);
    //     }
    //
    //     public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    //     {
    //         Data.CopyTo(array, arrayIndex);
    //     }
    //
    //     public bool Remove(KeyValuePair<string, object> item)
    //     {
    //         return Data.Remove(item);
    //     }
    //
    //     public int Count => Data.Count;
    //     public bool IsReadOnly => Data.IsReadOnly;
    //     public void Add(string key, object value)
    //     {
    //         Data.Add(key, value);
    //     }
    //
    //     public bool ContainsKey(string key)
    //     {
    //         return Data.ContainsKey(key);
    //     }
    //
    //     public bool Remove(string key)
    //     {
    //         return Data.Remove(key);
    //     }
    //
    //     public bool TryGetValue(string key, out object value)
    //     {
    //         return Data.TryGetValue(key, out value);
    //     }
    //
    //     public object this[string key]
    //     {
    //         get => key.Equals("data", StringComparison.InvariantCultureIgnoreCase) && 
    //                !Data.ContainsKey("data") 
    //             ? Data 
    //             : Data[key];
    //         set => Data[key] = value;
    //     }
    //
    //     public ICollection<string> Keys => Data.Keys;
    //     public ICollection<object> Values => Data.Values;
    // }

    public class ParentModel
    {
        public bool active { get; set; } = true;
        public AlphaModel alpha { get; set; }
        public int alphaId { get; set; }
        public DateTime created { get; set; } = DateTime.UtcNow;
        public string description { get; set; }
        public int id { get; set; }
        public Json fields { get; set; }
        public DateTime modified { get; set; } = DateTime.UtcNow;
        public string name { get; set; }
        public Guid guid1 { get; set; } = Guid.NewGuid();
        public Guid? guid2 { get; set; } = Guid.Empty;
    }

    public class ParentModelMap : EntityMap<ParentModel>
    {
        public ParentModelMap()
        {
            Map(x => x.id).ToColumn("id");
            Map(x => x.fields).ToColumn("fields");
            Map(x => x.name).ToColumn("name");
            Map(x => x.description).ToColumn("description");
            Map(x => x.alphaId).ToColumn("alpha_id");
            Map(x => x.created).ToColumn("created_dt");
            Map(x => x.active).ToColumn("active");
            Map(x => x.modified).ToColumn("modified_dt");
            Map(x => x.guid1).ToColumn("guid1");
            Map(x => x.guid2).ToColumn("guid2");
        }
    }

    public class PModel<T> : Valid_DynamicFilterSort.IPaginationModel<T> where T : class
    {
        public int count { get; set; }
        public List<T> data { get; set; }
        public int offset { get; set; }
        public int total { get; set; }
    }
}
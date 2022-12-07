using System;
using System.Collections.Generic;
using UnitTests.v2.Models;

namespace UnitTests.v2.Data
{
    public class TestData
    {
        public List<TestPersonModel> Persons = new List<TestPersonModel>();
        public List<TestGroupModel> Groups = new List<TestGroupModel>();

        public TestData()
        {
            var order = 0;
            var aang = new TestPersonModel("Aang", "",
                new KeyValuePair<string, object>("gender", "male"),
                new KeyValuePair<string, object>("bender", true),
                new KeyValuePair<string, object>("order",order++));
            
            var katara = new TestPersonModel("Katara", "",
                new KeyValuePair<string, object>("gender", "female"),
                new KeyValuePair<string, object>("bender", true),
                new KeyValuePair<string, object>("order",order++));

            var sokka = new TestPersonModel("Sokka", "",
                new KeyValuePair<string, object>("gender", "male"),
                new KeyValuePair<string, object>("bender", false),
                new KeyValuePair<string, object>("guid", Guid.Empty),
                new KeyValuePair<string, object>("int", 42),
                new KeyValuePair<string, object>("string", "forty-two"),
                new KeyValuePair<string, object>("double", 42D),
                new KeyValuePair<string, object>("decimal", 42M),
                new KeyValuePair<string, object>("float", 42F),
                new KeyValuePair<string, object>("datetime", DateTime.UnixEpoch),
                new KeyValuePair<string, object>("order",order++)
            );
            
            var zuko = new TestPersonModel("Zuko", "", 
                new KeyValuePair<string, object>("gender", "male"),
                new KeyValuePair<string, object>("bender", true),
                new KeyValuePair<string, object>("order",order++));
            
            var iroh = new TestPersonModel("Iroh", "", 
                new KeyValuePair<string, object>("gender", "male"),
                new KeyValuePair<string, object>("bender", true),
                new KeyValuePair<string, object>("order",order++));
            
            var toph = new TestPersonModel("Toph", "Beifong", 
                new KeyValuePair<string, object>("gender", "female"),
                new KeyValuePair<string, object>("bender", true),
                new KeyValuePair<string, object>("order",order++));
            
            var azula = new TestPersonModel("Azula", "", 
                new KeyValuePair<string, object>("gender", "female"),
                new KeyValuePair<string, object>("bender", true),
                new KeyValuePair<string, object>("order",order++));
            
            var airTribe = new TestGroupModel
            {
                GroupName = "AirTribe",
                Owner = aang,
                OwnerId = aang.Id,
                Members = new Dictionary<int, TestPersonModel>{{aang.Id, aang}}
            };
            
            var airBenders = new TestGroupModel
            {
                GroupName = "AirBender",
                Owner = aang,
                OwnerId = aang.Id,
                Members = new Dictionary<int, TestPersonModel>{{aang.Id, aang}}
            };

            var waterTribe = new TestGroupModel
            {
                GroupName = "WaterTribe",
                Owner = sokka,
                OwnerId = sokka.Id,
                Members = new Dictionary<int, TestPersonModel>
                {
                    {katara.Id, katara},
                    {sokka.Id, sokka}
                }
            };
            
            var waterBenders = new TestGroupModel
            {
                GroupName = "WaterBender",
                Owner = katara,
                OwnerId = katara.Id,
                Members = new Dictionary<int, TestPersonModel>
                {
                    {katara.Id, katara},
                    {aang.Id, aang}
                }
            };
            
            var fireNation = new TestGroupModel
            {
                GroupName = "FireNation",
                Owner = zuko,
                OwnerId = zuko.Id,
                Members = new Dictionary<int, TestPersonModel>
                {
                    {zuko.Id, zuko},
                    {azula.Id, azula},
                    {iroh.Id, iroh}
                }
            };

            var fireBenders = new TestGroupModel
            {
                GroupName = "FireBender",
                Owner = zuko,
                OwnerId = zuko.Id,
                Members = new Dictionary<int, TestPersonModel>
                {
                    {zuko.Id, zuko},
                    {azula.Id, azula},
                    {iroh.Id, iroh},
                    {aang.Id, aang}
                }
            };

            var earthNation = new TestGroupModel
            {
                GroupName = "EarthNation",
                Owner = toph,
                OwnerId = toph.Id,
                Members = new Dictionary<int, TestPersonModel>
                {
                    {toph.Id, toph}
                }
            };
            
            var earthBenders = new TestGroupModel
            {
                GroupName = "EarthBender",
                Owner = toph,
                OwnerId = toph.Id,
                Members = new Dictionary<int, TestPersonModel>
                {
                    {toph.Id, toph},
                    {aang.Id, aang}
                }
            };
            
            aang.GroupMemberships.Add(airTribe.Id, airTribe);
            aang.GroupMemberships.Add(airBenders.Id, airBenders);
            aang.GroupMemberships.Add(waterBenders.Id, waterBenders);
            aang.GroupMemberships.Add(earthBenders.Id, earthBenders);
            aang.GroupMemberships.Add(fireBenders.Id, fireBenders);
            
            katara.GroupMemberships.Add(waterTribe.Id, waterTribe);
            katara.GroupMemberships.Add(waterBenders.Id, waterBenders);
            
            sokka.GroupMemberships.Add(waterTribe.Id, waterTribe);
            
            zuko.GroupMemberships.Add(fireNation.Id, fireNation);
            zuko.GroupMemberships.Add(fireBenders.Id, fireNation);
            
            iroh.GroupMemberships.Add(fireNation.Id, fireNation);
            iroh.GroupMemberships.Add(fireBenders.Id, fireNation);
            
            azula.GroupMemberships.Add(fireNation.Id, fireNation);
            azula.GroupMemberships.Add(fireBenders.Id, fireNation);

            toph.GroupMemberships.Add(earthNation.Id, earthNation);
            toph.GroupMemberships.Add(earthBenders.Id, earthBenders);
            
            Persons.AddRange(new [] {aang, katara, sokka, zuko, iroh, azula, toph});
            Groups.AddRange(new [] {airTribe, airBenders, waterTribe, waterBenders, earthNation, earthBenders, fireNation, fireBenders});
        }
    }
}
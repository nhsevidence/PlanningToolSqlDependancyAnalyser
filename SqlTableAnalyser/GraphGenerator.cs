using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Neo4jClient;
using SqlDependancyAnalyser;

namespace SqlTableAnalyser
{
    public class GraphGenerator
    {
        private readonly GraphClient _graphClient;
        
        public GraphGenerator()
        {
            _graphClient = new GraphClient(
                new Uri(ConfigurationManager.AppSettings.Get("GraphDBUrl")),
                ConfigurationManager.AppSettings.Get("GraphDBUser"),
                ConfigurationManager.AppSettings.Get("GraphDBPassword"));
            _graphClient.Connect();
            
        }

        public void ResetGraphDatabase()
        {
            _graphClient.Cypher
                .Match("(n)")
                .DetachDelete("n")
                .ExecuteWithoutResults();
        }

        public void GenerateGraph(List<string> planningApps, Dictionary<string, List<string>> dbObjectApps)
        {
            planningApps.ForEach(planningApp =>
            {
                var newPlanningApplication = new PlanningApplication { Name = planningApp };
                _graphClient.Cypher
                    .Merge("(planningApplication:PlanningApplication { Name: {name} })")
                    .OnCreate()
                    .Set("planningApplication = {newPlanningApplication}")
                    .WithParams(new { name = newPlanningApplication.Name, newPlanningApplication})
                    .ExecuteWithoutResults();
            });

            foreach (var dbObject in dbObjectApps)
            {
                var newDbObject = new DbObject { Name = dbObject.Key };
                _graphClient.Cypher
                    .Merge("(dbObject:DbObject { Name: {name} })")
                    .OnCreate()
                    .Set("dbObject = {newDbObject}")
                    .WithParams(new { name = newDbObject.Name, newDbObject})
                    .ExecuteWithoutResults();
                foreach (var planningApp in dbObject.Value)
                {
                    _graphClient.Cypher
                        .Match("(dbObjectNode:DbObject)", "(planningApplicationNode:PlanningApplication)")
                        .Where((DbObject dbObjectNode) => dbObjectNode.Name == dbObject.Key)
                        .AndWhere((PlanningApplication planningApplicationNode) => planningApplicationNode.Name == planningApp)
                        .CreateUnique("(dbObjectNode)-[:USED_BY]->(planningApplicationNode)")
                        .ExecuteWithoutResults();
                }
            }
        }
    }
    
    public class DbObject
    {
        public string Name { get; set; }
    }
    public class PlanningApplication
    {
        public string Name{ get; set; }
    }
}
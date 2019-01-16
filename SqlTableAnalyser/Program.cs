using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Serilog;
using SqlTableAnalyser;

namespace SqlDependancyAnalyser
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var sqlConnectionString = ConfigurationManager.AppSettings.Get("sqlConnectionString");
            var diffFilePath = ConfigurationManager.AppSettings.Get("diffFilePath");
            var planningAppsConfig = ConfigurationManager.GetSection("planningApps") as NameValueCollection;
            var planningApps = new Dictionary<string, string>();
            var planningAppsDataAccess = new Dictionary<string, string>();
            var generateRandom = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("generateRandom"));
            var useGraphDb = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("useGraphDB"));

            planningAppsConfig.AllKeys.ToList().ForEach(planningApp => 
                planningApps.Add(planningApp, planningAppsConfig[planningApp]));

            log.Information("planningApps to analyse {0}", planningApps.Keys);

            foreach (var app in planningApps)
            {
                var dataAccess = File.ReadAllText(app.Value);
                planningAppsDataAccess.Add(app.Key,dataAccess);
            }
            var diffFileContent = File.ReadAllText(diffFilePath);
            var sprocNameParser = new SprocNameParser();
            var sprocDepAnalyser = new SprocDependancyAnalyser(sqlConnectionString);
            var dbObjectApps = new Dictionary<string, List<string>>();

            foreach (var planningAppDataAccess in planningAppsDataAccess)
            {
                // Step 1 - parse stored procedure names
                var storedProcedureNames = sprocNameParser.ParseSprocsNamesFrom(planningAppDataAccess.Value);
                log.Information("stored procedure names for {0}: {1}", planningAppDataAccess.Key, storedProcedureNames.Count);

                // Step 2 - find dependent sql objects for stored procedures
                var appObjectDependancies = sprocDepAnalyser.FindDependantObjectsForSprocs(storedProcedureNames);
                log.Information("object dependencies for for {0}: {1}", planningAppDataAccess.Key, appObjectDependancies.Count);

                // Step 3 - Find objects that have associated db diffs
                var objectsWithinDiff = new DiffChecker().FindObjsWithAssociatedDbDiffs(appObjectDependancies, diffFileContent);
                log.Information("object that are within the diff for {0}: {1}", planningAppDataAccess.Key, objectsWithinDiff.Count);
                
                // Step 4 - Add app to db object list
                foreach (var diffObject in objectsWithinDiff)
                {
                    if (dbObjectApps.ContainsKey(diffObject))
                    {
                        dbObjectApps[diffObject].Add(planningAppDataAccess.Key);
                    }
                    else
                    {
                        dbObjectApps.Add(diffObject, new List<string> { planningAppDataAccess.Key });
                    }
                }
                log.Information("added {0} app to dbObjectApps", planningAppDataAccess.Key);
            }
            log.Information("db objects: {0}", dbObjectApps.Count);

            var serializer = new JsonSerializer {Formatting = Formatting.Indented};
            using (var sw = new StreamWriter(ConfigurationManager.AppSettings.Get("outputFilePath")))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, dbObjectApps);
            }
            log.Information("object to app map written to {0}", ConfigurationManager.AppSettings.Get("outputFilePath"));

            if (generateRandom)
            {
                var sampleSize = Math.Round(dbObjectApps.Count / (1 + (dbObjectApps.Count * Math.Pow(0.20, 2))));// Slovin formula
                var randomDbObjectKeyIndexes = new HashSet<int>();
                //add unique random number to 
                while (randomDbObjectKeyIndexes.Count != sampleSize)
                {
                    randomDbObjectKeyIndexes.Add(new Random().Next(1, dbObjectApps.Count));
                }
    
                var randomDbObjects = randomDbObjectKeyIndexes.ToDictionary(k => k, v => dbObjectApps.Keys.ElementAt(v-1)).OrderBy(x=>x.Key);
                
                log.Information("random sample size {0}", sampleSize);
                log.Information("random DbObject indexes {0}", randomDbObjectKeyIndexes.OrderBy(x => x));
                log.Information("random DbObjects {0}", randomDbObjects);
            }

            if (useGraphDb)
            {
                var graphGenerator = new GraphGenerator();
                graphGenerator.GenerateGraph(planningApps.Keys.ToList(), dbObjectApps);
            }
        }
    }
}

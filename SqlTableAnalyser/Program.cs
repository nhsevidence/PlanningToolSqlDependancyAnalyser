using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlTableAnalyser;

namespace SqlDependancyAnalyser
{
    class Program
    {
        
        static void Main(string[] args)
        {
            string sqlConnectionString = "server=;database=;Persist Security Info=True;User ID=;Password=";
            string diffFilePath = @"C:\src\UAT_LIVE_Diffs.sql";

            var apps = new Dictionary<string, string>()
            {
                { "UsersPermissions", @"C:\src\planning-users-permissions\UsersPermissions\App_Code\DataAccess.cs"},
                { "Contacts", @"C:\src\planning-contacts\Contacts\App_Code\DataAccess.cs"},
                { "Contacts2", @"C:\src\planning-contacts\Contacts\App_Code\ContactsDB.cs"},
                { "TA", @"C:\src\planning-appraisals\Appraisals\App_Code\DataAccess.cs"},
                { "Guidelines", @"C:\src\planning-guidelines\Guidelines\App_Code\DataAccess.cs"},
                { "IP", @"C:\src\planning-interventional-procedures\IP\App_Code\DataAccess.cs"},
                { "IP2", @"C:\src\planning-interventional-procedures\IP\App_Code\ContactsDB.cs"},
                { "Diagnostics", @"C:\src\planning-diagnostics\Diagnostics\App_Code\DataAccess.cs"},
                { "PIP", @"C:\src\planning-pip\PIP\App_Code\DataAccess.cs"},
                { "SR", @"C:\src\planning-surveillance-reviews\SR\App_Code\DataAccess.cs"},
                { "AI", @"C:\src\planning-adoptionimpact\AIP\App_Code\DataAccess.cs"},
                { "Registrations", @"C:\src\planning-registrations\Registrations\App_Code\DataAccess.cs"},
            };
            var appDataAccessContent = new Dictionary<string, string>();
            foreach (var app in apps)
            {
                var dataAccess = File.ReadAllText(app.Value);
                appDataAccessContent.Add(app.Key,dataAccess);
            }
            var diffFileContent = File.ReadAllText(diffFilePath);
            var sprocNameParser = new SprocNameParser();
            var sprocDepAnalyser = new SprocDependancyAnalyser(sqlConnectionString);
            var dbObjectApps = new Dictionary<string, List<string>>();

            foreach (var dataAccessContent in appDataAccessContent)
            {
                // Step 1 - parse sproc names
                var sprocNames = sprocNameParser.ParseSprocsNamesFrom(dataAccessContent.Value);
                Console.WriteLine($"got sproc names for {dataAccessContent.Key}");
                // Step 2 - find dependent sql objects for sproc
                var appObjectDependancies = sprocDepAnalyser.FindDependantObjectsForSprocs(sprocNames);
                Console.WriteLine($"got object dependencies for {dataAccessContent.Key}");
                // Step 3 - Find objects that have associated db diffs
                var objectsWithinDiff = new DiffChecker().FindObjsWithAssociatedDbDiffs(appObjectDependancies, diffFileContent);
                Console.WriteLine($"got object that are within the diff for {dataAccessContent.Key}");
                // Step 4 - Add app to object list
                foreach (var diffObject in objectsWithinDiff)
                {
                    if (dbObjectApps.ContainsKey(diffObject))
                    {
                        dbObjectApps[diffObject].Add(dataAccessContent.Key);
                    }
                    else
                    {
                        dbObjectApps.Add(diffObject, new List<string> { dataAccessContent.Key });
                    }
                }
                Console.WriteLine($"added apps to dbObjectApps for {dataAccessContent.Key}");
            }

            Console.WriteLine($"db object counts:{dbObjectApps.Count}");

            string dbObjectAppsOutput = "";
            foreach (var dbObject in dbObjectApps.OrderBy(x=>x.Key))
            {
                dbObjectAppsOutput = dbObjectAppsOutput +
                                     $"{dbObject.Key}:{string.Join(", ", dbObject.Value)}";
                Console.WriteLine($"db object {dbObject.Key} is used in {string.Join(", ", dbObject.Value)}");
            }

            File.WriteAllText(@"C:\src\dbObjectApps.txt", dbObjectAppsOutput);

            Console.ReadKey();
        }
    }
}

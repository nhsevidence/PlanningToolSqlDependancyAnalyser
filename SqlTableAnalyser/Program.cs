using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDependancyAnalyser
{
    class Program
    {
        
        static void Main(string[] args)
        {
            string sqlConnectionString = "server=;database=;Persist Security Info=True;User ID=;Password=";
            string dataAccessFilePath = @"C:\PATH_TO_DATAACCESS.CS";

            var fileContent = File.ReadAllText(dataAccessFilePath);

            var sprocNameParser = new SprocNameParser();
            var sprocNames = sprocNameParser.ParseSprocsNamesFrom(fileContent);

            
           
            var sprocTableAnalyser = new SprocDependancyAnalyser(sqlConnectionString);
            var dependancySet = new DependancySet();
            foreach (var sprocName in sprocNames)
            {
                var tableNames = sprocTableAnalyser.FindDependancies(sprocName);
                tableNames.ForEach(t => dependancySet.AddDependantObject(sprocName, t));
            }

            dependancySet.Print();

            Console.ReadKey();
        }
    }
}

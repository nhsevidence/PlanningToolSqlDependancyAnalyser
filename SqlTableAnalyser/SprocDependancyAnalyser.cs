using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SqlDependancyAnalyser
{
    public class SprocDependancyAnalyser
    {
        private readonly string _connectionString;

        public SprocDependancyAnalyser(string connectionString)
        {
            _connectionString = connectionString;
        }
        public List<string> FindDependantObjectForSproc(string sprocName)
        {
            var sqlQuery = String.Format(@"
SELECT OBJECT_NAME(referencing_id) AS sp_name,
    referenced_entity_name AS dependancy_name
FROM sys.sql_expression_dependencies AS sed  
INNER JOIN sys.objects AS o ON sed.referencing_id = o.object_id  
WHERE referencing_id = OBJECT_ID(N'dbo.{0}');  
  ", sprocName);

            var tableNames = new List<string>();
            using (var myConnection = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd =
                    new SqlCommand(sqlQuery,
                        myConnection))
                {
                    myConnection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Check is the reader has any rows at all before starting to read.
                        if (reader.HasRows)
                        {
                            // Read advances to the next row.
                            while (reader.Read())
                            {
                                var tableName = reader.GetString(reader.GetOrdinal("dependancy_name"));
                                tableNames.Add(tableName);

                            }
                        }
                    }
                }
            }

            return tableNames;

        }


        public SortedSet<string> FindDependantObjectsForSprocs(List<string> sprocNames)
        {
            var dependancySet = new SortedSet<string>();
            
            foreach (var sprocName in sprocNames)
            {
                dependancySet.Add(sprocName);
                var objNames = FindDependantObjectForSproc(sprocName);
                dependancySet.UnionWith(objNames);
            }

            return dependancySet;
        }
    }
}
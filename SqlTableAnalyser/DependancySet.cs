using System;
using System.Collections.Generic;

namespace SqlDependancyAnalyser
{
    internal class DependancySet 
    {
        private Dictionary<string, List<string>> _dependancySet;
        private HashSet<string> _uniqueSprocNames;
        private HashSet<string> _uniqueDependantObjectNames;

        public DependancySet()
        {
            _uniqueDependantObjectNames = new HashSet<string>();
            _uniqueSprocNames = new HashSet<string>();
            _dependancySet = new Dictionary<string, List<string>>();
        }

        public void AddDependantObject(string sprocName, string objName)
        {
            _uniqueSprocNames.Add(sprocName);
            _uniqueDependantObjectNames.Add(objName);
            //if (_dependancySet.ContainsKey(tableName))
            //{
            //    _dependancySet[tableName].Add(sprocName);
            //}
            //else
            //{
            //    _dependancySet.Add(tableName, new List<string>() {sprocName});
            //}
        }

        public void Print()
        {
            SortedSet<string> combined = new SortedSet<string>();
            combined.UnionWith(_uniqueSprocNames);
            combined.UnionWith(_uniqueDependantObjectNames);
            foreach (var dependancy in combined)
            {
                Console.WriteLine(dependancy);
            }

            //Console.WriteLine("Set of dependancies across all procedures:");

            //foreach (var dep in _dependancySet)
            //{
            //    var msg = $"{dep.Key} - Used by procs {string.Join(",", dep.Value)}";
            //    Console.WriteLine(msg);
            //}
        }
    }
}
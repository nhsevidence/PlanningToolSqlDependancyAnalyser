using System;
using System.Collections.Generic;

namespace SqlDependancyAnalyser
{
    public class AppDependancySet 
    {
        private SortedSet<string> _uniqueDependantObjNames;

        public AppDependancySet()
        {
            _uniqueDependantObjNames = new SortedSet<string>();
        }

        public void AddDependantObject(string objName)
        {
            _uniqueDependantObjNames.Add(objName);
        }

        public void Print()
        {
            foreach (var name in _uniqueDependantObjNames)
            {
                Console.WriteLine(name);
            }
        }
    }
}
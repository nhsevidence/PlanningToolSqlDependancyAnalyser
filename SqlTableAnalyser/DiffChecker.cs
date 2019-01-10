using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SqlTableAnalyser
{
    public class DiffChecker
    {
        public SortedSet<string> FindObjsWithAssociatedDbDiffs(SortedSet<string> objNames, string fileContent)
        {
            var diffObjects =  new SortedSet<string>();
            foreach (var obj in objNames)
            {
                if (Regex.IsMatch(fileContent, string.Format(@"\b{0}\b", Regex.Escape(obj))))
                {
                    Console.WriteLine($"{obj} was found in diff");
                    diffObjects.Add(obj);
                }
            }
            return diffObjects;
        }
    }
}
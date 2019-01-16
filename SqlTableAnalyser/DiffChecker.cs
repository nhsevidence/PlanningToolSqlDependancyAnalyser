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
                if (Regex.IsMatch(fileContent, $@"\b{Regex.Escape(obj)}\b"))
                {
                    diffObjects.Add(obj);
                }
            }
            return diffObjects;
        }
    }
}
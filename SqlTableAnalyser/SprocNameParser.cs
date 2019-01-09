using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SqlDependancyAnalyser
{
    public class SprocNameParser
    {
        public List<string> ParseSprocsNamesFrom(string codeStr)
        {
            var results = new List<string>();

            results.AddRange(ParseSprocNamesFromSqlCommandPattern(codeStr));
            results.AddRange(ParseSprocNamesFromCommandTextPattern(codeStr));

            return results;
        }

        private IEnumerable<string> ParseSprocNamesFromCommandTextPattern(string codeStr)
        {
            var results = new List<string>();

            var matches = Regex.Matches(codeStr, ".CommandText = \"(?<name>.*?)\"");
            foreach (Match match in matches)
            {
                results.Add(match.Groups["name"].Value);
            }

            return results;
        }

        private List<string> ParseSprocNamesFromSqlCommandPattern(string codeStr)
        {
            var results = new List<string>();

            var matches = Regex.Matches(codeStr, "SqlCommand\\(\"(?<name>.*?)\"");
            foreach (Match match in matches)
            {
                results.Add(match.Groups["name"].Value);
            }

            return results;
        }
    }
}
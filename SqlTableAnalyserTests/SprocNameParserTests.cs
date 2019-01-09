using System.Linq;
using FluentAssertions;
using SqlDependancyAnalyser;
using Xunit;

namespace SqlDependancyAnalyserTests
{
    public class SprocNameParserTests
    {
        [Fact]
        public void ParsingAnEmptyFileShouldReturnEmptyList()
        {
            var codeStr = "";
            var daReader = new SprocNameParser();
            var results = daReader.ParseSprocsNamesFrom(codeStr);
            results.Count.Should().Be(0);
        }
        [Fact]
        public void ParsingShouldExtractSprocNameFromSingleSqlCommand()
        {
            var codeStr = "SqlCommand myCommand = new SqlCommand(\"SPROC_NAME\", myConnection);";
            var daReader = new SprocNameParser();
            var results = daReader.ParseSprocsNamesFrom(codeStr);
            results[0].Should().Be("SPROC_NAME");
        }
        [Fact]
        public void ParsingShouldExtractSprocNameFromMultipleSqlCommands()
        {
            var codeStr = 
                "SqlCommand myCommand = new SqlCommand(\"SPROC_NAME_1\", myConnection);" +
                "SqlCommand myCommand = new SqlCommand(\"SPROC_NAME_2\", myConnection);";
            var daReader = new SprocNameParser();
            var results = daReader.ParseSprocsNamesFrom(codeStr);
            results[0].Should().Be("SPROC_NAME_1");
            results[1].Should().Be("SPROC_NAME_2");
        }
        [Fact]
        public void ParsingShouldExtractSprocNameFromSinglCommandText()
        {
            var codeStr = "myCommand.CommandText = \"SPROC_NAME\";";
            var daReader = new SprocNameParser();
            var results = daReader.ParseSprocsNamesFrom(codeStr);
            results[0].Should().Be("SPROC_NAME");
        }
    }
}

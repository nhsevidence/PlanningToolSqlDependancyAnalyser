using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using SqlTableAnalyser;
using Xunit;

namespace SqlTableAnalyserTests
{
    public class DiffCheckerTests
    {
        [Fact]
        public void ShouldFindASingleMatch()
        {
            var diffChecker = new DiffChecker();
            var dbDiffContent = "some content here with Obj1Name in";
            var objNames = new SortedSet<string>()
            {
                "Obj1Name"
            };

            var matches = diffChecker.FindObjsWithAssociatedDbDiffs(objNames, dbDiffContent);

            matches.Count.Should().Be(1);
            matches.First().Should().Be("Obj1Name");
        }

        [Fact]
        public void ShouldntFindASingleMatchPartial()
        {
            var diffChecker = new DiffChecker();
            var dbDiffContent = "some content here with Obj1NameAbc in";
            var objNames = new SortedSet<string>()
            {
                "Obj1Name"
            };

            var matches = diffChecker.FindObjsWithAssociatedDbDiffs(objNames, dbDiffContent);

            matches.Count.Should().Be(0);
        }

        [Fact]
        public void ShouldFindAMatchGlobally()
        {
            var diffChecker = new DiffChecker();
            var dbDiffContent = "some content here\r\n in some more content here with Obj1Name";
            var objNames = new SortedSet<string>()
            {
                "Obj1Name"
            };

            var matches = diffChecker.FindObjsWithAssociatedDbDiffs(objNames, dbDiffContent);

            matches.Count.Should().Be(1);
        }
    }
}

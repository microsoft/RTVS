using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Core.Formatting;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Formatting {
    [ExcludeFromCodeCoverage]
    public class FormatScopeTest {
        [Test]
        [Category.R.Formatting]
        public void Formatter_EmptyFileTest() {
            RFormatter f = new RFormatter();
            string s = f.Format(string.Empty);
            s.Should().BeEmpty();
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatRandom01() {
            RFormatter f = new RFormatter();
            string original = "a   b 1.  2 Inf\tNULL";

            string actual = f.Format(original);

            actual.Should().Be(@"a b 1. 2 Inf NULL");
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_StatementTest01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("x<-2");
            string expected =
@"x <- 2";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatSimpleScopesTest01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("{{}}");
            string expected =
@"{
  { }
}";
            actual.Should().Be(expected);
        }
    }
}

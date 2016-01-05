using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenizeIntegersTest : TokenizeTestBase<RToken, RTokenType> {
        private readonly CoreTestFilesFixture _files;

        public TokenizeIntegersTest(CoreTestFilesFixture files) {
            _files = files;
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeIntegers1() {
            var tokens = Tokenize("+1 ", new RTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(RTokenType.Number)
                .And.StartAt(0)
                .And.HaveLength(2);
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeIntegers2() {
            var tokens = this.Tokenize("-12 +1", new RTokenizer());

            tokens.Should().HaveCount(3);

            tokens[0].Should().HaveType(RTokenType.Number)
                .And.StartAt(0)
                .And.HaveLength(3);

            tokens[1].Should().HaveType(RTokenType.Operator)
                .And.StartAt(4)
                .And.HaveLength(1);

            tokens[2].Should().HaveType(RTokenType.Number)
                .And.StartAt(5)
                .And.HaveLength(1);
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeIntegers3() {
            var tokens = Tokenize("-12+-1", new RTokenizer());

            tokens.Should().HaveCount(3);

            tokens[0].Should().HaveType(RTokenType.Number)
                .And.StartAt(0)
                .And.HaveLength(3);

            tokens[1].Should().HaveType(RTokenType.Operator)
                .And.StartAt(3)
                .And.HaveLength(1);

            tokens[2].Should().HaveType(RTokenType.Number)
                .And.StartAt(4)
                .And.HaveLength(2);
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeFile_IntegerFile() {
            TokenizeFiles.TokenizeFile(_files, @"Tokenization\Integers.r");
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeFile_HexFile() {
            TokenizeFiles.TokenizeFile(_files, @"Tokenization\Hex.r");
        }
    }
}

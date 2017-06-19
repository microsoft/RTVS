// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Disposables;
using Microsoft.Markdown.Editor.Preview.Code;
using Microsoft.Markdown.Editor.Preview.Parser;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text;
using NSubstitute;
using Xunit;

namespace Microsoft.Markdown.Editor.Test.Preview {
    [ExcludeFromCodeCoverage]
    [Category.Md.Preview]
    public class RCodeBlockTest {
        [Test]
        public void BasicCtor() {
            var block = new RCodeBlock(0, null, string.Empty, 0);
            block.State.Should().Be(CodeBlockState.Created);
            block.Eval.Should().BeTrue();
            block.DisplayErrors.Should().BeTrue();
            block.DisplayWarnings.Should().BeTrue();
            block.EchoContent.Should().BeTrue();
        }

        [CompositeTest]
        [InlineData(null, true, true, true, true)]
        [InlineData("", true, true, true, true)]
        [InlineData("echo=FALSE", false, true, true, true)]
        [InlineData("error=FALSE", true, true, false, true)]
        [InlineData("error=FALSE, echo=TRUE", true, true, false, true)]
        [InlineData("eval=FALSE", true, false, true, true)]
        [InlineData("error=T, warning=F}", true, true, true, false)]
        public void Options(string arguments, bool echo, bool eval, bool error, bool warning) {
            var block = new RCodeBlock(0, arguments, string.Empty, 0);
            block.EchoContent.Should().Be(echo);
            block.Eval.Should().Be(eval);
            block.DisplayErrors.Should().Be(error);
            block.DisplayWarnings.Should().Be(warning);
        }

        [Test]
        public async Task SimpleEval() {
            var block = new RCodeBlock(0, null, string.Empty, 0);
            var session = MakeSessionSubstitute();
            await block.EvaluateAsync(session, new RSessionCallback(), CancellationToken.None);
            block.State.Should().Be(CodeBlockState.Evaluated);
        }

        private static IRSession MakeSessionSubstitute() {
            var session = Substitute.For<IRSession>();
            var inter = Substitute.For<IRSessionInteraction>();
            session.BeginInteractionAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(inter);
            return session;
        }
    }
}

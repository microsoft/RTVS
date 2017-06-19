// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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
            var session = Substitute.For<IRSession>();
            var inter = Substitute.For<IRSessionInteraction>();
            session.BeginInteractionAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(inter);

            var block = new RCodeBlock(0, null, string.Empty, 0);
            await block.EvaluateAsync(session, new RSessionCallback(), CancellationToken.None);
            block.State.Should().Be(CodeBlockState.Evaluated);
        }

        [Test]
        public async Task Cancellation() {
            var session = Substitute.For<IRSession>();
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var block = new RCodeBlock(0, null, string.Empty, 0);
            await block.EvaluateAsync(session, new RSessionCallback(), cts.Token);
            block.Result.Should().Contain("canceled");
        }

        [Test]
        public async Task Output() {
            var session = Substitute.For<IRSession>();
            var inter = Substitute.For<IRSessionInteraction>();

            session.BeginInteractionAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(inter));
            inter.When(s => s.RespondAsync(Arg.Any<string>())).Do(c => {
                session.Output += Raise.EventWith(new ROutputEventArgs(OutputType.Output, "output"));
            });

            var block = new RCodeBlock(0, null, string.Empty, 0);
            await block.EvaluateAsync(session, new RSessionCallback(), CancellationToken.None);
            block.Result.Should().Contain("<code");
            block.Result.Should().Contain("output");
            block.Result.Should().NotContain("color: red");
        }

        [Test]
        public async Task Error() {
            var session = Substitute.For<IRSession>();
            var inter = Substitute.For<IRSessionInteraction>();

            session.BeginInteractionAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(inter));
            inter.When(s => s.RespondAsync(Arg.Any<string>())).Do(c => {
                session.Output += Raise.EventWith(new ROutputEventArgs(OutputType.Error, "error"));
            });

            var block = new RCodeBlock(0, null, string.Empty, 0);
            await block.EvaluateAsync(session, new RSessionCallback(), CancellationToken.None);
            block.Result.Should().Contain("<code");
            block.Result.Should().Contain("error");
            block.Result.Should().Contain("color: red");
        }

        [Test]
        public async Task Exception() {
            var session = Substitute.For<IRSession>();
            var inter = Substitute.For<IRSessionInteraction>();

            session.BeginInteractionAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(inter));
            inter.When(s => s.RespondAsync(Arg.Any<string>())).Do(c => throw new RException("disconnected"));

            var block = new RCodeBlock(0, null, string.Empty, 0);
            await block.EvaluateAsync(session, new RSessionCallback(), CancellationToken.None);
            block.Result.Should().Contain("<code");
            block.Result.Should().Contain("disconnected");
            block.Result.Should().Contain("color: red");
        }

        [Test]
        public async Task Plot() {
            var session = Substitute.For<IRSession>();
            var inter = Substitute.For<IRSessionInteraction>();
            var cb = new RSessionCallback { PlotResult = new byte[] { 1, 2, 3, 4 } };
            session.BeginInteractionAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(inter));

            var block = new RCodeBlock(0, null, string.Empty, 0);
            await block.EvaluateAsync(session, cb, CancellationToken.None);
            block.Result.Should().Be("<img src='data:image/gif;base64, AQIDBA==' />");
            cb.PlotResult.Should().BeNull();
        }
    }
}

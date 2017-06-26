// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Markdown.Editor.Preview.Code;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;
using Xunit;

namespace Microsoft.Markdown.Editor.Test.Preview {
    [ExcludeFromCodeCoverage]
    [Category.Md.Preview]
    public sealed class RCodeBlockTest {
        private const string _documentName = "test.rmd";
        private readonly TestCoreShell _shell;
        private readonly IRSession _session;
        private readonly IRSessionInteraction _interaction;

        public RCodeBlockTest() {
            _shell = TestCoreShell.CreateSubstitute();
            _session = _shell.SetupSessionSubstitute();

            _interaction = Substitute.For<IRSessionInteraction>();
            _session.BeginInteractionAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(_interaction));
        }

        [Test]
        public void BasicCtor() {
            var block = new RCodeBlock(0, string.Empty);
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
            var block = new RCodeBlock(0, string.Empty, arguments);
            block.EchoContent.Should().Be(echo);
            block.Eval.Should().Be(eval);
            block.DisplayErrors.Should().Be(error);
            block.DisplayWarnings.Should().Be(warning);
        }

        [Test]
        public async Task SimpleEval() {
            var block = new RCodeBlock(0, string.Empty);
            var evaluator = new RCodeEvaluator(_documentName, _shell.Services);
            await evaluator.EvaluateBlockAsync(block, CancellationToken.None);
            block.State.Should().Be(CodeBlockState.Evaluated);
        }

        [Test]
        public async Task Cancellation() {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var block = new RCodeBlock(0, string.Empty);
            var evaluator = new RCodeEvaluator(_documentName, _shell.Services);

            await evaluator.EvaluateBlockAsync(block, cts.Token);
            block.Result.Should().Contain("canceled");
        }

        [Test]
        public async Task Output() {
            _interaction.When(s => s.RespondAsync(Arg.Any<string>())).Do(c => {
                _session.Output += Raise.EventWith(new ROutputEventArgs(OutputType.Output, "output"));
            });

            var block = new RCodeBlock(0, string.Empty);
            var evaluator = new RCodeEvaluator(_documentName, _shell.Services);
            await evaluator.EvaluateBlockAsync(block, CancellationToken.None);

            block.Result.Should().Contain("<code");
            block.Result.Should().Contain("output");
            block.Result.Should().NotContain("color: red");
        }

        [Test]
        public async Task Error() {
            _interaction.When(s => s.RespondAsync(Arg.Any<string>())).Do(c => {
                _session.Output += Raise.EventWith(new ROutputEventArgs(OutputType.Error, "error"));
            });

            var block = new RCodeBlock(0, string.Empty);
            var evaluator = new RCodeEvaluator(_documentName, _shell.Services);
            await evaluator.EvaluateBlockAsync(block, CancellationToken.None);

            block.Result.Should().Contain("<code");
            block.Result.Should().Contain("error");
            block.Result.Should().Contain("color: red");
        }

        [Test]
        public async Task Exception() {
            _interaction.When(s => s.RespondAsync(Arg.Any<string>())).Do(c => throw new RException("disconnected"));

            var block = new RCodeBlock(0, string.Empty);
            var evaluator = new RCodeEvaluator(_documentName, _shell.Services);
            await evaluator.EvaluateBlockAsync(block, CancellationToken.None);

            block.Result.Should().Contain("<code");
            block.Result.Should().Contain("disconnected");
            block.Result.Should().Contain("color: red");
        }

        [Test]
        public async Task Plot() {
            var cb = new RSessionCallback { PlotResult = new byte[] { 1, 2, 3, 4 } };

            var block = new RCodeBlock(0, string.Empty);
            var evaluator = new RCodeEvaluator(_documentName, _shell.Services);
            evaluator.EvalSession.SessionCallback = cb;
            await evaluator.EvaluateBlockAsync(block, CancellationToken.None);

            block.Result.Should().Be("<img src='data:image/gif;base64, AQIDBA==' />");
            cb.PlotResult.Should().BeNull();
        }
    }
}

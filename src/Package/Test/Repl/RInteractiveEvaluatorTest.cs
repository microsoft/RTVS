// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.R.Components.Test.StubFactories;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Microsoft.VisualStudio.Shell.Mocks;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public sealed class RInteractiveEvaluatorTest {
        private readonly IRSessionProvider _sessionProvider;

        public RInteractiveEvaluatorTest() {
            _sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
        }

        [Test]
        [Category.Repl]
        public async Task EvaluatorTest01() {
            using (new VsRHostScript()) {
                var session = _sessionProvider.GetInteractiveWindowRSession();
                using (var eval = new RInteractiveEvaluator(session, RHistoryStubFactory.CreateDefault(), VsAppShell.Current, RToolsSettings.Current)) {
                    var tb = new TextBufferMock(string.Empty, RContentTypeDefinition.ContentType);
                    var tv = new WpfTextViewMock(tb);

                    var iwm = new InteractiveWindowMock(tv);
                    eval.CurrentWindow = iwm;

                    var result = await eval.InitializeAsync();
                    result.Should().Be(ExecutionResult.Success);
                    session.IsHostRunning.Should().BeTrue();

                    eval.CanExecuteCode("x <-").Should().BeFalse();
                    eval.CanExecuteCode("(()").Should().BeFalse();
                    eval.CanExecuteCode("a *(b+c)").Should().BeTrue();

                    VsRHostScript.DoIdle(300);

                    result = await eval.ExecuteCodeAsync(new string(new char[10000])+"\r\n");
                    result.Should().Be(ExecutionResult.Failure);
                    string text = tb.CurrentSnapshot.GetText();
                    text.Should().Contain(string.Format(Microsoft.R.Components.Resources.InputIsTooLong, 4096));
                    tb.Clear();

                    result = await eval.ExecuteCodeAsync("z <- '電話帳 全米のお'\n");
                    result.Should().Be(ExecutionResult.Success);
                    tb.Clear();

                    result = await eval.ExecuteCodeAsync("z" + Environment.NewLine);
                    result.Should().Be(ExecutionResult.Success);
                    text = tb.CurrentSnapshot.GetText();
                    text.TrimEnd().Should().Be("[1] \"電話帳 全米のお\"");
                    tb.Clear();

                    result = await eval.ExecuteCodeAsync("Encoding(z)\n");
                    result.Should().Be(ExecutionResult.Success);
                    text = tb.CurrentSnapshot.GetText();
                    text.TrimEnd().Should().Be("[1] \"UTF-8\"");
                    tb.Clear();

                    result = await eval.ExecuteCodeAsync("x <- c(1:10)\n");
                    result.Should().Be(ExecutionResult.Success);
                    text = tb.CurrentSnapshot.GetText();
                    text.Should().Be(string.Empty);
                    tb.Clear();

                    await eval.ResetAsync(initialize: false);
                    text = tb.CurrentSnapshot.GetText();
                    text.Should().StartWith(Microsoft.R.Components.Resources.MicrosoftRHostStopping);
                }
            }
        }

        [Test]
        [Category.Repl]
        public async Task EvaluatorTest02() {
            using (new VsRHostScript()) {
                var session = _sessionProvider.GetInteractiveWindowRSession();
                using (var eval = new RInteractiveEvaluator(session, RHistoryStubFactory.CreateDefault(), VsAppShell.Current, RToolsSettings.Current)) {
                    var tb = new TextBufferMock(string.Empty, RContentTypeDefinition.ContentType);
                    var tv = new WpfTextViewMock(tb);

                    var iwm = new InteractiveWindowMock(tv);
                    eval.CurrentWindow = iwm;

                    var result = await eval.InitializeAsync();
                    result.Should().Be(ExecutionResult.Success);
                    session.IsHostRunning.Should().BeTrue();

                    VsRHostScript.DoIdle(1000);
                    tb.Clear();

                    result = await eval.ExecuteCodeAsync("w <- dQuote('text')" + Environment.NewLine);
                    result.Should().Be(ExecutionResult.Success);
                    var text = tb.CurrentSnapshot.GetText();
                    text.Should().Be(string.Empty);
                    tb.Clear();

                    result = await eval.ExecuteCodeAsync("w" + Environment.NewLine);
                    result.Should().Be(ExecutionResult.Success);
                    text = tb.CurrentSnapshot.GetText();
                    text.TrimEnd().Should().Be("[1] \"“text”\"");
                    tb.Clear();

                    result = await eval.ExecuteCodeAsync("e <- dQuote('абвг')" + Environment.NewLine);
                    result.Should().Be(ExecutionResult.Success);
                    text = tb.CurrentSnapshot.GetText();
                    text.Should().Be(string.Empty);
                    tb.Clear();

                    result = await eval.ExecuteCodeAsync("e" + Environment.NewLine);
                    result.Should().Be(ExecutionResult.Success);
                    text = tb.CurrentSnapshot.GetText();
                    text.TrimEnd().Should().Be("[1] \"“абвг”\"");
                    tb.Clear();
                }
            }
        }
    }
}

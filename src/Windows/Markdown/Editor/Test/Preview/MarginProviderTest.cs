// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Preview.Margin;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text.Editor;
using NSubstitute;

namespace Microsoft.Markdown.Editor.Test.Preview {
    [ExcludeFromCodeCoverage]
    [Category.Md.Preview]
    public class MarginProviderTest {
        [Test(ThreadType.UI)]
        public void RightMarginProviderConstructorTest() {
            var shell = SetupServices(out IWpfTextViewHost host, out IWpfTextViewMargin container, out IRMarkdownEditorSettings settings);
            var provider = new PreviewRightMarginProvider(shell);
            var margin = provider.CreateMargin(host, container);
            margin.Should().BeNull();

            settings.EnablePreview.Returns(true);
            margin = provider.CreateMargin(host, container);
            margin.Should().NotBeNull();
        }

        [Test(ThreadType.UI)]
        public void BottonMarginProviderConstructorTest() {
            var shell = SetupServices(out IWpfTextViewHost host, out IWpfTextViewMargin container, out IRMarkdownEditorSettings settings);
            var provider = new PreviewBottomMarginProvider(shell);
            var margin = provider.CreateMargin(host, container);
            margin.Should().BeNull();

            settings.EnablePreview.Returns(true);
            margin = provider.CreateMargin(host, container);
            margin.Should().NotBeNull();
        }

        private static ICoreShell SetupServices(out IWpfTextViewHost host, out IWpfTextViewMargin container, out IRMarkdownEditorSettings settings) {
            var shell = TestCoreShell.CreateSubstitute();
            settings = Substitute.For<IRMarkdownEditorSettings>();
            shell.ServiceManager.AddService(settings);
            var wfp = Substitute.For<IRInteractiveWorkflowProvider>();
            shell.ServiceManager.AddService(wfp);

            var workflow = Substitute.For<IRInteractiveWorkflow>();
            wfp.GetOrCreate().Returns(workflow);

            var session = Substitute.For<IRSession>();
            session.IsHostRunning.Returns(true);
            workflow.RSessions.GetOrCreate(Arg.Any<string>()).Returns(session);

            host = Substitute.For<IWpfTextViewHost>();
            var tb = new TextBufferMock("", MdContentTypeDefinition.ContentType);
            var tv = new WpfTextViewMock(tb);
            host.TextView.Returns(tv);

            container = Substitute.For<IWpfTextViewMargin>();
            return shell;
        }
    }
}

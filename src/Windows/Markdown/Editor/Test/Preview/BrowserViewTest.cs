// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Markdown.Editor.Preview.Browser;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text;
using mshtml;
using NSubstitute;

namespace Microsoft.Markdown.Editor.Test.Preview {
    [ExcludeFromCodeCoverage]
    [Category.Md.Preview]
    public sealed class BrowserViewTest {
        private readonly BrowserView _browser;

        public BrowserViewTest() {
            var shell = TestCoreShell.CreateSubstitute();
            shell.SetupSettingsSubstitute();
            shell.SetupSessionSubstitute();

            shell.ServiceManager.RemoveService(shell.ServiceManager.GetService<IFileSystem>());
            shell.ServiceManager.AddService(new FileSystem());

            _browser = new BrowserView("file", shell.Services);
        }

        [Test(ThreadType = ThreadType.UI)]
        public void Update() {
            const string interactive = "interactive";

            var snapshot = Substitute.For<ITextSnapshot>();
            snapshot.GetText().Returns("Text **bold** text");

            _browser.UpdateBrowser(snapshot);
            var htmlDocument = (HTMLDocument)_browser.Control.Document;

            var ready = htmlDocument.readyState == interactive;
            for (var i = 0; i < 20 && !ready; i++) {
                ready = htmlDocument.readyState == interactive;
                UIThreadHelper.Instance.DoEvents(100);
            }

            ready.Should().BeTrue();

            var element = htmlDocument.getElementById("___markdown-content___");
            var innerHtml = element.innerHTML.Trim();
            innerHtml.Should().Be("<p id=\"pragma-line-0\">Text <strong>bold</strong> text</p>");
        }
    }
}

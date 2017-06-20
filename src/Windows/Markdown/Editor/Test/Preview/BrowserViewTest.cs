// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using mshtml;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Markdown.Editor.Preview.Browser;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text;
using NSubstitute;

namespace Microsoft.Markdown.Editor.Test.Preview {
    [ExcludeFromCodeCoverage]
    [Category.Md.Preview]
    public class BrowserViewTest {
        private TestCoreShell _shell;
        private readonly BrowserView _browser;

        public BrowserViewTest() {
            _shell = TestCoreShell.CreateSubstitute();
            _shell.SetupSettingsSubstitute();
            _shell.SetupSessionSubstitute();

            _shell.ServiceManager.RemoveService<IFileSystem>();
            _shell.ServiceManager.AddService(new FileSystem());

            _browser = new BrowserView("file", _shell.Services);
        }

        [Test(ThreadType = ThreadType.UI)]
        public void Update() {
            var snapshot = Substitute.For<ITextSnapshot>();
            snapshot.GetText().Returns("Text **bold** text");
            _browser.UpdateBrowser(snapshot);

            UIThreadHelper.Instance.DoEvents();

            var htmlDocument = (HTMLDocument)_browser.Control.Document;
            var element = htmlDocument.getElementById("___markdown-content___");
            var innerHtml = element.innerHTML.Trim();
            innerHtml.Should().Be("<p>Text <strong>bold</strong> text</p>");
         }
    }
}

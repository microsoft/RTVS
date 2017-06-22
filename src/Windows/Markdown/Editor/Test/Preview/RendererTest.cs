// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Markdown.Editor.Preview.Browser;
using Microsoft.Markdown.Editor.Preview.Parser;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Markdown.Editor.Test.Preview {
    [ExcludeFromCodeCoverage]
    [Category.Md.Preview]
    public sealed class RendererTest {
        private const string _folder = "Preview";
        private readonly MarkdownTestFilesFixture _files;
        private readonly TestCoreShell _shell;
        private readonly bool _regenerateBaselineFiles = false;

        public RendererTest(MarkdownTestFilesFixture files) {
            _files = files;
            _shell = TestCoreShell.CreateSubstitute();
            _shell.SetupSettingsSubstitute();
            _shell.SetupSessionSubstitute();
        }

        [CompositeTest]
        [InlineData("01.rmd")]
        [InlineData("02.rmd")]
        public void StaticRender(string fileName) {
            var source = _files.LoadDestinationFile(Path.Combine(_files.DestinationPath, _folder, fileName));
            var renderer = new DocumentRenderer(nameof(RendererTest), _shell.Services);
            var document = MarkdownFactory.ParseToMarkdown(source);
            var actual = renderer.RenderStaticHtml(document);
            CompareToBaseline(fileName, actual);
        }

        private void CompareToBaseline(string baselineFileName, string actual) {
            var baselineFilePath = Path.Combine(_files.SourcePath, _folder, baselineFileName) + ".html";
            if (_regenerateBaselineFiles) {
                // Update this to your actual enlistment if you need to update baseline
                TestFiles.UpdateBaseline(baselineFilePath, actual);
            } else {
                TestFiles.CompareToBaseLine(baselineFilePath, actual);
            }
        }
    }
}

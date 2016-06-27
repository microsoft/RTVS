// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.QuickInfo;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Xunit;

namespace Microsoft.R.Editor.Test.QuickInfo {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    public class FunctionIndexTest : IAsyncLifetime {
        private readonly IExportProvider _exportProvider;
        private readonly IEditorShell _editorShell;
        private readonly FunctionIndex _functionIndex;

        public FunctionIndexTest(REditorMefCatalogFixture catalog) {
            _exportProvider = catalog.CreateExportProvider();
            _editorShell = _exportProvider.GetExportedValue<IEditorShell>();
            _functionIndex = new FunctionIndex(_editorShell);
        }

        public Task InitializeAsync() {
            return FunctionIndexUtility.InitializeAsync(_functionIndex);
        }

        public async Task DisposeAsync() {
            await FunctionIndexUtility.DisposeAsync(_functionIndex, _exportProvider);
            _exportProvider.Dispose();
        }

        [Test]
        public async Task QuickInfoSourceTest01() {
            string content = @"x <- as.matrix(x)";
            AstRoot ast = RParser.Parse(content);

            int caretPosition = 15; // in arguments
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            QuickInfoSource quickInfoSource = new QuickInfoSource(textBuffer, _editorShell);
            QuickInfoSessionMock quickInfoSession = new QuickInfoSessionMock(textBuffer, caretPosition);
            List<object> quickInfoContent = new List<object>();

            quickInfoSession.TriggerPoint = new SnapshotPoint(textBuffer.CurrentSnapshot, caretPosition);
            var applicableSpan = await quickInfoSource.AugmentQuickInfoSessionAsync(ast, caretPosition, quickInfoSession, quickInfoContent);

            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 10);

            applicableSpan.Should().NotBeNull();
            quickInfoContent.Should().ContainSingle()
                .Which.ToString().Should().StartWith("as.matrix(x, data, nrow, ncol, byrow, dimnames, rownames.force, ...)");
        }

        [Test]
        public async Task QuickInfoSourceTest02() {
            // 'as.Date.character' RD contains no function info for 'as.Date.character', but the one for 'as.Date'
            // then, the current code expects to add 'as.Date' quick info, which is the first function info for as.Date.character
            string content = @"x <- as.Date.character(x)";
            AstRoot ast = RParser.Parse(content);

            int caretPosition = 23; // in arguments
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            QuickInfoSource quickInfoSource = new QuickInfoSource(textBuffer, _editorShell);
            QuickInfoSessionMock quickInfoSession = new QuickInfoSessionMock(textBuffer, caretPosition);
            List<object> quickInfoContent = new List<object>();

            quickInfoSession.TriggerPoint = new SnapshotPoint(textBuffer.CurrentSnapshot, caretPosition);
            var applicableSpan = await quickInfoSource.AugmentQuickInfoSessionAsync(ast, caretPosition, quickInfoSession, quickInfoContent);

            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 10);

            applicableSpan.Should().NotBeNull();
            quickInfoContent.Should().ContainSingle()
                .Which.ToString().Should().StartWith("as.Date(x, ...)");
        }
    }
}

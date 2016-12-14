// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.QuickInfo;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.R.Support.Help.Packages;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Xunit;

namespace Microsoft.R.Editor.Test.QuickInfo {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    [Collection(CollectionNames.NonParallel)]
    public class FunctionIndexTest : FunctionIndexBasedTest {
        public FunctionIndexTest(REditorMefCatalogFixture catalog) : base(catalog) { }


        [CompositeTest]
        [InlineData(true)]
        [InlineData(false)]
        public async Task QuickInfoSourceTest01(bool cached) {
            if(!cached) {
                Support.Help.Packages.PackageIndex.ClearCache();
            }
            string content = @"x <- as.matrix(x)";
            AstRoot ast = RParser.Parse(content);

            int caretPosition = 15; // in arguments
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            QuickInfoSource quickInfoSource = new QuickInfoSource(textBuffer, EditorShell);
            QuickInfoSessionMock quickInfoSession = new QuickInfoSessionMock(textBuffer, caretPosition);
            List<object> quickInfoContent = new List<object>();

            quickInfoSession.TriggerPoint = new SnapshotPoint(textBuffer.CurrentSnapshot, caretPosition);
            var applicableSpan = await quickInfoSource.AugmentQuickInfoSessionAsync(ast, caretPosition, quickInfoSession, quickInfoContent);

            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 10);

            applicableSpan.Should().NotBeNull();
            quickInfoContent.Should().ContainSingle()
                .Which.ToString().Should().StartWith("as.matrix(x, ..., data, nrow, ncol, byrow, dimnames, rownames.force)");
        }

        [Test]
        public async Task QuickInfoSourceTest02() {
            // 'as.Date.character' RD contains no function info for 'as.Date.character', but the one for 'as.Date'
            // and as.Date.character appears as alias. We verify as.Date.character is shown in the signature info.
            string content = @"x <- as.Date.character(x)";
            AstRoot ast = RParser.Parse(content);

            int caretPosition = 23; // in arguments
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            QuickInfoSource quickInfoSource = new QuickInfoSource(textBuffer, EditorShell);
            QuickInfoSessionMock quickInfoSession = new QuickInfoSessionMock(textBuffer, caretPosition);
            List<object> quickInfoContent = new List<object>();

            quickInfoSession.TriggerPoint = new SnapshotPoint(textBuffer.CurrentSnapshot, caretPosition);
            var applicableSpan = await quickInfoSource.AugmentQuickInfoSessionAsync(ast, caretPosition, quickInfoSession, quickInfoContent);

            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 10);

            applicableSpan.Should().NotBeNull();
            quickInfoContent.Should().ContainSingle()
                .Which.ToString().Should().StartWith("as.Date.character(x, ...)");
        }
    }
}

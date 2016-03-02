// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.QuickInfo;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Xunit;

namespace Microsoft.R.Editor.Test.QuickInfo {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    [Collection(CollectionNames.NonParallel)]
    public class FunctionIndexTest : IAsyncLifetime {
        public Task InitializeAsync() {
            return FunctionIndexUtility.InitializeAsync();
        }

        public Task DisposeAsync() {
            return FunctionIndexUtility.DisposeAsync();
        }

        [Test]
        public async Task QuickInfoSourceTest01() {
            string content = @"x <- as.matrix(x)";
            AstRoot ast = RParser.Parse(content);

            int caretPosition = 6;
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            QuickInfoSource quickInfoSource = new QuickInfoSource(textBuffer);
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

            int caretPosition = 6;
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            QuickInfoSource quickInfoSource = new QuickInfoSource(textBuffer);
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

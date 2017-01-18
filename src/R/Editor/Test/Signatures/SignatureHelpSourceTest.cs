// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    public class SignatureHelpSourceTest : FunctionIndexBasedTest {
        public SignatureHelpSourceTest(IExportProvider exportProvider) : base(exportProvider) { }

        [Test]
        public async Task SignatureHelpSourceTest01() {
            string content = @"x <- as.matrix(x)";

            AstRoot ast = RParser.Parse(content);
            int caretPosition = 15;
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            SignatureHelpSource signatureHelpSource = new SignatureHelpSource(textBuffer, EditorShell);
            SignatureHelpSessionMock signatureHelpSession = new SignatureHelpSessionMock(textBuffer, caretPosition);
            List<ISignature> signatures = new List<ISignature>();

            signatureHelpSession.TrackingPoint = new TrackingPointMock(textBuffer, caretPosition, PointTrackingMode.Positive, TrackingFidelityMode.Forward);
            await signatureHelpSource.AugmentSignatureHelpSessionAsync(signatureHelpSession, signatures, ast);

            signatures.Should().ContainSingle();
            signatures[0].Parameters.Should().HaveCount(8);
            signatures[0].CurrentParameter.Name.Should().Be("x");
            signatures[0].Content.Should().Be("as.matrix(x, ..., data, nrow, ncol, byrow, dimnames, rownames.force)");
            signatures[0].Documentation.Should().NotBeEmpty();
        }

        [Test]
        public async Task SignatureHelpSourceTest02() {
            string content = 
@"
x <- function(a, b = TRUE, c = 12/7) { }
x( )
";

            AstRoot ast = RParser.Parse(content);
            int caretPosition = content.IndexOf("( )")+1;
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            SignatureHelpSource signatureHelpSource = new SignatureHelpSource(textBuffer, EditorShell);
            SignatureHelpSessionMock signatureHelpSession = new SignatureHelpSessionMock(textBuffer, caretPosition);
            List<ISignature> signatures = new List<ISignature>();

            signatureHelpSession.TrackingPoint = new TrackingPointMock(textBuffer, caretPosition, PointTrackingMode.Positive, TrackingFidelityMode.Forward);
            await signatureHelpSource.AugmentSignatureHelpSessionAsync(signatureHelpSession, signatures, ast);

            signatures.Should().ContainSingle();
            signatures[0].Parameters.Should().HaveCount(3);
            signatures[0].CurrentParameter.Name.Should().Be("a");
            signatures[0].Content.Should().Be("x(a, b = TRUE, c = 12/7)");
            signatures[0].Documentation.Should().BeEmpty();
        }
    }
}

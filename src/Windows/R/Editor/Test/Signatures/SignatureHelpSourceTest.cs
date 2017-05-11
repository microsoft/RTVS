// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    public class SignatureHelpSourceTest : FunctionIndexBasedTest {
        public SignatureHelpSourceTest(IServiceContainer services) : base(services) { }

        [Test]
        public async Task SignatureHelpSourceTest01() {
            const string content = @"x <- as.matrix(x)";

            var ast = RParser.Parse(content);
            var caretPosition = 15;
            var textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var signatureHelpSource = new RSignatureHelpSource(textBuffer, Services);
            var signatureHelpSession = new SignatureHelpSessionMock(Services, textBuffer, caretPosition);
            var signatures = new List<ISignature>();

            signatureHelpSession.TrackingPoint = new TrackingPointMock(textBuffer, caretPosition, PointTrackingMode.Positive, TrackingFidelityMode.Forward);
            await signatureHelpSource.AugmentSignatureHelpSessionAsync(signatureHelpSession, signatures, ast);

            signatures.Should().ContainSingle();
            signatures[0].Parameters.Should().HaveCount(2);
            signatures[0].CurrentParameter.Name.Should().Be("x");
            signatures[0].Content.Should().Be("as.matrix(x, ...)");
            signatures[0].Documentation.Should().NotBeEmpty();
        }

        [Test]
        public async Task SignatureHelpSourceTest02() {
            const string content =
@"
x <- function(a, b = TRUE, c = 12/7) { }
x( )
";

            var ast = RParser.Parse(content);
            var caretPosition = content.IndexOfOrdinal("( )") + 1;
            var textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var signatureHelpSource = new RSignatureHelpSource(textBuffer, Services);
            var signatureHelpSession = new SignatureHelpSessionMock(Services, textBuffer, caretPosition);
            var signatures = new List<ISignature>();

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

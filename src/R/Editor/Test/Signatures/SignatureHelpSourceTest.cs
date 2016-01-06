using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Xunit;

namespace Microsoft.R.Editor.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    [Collection(CollectionNames.NonParallel)]
    public class SignatureHelpSourceTest : IAsyncLifetime {
        public Task InitializeAsync() {
            return FunctionIndexUtility.InitializeAsync();
        }

        public Task DisposeAsync() {
            return FunctionIndexUtility.DisposeAsync();
        }

        [Test]
        public async Task SignatureHelpSourceTest01() {
            string content = @"x <- as.matrix(x)";

            AstRoot ast = RParser.Parse(content);
            int caretPosition = 15;
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            SignatureHelpSource signatureHelpSource = new SignatureHelpSource(textBuffer);
            SignatureHelpSessionMock signatureHelpSession = new SignatureHelpSessionMock(textBuffer, caretPosition);
            List<ISignature> signatures = new List<ISignature>();

            signatureHelpSession.TrackingPoint = new TrackingPointMock(textBuffer, caretPosition, PointTrackingMode.Positive, TrackingFidelityMode.Forward);
            await signatureHelpSource.AugmentSignatureHelpSessionAsync(signatureHelpSession, signatures, ast);

            signatures.Should().ContainSingle();
            signatures[0].Parameters.Should().HaveCount(2);
            signatures[0].CurrentParameter.Name.Should().Be("x");
            signatures[0].Content.Should().Be("as.matrix(x, ...)");
            signatures[0].Documentation.Should().NotBeEmpty();
        }
    }
}

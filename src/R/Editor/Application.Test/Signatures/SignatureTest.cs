using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.R.Support.Help.Functions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Language.Intellisense;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class SignatureTest {
        [Test]
        [Category.Interactive]
        public void R_SignatureParametersMatch() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                using (new RHostScript(EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>())) {
                    PrepareFunctionIndex();

                    script.Type("x <- lm(");

                    ISignatureHelpSession session = script.GetSignatureSession();
                    session.Should().NotBeNull();
                    IParameter parameter = session.SelectedSignature.CurrentParameter;
                    parameter.Should().NotBeNull();

                    parameter.Name.Should().Be("formula");

                    script.Type("sub");
                    script.DoIdle(500);
                    script.Type("{TAB}");
                    script.DoIdle(1000);

                    parameter = session.SelectedSignature.CurrentParameter;
                    parameter.Name.Should().Be("subset");

                    string actual = script.EditorText;
                    actual.Should().Be("x <- lm(subset = )");

                    session = script.GetSignatureSession();
                    parameter = session.SelectedSignature.CurrentParameter;
                }
            }
        }

        [Test]
        [Category.Interactive]
        public void R_SignatureSessionNavigation() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                using (new RHostScript(EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>())) {
                    PrepareFunctionIndex();

                    script.Type("x <- lm(subset = a, sing");
                    script.DoIdle(500);
                    script.Type("{TAB}");
                    script.DoIdle(1000);

                    ISignatureHelpSession session = script.GetSignatureSession();
                    session.Should().NotBeNull();
                    IParameter parameter = session.SelectedSignature.CurrentParameter;
                    parameter.Should().NotBeNull();

                    parameter.Name.Should().Be("singular.ok");

                    script.MoveLeft(17);
                    parameter = session.SelectedSignature.CurrentParameter;
                    parameter.Name.Should().Be("subset");

                    script.MoveRight(3);
                    parameter = session.SelectedSignature.CurrentParameter;
                    parameter.Name.Should().Be("singular.ok");
                }
            }
        }

        private void PrepareFunctionIndex() {
            FunctionIndex.Initialize();
            FunctionIndex.BuildIndexAsync().Wait();
        }
    }
}

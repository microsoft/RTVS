using System;
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
        [Test(Skip = "Unstable")]
        [Category.Interactive]
        public void R_SignatureParametersMatch() {
            using (new RHostScript()) {
                using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
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

        [Test(Skip = "Unstable")]
        [Category.Interactive]
        public void R_SignatureSessionNavigation() {
            using (new RHostScript()) {
                using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
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

        [ExcludeFromCodeCoverage]
        public sealed class RHostScript : IDisposable {
            public IRSessionProvider SessionProvider { get; private set; }
            public IRSession Session { get; private set; }

            public RHostScript() {
                SessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                Session = SessionProvider.Create(0, new RHostClientTestApp());
                Session.StartHostAsync("RHostScript", IntPtr.Zero).Wait();
            }

            public void Dispose() {
                if (Session != null) {
                    Session.StopHostAsync().Wait();
                    Session.Dispose();
                    Session = null;
                }

                if (SessionProvider != null) {
                    SessionProvider.Dispose();
                    SessionProvider = null;
                }
            }
        }
    }
}

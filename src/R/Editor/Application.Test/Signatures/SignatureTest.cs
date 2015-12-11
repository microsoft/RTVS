using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Support.Help.Functions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Application.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SignatureTest {
        [TestMethod]
        [TestCategory("Interactive")]
        public void R_SignatureParametersMatch() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                PrepareFunctionIndex();

                script.Type("x <- lm(");

                ISignatureHelpSession session = script.GetSignatureSession();
                Assert.IsNotNull(session);
                IParameter parameter = session.SelectedSignature.CurrentParameter;
                Assert.IsNotNull(parameter);

                Assert.AreEqual("formula", parameter.Name);

                script.Type("sub");
                script.DoIdle(100);
                script.Type("{TAB}");
                script.DoIdle(200);

                parameter = session.SelectedSignature.CurrentParameter;
                Assert.AreEqual("subset", parameter.Name);

                string actual = script.EditorText;
                Assert.AreEqual("x <- lm(subset = )", actual);

                session = script.GetSignatureSession();
                parameter = session.SelectedSignature.CurrentParameter;
            }
        }

        [TestMethod]
        [TestCategory("Interactive")]
        public void R_SignatureSessionNavigation() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                PrepareFunctionIndex();

                script.Type("x <- lm(subset = a, sing");
                script.DoIdle(100);
                script.Type("{TAB}");
                script.DoIdle(1000);

                ISignatureHelpSession session = script.GetSignatureSession();
                Assert.IsNotNull(session);
                IParameter parameter = session.SelectedSignature.CurrentParameter;
                Assert.IsNotNull(parameter);

                Assert.AreEqual("singular.ok", parameter.Name);

                script.MoveLeft(17);
                parameter = session.SelectedSignature.CurrentParameter;
                Assert.AreEqual("subset", parameter.Name);

                script.MoveRight(3);
                parameter = session.SelectedSignature.CurrentParameter;
                Assert.AreEqual("singular.ok", parameter.Name);
            }
        }

        private void PrepareFunctionIndex() {
            FunctionIndex.Initialize();
            FunctionIndex.BuildIndexAsync().Wait();
        }
    }
}

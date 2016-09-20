// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Signatures;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Language.Intellisense;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class SignatureTest {
        [Test]
        [Category.Interactive]
        public async Task R_SignatureParametersMatch() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                FunctionRdDataProvider.HostStartTimeout = 10000;
                using (new RHostScript(EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>())) {
                    await PrepareFunctionIndex();
                    FunctionIndexUtility.GetFunctionInfoAsync("lm").Wait(3000);

                    script.Type("x <- lm(");
                    script.DoIdle(2000);

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
        public async Task R_SignatureSessionNavigation() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                FunctionRdDataProvider.HostStartTimeout = 10000;
                using (new RHostScript(EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>())) {
                    await PrepareFunctionIndex();
                    var info = await FunctionIndexUtility.GetFunctionInfoAsync("lm");

                    script.Type("x <- lm(subset = a, sing");
                    script.DoIdle(1000);
                    script.Type("{TAB}");
                    script.DoIdle(1000);

                    ISignatureHelpSession session = script.GetSignatureSession();
                    session.Should().NotBeNull();

                    script.DoIdle(200);
                    IParameter parameter = session.SelectedSignature.CurrentParameter;
                    parameter.Should().NotBeNull();
                    parameter.Name.Should().Be("singular.ok");

                    script.MoveLeft(17);
                    script.DoIdle(200);
                    parameter = session.SelectedSignature.CurrentParameter;
                    parameter.Name.Should().Be("subset");

                    script.MoveRight(3);
                    script.DoIdle(200);
                    parameter = session.SelectedSignature.CurrentParameter;
                    parameter.Name.Should().Be("singular.ok");
                }
            }
        }

        [Test]
        [Category.Interactive]
        public async Task R_EqualsCompletion01() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                await PrepareFunctionIndex();
                var info = await FunctionIndexUtility.GetFunctionInfoAsync("addmargins");

                script.DoIdle(100);
                script.Type("addmargins(Fu");
                script.DoIdle(300);
                script.Type("=");
                script.DoIdle(300);

                string expected = "addmargins(FUN = )";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        private Task PrepareFunctionIndex() {
            FunctionIndex.Initialize();
            return FunctionIndex.BuildIndexAsync();
        }
    }
}

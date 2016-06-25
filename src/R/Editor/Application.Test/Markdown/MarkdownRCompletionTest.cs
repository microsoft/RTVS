// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Signatures;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Language.Intellisense;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Markdown {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class MarkdownRCompletionTest : IDisposable {
        private readonly IExportProvider _exportProvider;
        private readonly EditorHostMethodFixture _editorHost;

        public MarkdownRCompletionTest(REditorApplicationMefCatalogFixture catalogFixture, EditorHostMethodFixture editorHost) {
            _exportProvider = catalogFixture.CreateExportProvider();
            _editorHost = editorHost;
        }

        public void Dispose() {
            _exportProvider.Dispose();
        }

        [Test]
        [Category.Interactive]
        public void TypeRBlock() {
            using (var script = _editorHost.StartScript(_exportProvider, MdContentTypeDefinition.ContentType)) {
                script.Type("```{r}{ENTER}{ENTER}```");
                script.MoveUp();
                script.Type("x");
                script.DoIdle(200);
                script.Type("<-");
                script.DoIdle(200);
                script.Type("funct");
                script.DoIdle(200);
                script.Type("{TAB}(){");
                script.DoIdle(200);
                script.Type("{ENTER}abbr{TAB}(");
 
                string expected = 
@"```{r}
x <- function() {
    abbreviate()
}
```";
                string actual = script.EditorText;
                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void RSignature() {
            using (var script = _editorHost.StartScript(_exportProvider, "```{r}\r\n\r\n```", MdContentTypeDefinition.ContentType)) {
                FunctionRdDataProvider.HostStartTimeout = 10000;
                using (new RHostScript(EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>())) {
                    FunctionIndex.Initialize();
                    FunctionIndex.BuildIndexAsync().Wait();
                    FunctionIndexUtility.GetFunctionInfoAsync("lm").Wait(3000);

                    script.MoveDown();
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
                    actual.Should().Be("```{r}\r\nx <- lm(subset = )\r\n```");

                    session = script.GetSignatureSession();
                    parameter = session.SelectedSignature.CurrentParameter;
                }
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class FormatSelectionTest : IDisposable {
        private readonly IExportProvider _exportProvider;
        private readonly EditorHostMethodFixture _editorHost;

        public FormatSelectionTest(REditorApplicationMefCatalogFixture catalogFixture, EditorHostMethodFixture editorHost) {
            _exportProvider = catalogFixture.CreateExportProvider();
            _editorHost = editorHost;
        }

        public void Dispose() {
            _exportProvider.Dispose();
        }
        
        [Test]
        [Category.Interactive]
        public void R_FormatSelection01() {
            string content = "\nwhile (TRUE) {\n        if(x>1) {\n   }\n}";
            string expected = "\nwhile (TRUE) {\n    if (x > 1) {\n    }\n}";
            using (var script = _editorHost.StartScript(_exportProvider, content, RContentTypeDefinition.ContentType)) {
                script.Select(20, 18);
                script.Execute(VSConstants.VSStd2KCmdID.FORMATSELECTION, 50);
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }
    }
}

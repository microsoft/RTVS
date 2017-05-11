// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class FormatSelectionTest {
        private readonly IServiceContainer _services;
        private readonly EditorHostMethodFixture _editorHost;

        public FormatSelectionTest(IServiceContainer services, EditorHostMethodFixture editorHost) {
            _services = services;
            _editorHost = editorHost;
        }

        [Test]
        [Category.Interactive]
        public async Task R_FormatSelection01() {
            var content = "\nwhile (TRUE) {\n        if(x>1) {\n   }\n}";
            var expected = "\nwhile (TRUE) {\n    if (x > 1) {\n    }\n}";
            using (var script = await _editorHost.StartScript(_services, content, RContentTypeDefinition.ContentType)) {
                script.Select(20, 18);
                script.Execute(VSConstants.VSStd2KCmdID.FORMATSELECTION, 50);
                var actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }
    }
}

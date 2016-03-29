// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Selection {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class DocumentatonTest {
        [Test]
        [Category.Interactive]
        public void InsertRoxygenBlock() {
            string content =
@"
x <- function(a,b,c) { }
";
            string expected =
@"#' Title
#'
#' @param a
#' @param b
#' @param c
#'
#' @return
#' @export
#'
#' @examples
x <- function(a,b,c) { }
";

            using (var script = new TestScript(content, RContentTypeDefinition.ContentType)) {
                script.Type("###");
                var actual = EditorWindow.TextBuffer.CurrentSnapshot.GetText();
                actual.Should().Be(expected);
            }
        }
    }
}

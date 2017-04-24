// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Comments;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.R.Editor.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.Documentation]
    public class RoxygenTest {
        [Test]
        public void InsertRoxygen01() {
            var tb = new TextBufferMock(string.Empty, RContentTypeDefinition.ContentType);
            var eb = tb.ToEditorBuffer();

            var ast = RParser.Parse(tb.CurrentSnapshot.GetText());
            var result = RoxygenBlock.TryInsertBlock(eb, ast, 0);
            result.Should().BeFalse();

            tb = new TextBufferMock("x <- 1\r\ny <- 2", RContentTypeDefinition.ContentType);
            eb = tb.ToEditorBuffer();

            ast = RParser.Parse(tb.CurrentSnapshot.GetText());
            RoxygenBlock.TryInsertBlock(eb, ast, 0).Should().BeFalse();
            RoxygenBlock.TryInsertBlock(eb, ast, 8).Should().BeFalse();

            tb = new TextBufferMock("##\r\nx <- function(a) { }", RContentTypeDefinition.ContentType);
            eb = tb.ToEditorBuffer();

            ast = RParser.Parse(tb.CurrentSnapshot.GetText());
            RoxygenBlock.TryInsertBlock(eb, ast, 4).Should().BeTrue();
            var actual = tb.CurrentSnapshot.GetText();
            actual.Should().Be(
@"#' Title
#'
#' @param a
#'
#' @return
#' @export
#'
#' @examples
x <- function(a) { }");

            var funcStart = tb.CurrentSnapshot.GetText().IndexOfOrdinal("x <-");
            tb.Insert(funcStart, "\r\n");
            RoxygenBlock.TryInsertBlock(eb, ast, funcStart - 2).Should().BeFalse();
        }

        [Test]
        public void InsertRoxygen02() {
            var tb = new TextBufferMock("##\r\nx <- function() { }", RContentTypeDefinition.ContentType);
            var eb = tb.ToEditorBuffer();

            var ast = RParser.Parse(tb.CurrentSnapshot.GetText());
            RoxygenBlock.TryInsertBlock(eb, ast, 4).Should().BeTrue();
            string actual = tb.CurrentSnapshot.GetText();
            actual.Should().Be(
@"#' Title
#'
#' @return
#' @export
#'
#' @examples
x <- function() { }");
        }

        [Test]
        public void InsertRoxygen03() {
            var tb = new TextBufferMock("##\r\nx <-\r\n function(a=1, b, c=FALSE) { }", RContentTypeDefinition.ContentType);
            var eb = tb.ToEditorBuffer();

            var ast = RParser.Parse(tb.CurrentSnapshot.GetText());
            RoxygenBlock.TryInsertBlock(eb, ast, 4).Should().BeTrue();
            string actual = tb.CurrentSnapshot.GetText();
            actual.Should().Be(
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
x <-
 function(a=1, b, c=FALSE) { }");
        }
    }
}

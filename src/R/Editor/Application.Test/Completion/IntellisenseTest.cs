using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Completion {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class IntellisenseTest {
        [Test]
        [Category.Interactive]
        public void R_KeywordIntellisense() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("funct");
                script.DoIdle(100);
                script.Type("{TAB}");

                string expected = "function";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_LibraryIntellisense() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("library(ut");
                script.DoIdle(100);
                script.Type("{TAB}");

                string expected = "library(utils)";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_RequireIntellisense() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("require(uti");
                script.DoIdle(100);
                script.Type("{TAB}");

                string expected = "require(utils)";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_CompletionFilter01() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("x <- lm");
                script.DoIdle(100);
                script.Type("mmm");
                script.DoIdle(100);
                script.Backspace();
                script.Backspace();
                script.Backspace();
                script.Backspace();
                script.DoIdle(100);
                script.Type("abels.{TAB}");

                string expected = "x <- labels.default";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_CompletionFilter02() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("x <- lm");
                script.DoIdle(100);
                script.Type("+");

                string expected = "x <- lm+";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }
    }
}

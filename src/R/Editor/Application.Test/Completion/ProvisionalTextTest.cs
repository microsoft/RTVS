// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Completion {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public sealed class RProvisionalTextTest {
        [Test]
        [Category.Interactive]
        public void R_ProvisionalText01() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("{");
                script.Type("(");
                script.Type("[");
                script.Type("\"");

                string expected = "{([\"\"])}";
                string actual = script.EditorText;

                actual.Should().Be(expected);

                REditorSettings.AutoFormat = false;

                script.Type("\"");
                script.Type("]");
                script.Type(")");
                script.Type("}");
                script.DoIdle(1000);

                expected = "{([\"\"])}";
                actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_ProvisionalText02() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("c(\"");

                string expected = "c(\"\")";
                string actual = script.EditorText;

                actual.Should().Be(expected);

                // Move caret outside of the provisional text area 
                // and back so provisional text becomes permanent.
                script.MoveRight();
                script.MoveLeft();

                // Let parser hit on idle so AST updates
                script.DoIdle(300);

                // There should not be completion inside ""
                script.Type("\"");

                expected = "c(\"\"\")";
                actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_ProvisionalCurlyBrace01() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = false;

                script.Type("while(1)");
                script.DoIdle(300);
                script.Type("{");
                script.DoIdle(300);
                script.Type("{ENTER}}");

                string expected = "while (1) {\r\n}";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }
    }
}

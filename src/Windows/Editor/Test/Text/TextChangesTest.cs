// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor.Extensions;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Languages.Editor.Test.Text {
    [ExcludeFromCodeCoverage]
    public class TextChangesTest {
        [Test]
        [Category.Languages.Core]
        public void TextChanges_DeleteInMiddle() {
            IList<TextChange> changes = BuildChangeList("abc", "adc");

            changes.Should().ContainSingle()
                .Which.Should().Be(new TextChange(1, 1, "d"));
        }

        [Test]
        [Category.Languages.Core]
        public void TextChanges_DontBreakCRNL() {
            IList<TextChange> changes = BuildChangeList(" \n\n ", " \r\n ");

            changes.Should().ContainSingle()
                .Which.Should().Be(new TextChange(1, 2, "\r\n"));
        }

        [Test]
        [Category.Languages.Core]
        public void TextChanges_DeleteOnlyAtStart() {
            IList<TextChange> changes = BuildChangeList("abc", "bc");
            changes.Should().ContainSingle()
                .Which.Should().Be(new TextChange(0, 1, ""));
        }

        private IList<TextChange> BuildChangeList(string oldText, string newText) {
            return TextChanges.BuildChangeList(oldText, newText, int.MaxValue);
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Language.Editor.Test.Settings;
using Microsoft.Languages.Core.Formatting;
using Microsoft.R.Editor.Settings;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.Test.Settings {
    [ExcludeFromCodeCoverage]
    [Category.R.Settings]
    public class SettingsTest {
        [Test]
        public void Settings_TestDefaults() {
            var settings = new REditorSettings(new TestSettingsStorage());

            settings.CommitOnSpace.Should().BeFalse();
            settings.CompletionEnabled.Should().BeTrue();
            settings.FormatOnPaste.Should().BeTrue();
            settings.IndentSize.Should().Be(4);
            settings.IndentStyle.Should().Be(IndentStyle.Smart);
            settings.IndentType.Should().Be(IndentType.Spaces);
            settings.TabSize.Should().Be(4);
            settings.SyntaxCheckEnabled.Should().BeTrue();
            settings.SignatureHelpEnabled.Should().BeTrue();

            settings.FormatOptions.IndentSize.Should().Be(4);
            settings.FormatOptions.TabSize.Should().Be(4);
            settings.FormatOptions.IndentType.Should().Be(IndentType.Spaces);
            settings.FormatOptions.SpaceAfterComma.Should().BeTrue();
            settings.FormatOptions.SpaceAfterKeyword.Should().BeTrue();
            settings.FormatOptions.BracesOnNewLine.Should().BeFalse();
        }
    }
}

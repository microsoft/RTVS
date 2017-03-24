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
            REditorSettings.Initialize(new TestSettingsStorage());

            REditorSettings.CommitOnSpace.Should().BeFalse();
            REditorSettings.CompletionEnabled.Should().BeTrue();
            REditorSettings.FormatOnPaste.Should().BeTrue();
            REditorSettings.IndentSize.Should().Be(4);
            REditorSettings.IndentStyle.Should().Be(IndentStyle.Smart);
            REditorSettings.IndentType.Should().Be(IndentType.Spaces);
            REditorSettings.TabSize.Should().Be(4);
            REditorSettings.SyntaxCheck.Should().BeTrue();
            REditorSettings.SignatureHelpEnabled.Should().BeTrue();
            //REditorSettings.ShowTclFunctions.Should().BeFalse();
            //REditorSettings.ShowInternalFunctions.Should().BeFalse();

            REditorSettings.FormatOptions.IndentSize.Should().Be(4);
            REditorSettings.FormatOptions.TabSize.Should().Be(4);
            REditorSettings.FormatOptions.IndentType.Should().Be(IndentType.Spaces);
            REditorSettings.FormatOptions.SpaceAfterComma.Should().BeTrue();
            REditorSettings.FormatOptions.SpaceAfterKeyword.Should().BeTrue();
            REditorSettings.FormatOptions.BracesOnNewLine.Should().BeFalse();
        }
    }
}

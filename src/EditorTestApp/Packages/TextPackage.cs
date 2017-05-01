// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Settings;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Application.Packages {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IWritableEditorSettingsStorage))]
    [ContentType("text")]
    [Name("Generic Test settings")]
    [Order(Before = "Default")]
    internal sealed class TextSettingsStorage : SettingsStorage { }
}

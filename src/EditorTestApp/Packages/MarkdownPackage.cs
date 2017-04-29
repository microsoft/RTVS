// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Settings;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Application.Packages {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [Name("Markdown Text View Connection Listener")]
    [Order(Before = "Default")]
    internal sealed class TestMdTextViewConnectionListener : MdTextViewConnectionListener {
        [ImportingConstructor]
        public TestMdTextViewConnectionListener(ICoreShell coreShell): base(coreShell.Services) { }
    }

    [ExcludeFromCodeCoverage]
    [Export(typeof(IWritableEditorSettingsStorage))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    [Name("Markdown Test settings")]
    [Order(Before = "Default")]
    internal sealed class MdSettingsStorage : SettingsStorage { }
}

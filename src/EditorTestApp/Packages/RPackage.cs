// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Settings;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Application.Packages {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [Name("R Text View Connection Listener")]
    [Order(Before = "Default")]
    internal sealed class TestRTextViewConnectionListener : RTextViewConnectionListener {
        public TestRTextViewConnectionListener() {
        }

        protected override void OnTextBufferCreated(ITextBuffer textBuffer) {
            InitEditorInstance(textBuffer);
            base.OnTextBufferCreated(textBuffer);
        }

        private void InitEditorInstance(ITextBuffer textBuffer) {
            if (ServiceManager.GetService<IEditorInstance>(textBuffer) == null) {
                ContentTypeImportComposer<IEditorFactory> importComposer = new ContentTypeImportComposer<IEditorFactory>(EditorShell.Current.CompositionService);
                IEditorFactory factory = importComposer.GetImport(textBuffer.ContentType.TypeName);
                IEditorInstance editorInstance = factory.CreateEditorInstance(textBuffer, new RDocumentFactory());
            }
        }
    }

    [ExcludeFromCodeCoverage]
    [Export(typeof(IWritableEditorSettingsStorage))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Test settings")]
    [Order(Before = "Default")]
    internal sealed class RSettingsStorage : SettingsStorage { }
}

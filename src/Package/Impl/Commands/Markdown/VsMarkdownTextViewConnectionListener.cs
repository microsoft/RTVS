// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Document.Markdown;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Workspace;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Commands.Markdown {
    [Export(typeof(IWpfTextViewConnectionListener))]
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [Name("Visual Studio Markdown Editor Text View Connection Listener")]
    [Order(Before = "Default")]
    internal sealed class VsMarkdownTextViewConnectionListener : MdTextViewConnectionListener {
        protected override void OnTextViewGotAggregateFocus(ITextView textView, ITextBuffer textBuffer) {
            // Only attach controllers if the document is editable
            if (textView.Roles.Contains(PredefinedTextViewRoles.Editable)) {
                // Check if another buffer already attached a command controller to the view.
                // Don't allow two to be attached, or commands could be run twice.
                // This currently can only happen with inline diff views.
                MdMainController mainController = MdMainController.FromTextView(textView);
                if (textBuffer == mainController.TextBuffer) {
                    // Connect main controller to VS text view filter chain. The chain looks like
                    // VS IDE -> HTML main controller -> Core editor
                    // However, IDE wants IOleCommandTarget and core editor, although managed,
                    // is represented by OLE command target as well. Since HTML controller
                    // is not specific to VS and does not use OLE, we create OLE-to-managed target shim
                    // and managed target-to-OLE shims. 

                    IVsEditorAdaptersFactoryService adapterService = VsAppShell.Current.ExportProvider.GetExport<IVsEditorAdaptersFactoryService>().Value;
                    IVsTextView viewAdapter = adapterService.GetViewAdapter(textView);

                    if (viewAdapter != null) {
                        // Create OLE shim that wraps main controller ICommandTarget and represents
                        // it as IOleCommandTarget that is accepted by VS IDE.
                        CommandTargetToOleShim oleController = new CommandTargetToOleShim(textView, mainController);

                        IOleCommandTarget nextOleTarget;
                        viewAdapter.AddCommandFilter(oleController, out nextOleTarget);

                        // nextOleTarget is typically a core editor wrapped into OLE layer.
                        // Create a wrapper that will present OLE target as ICommandTarget to
                        // HTML main controller so controller can operate in platform-agnostic way.
                        ICommandTarget nextCommandTarget = VsAppShell.Current.TranslateCommandTarget(textView, nextOleTarget);

                        mainController.ChainedController = nextCommandTarget;
                    }
                }
            }

            base.OnTextViewGotAggregateFocus(textView, textBuffer);
        }

        protected override void OnTextBufferCreated(ITextBuffer textBuffer) {
            // Force creations
            var appShell = VsAppShell.Current;
            InitEditorInstance(textBuffer);
            base.OnTextBufferCreated(textBuffer);
        }

        private void InitEditorInstance(ITextBuffer textBuffer) {
            if (ServiceManager.GetService<IEditorInstance>(textBuffer) == null) {
                ITextDocument textDocument;

                textBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDocument);
                Debug.Assert(textDocument != null);

                VsWorkspaceItem workspaceItem = new VsWorkspaceItem(textDocument.FilePath, textDocument.FilePath);

                ContentTypeImportComposer<IEditorFactory> importComposer = new ContentTypeImportComposer<IEditorFactory>(VsAppShell.Current.CompositionService);
                IEditorFactory factory = importComposer.GetImport(textBuffer.ContentType.TypeName);

                IEditorInstance editorInstance = factory.CreateEditorInstance(workspaceItem, textBuffer, new VsMdEditorDocumentFactory());
            }
        }
    }
}

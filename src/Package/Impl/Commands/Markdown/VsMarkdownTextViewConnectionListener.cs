// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.R.Package.Editors;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.Markdown;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Commands.Markdown {
    [Export(typeof(IWpfTextViewConnectionListener))]
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [Name("Visual Studio Markdown Editor Text View Connection Listener")]
    [Order(Before = "Default")]
    internal sealed class VsMarkdownTextViewConnectionListener : MdTextViewConnectionListener {
        private readonly IVsEditorAdaptersFactoryService _adapterService;
        private CommandTargetToOleShim _oleController;

        [ImportingConstructor]
        public VsMarkdownTextViewConnectionListener(ICoreShell coreShell): base(coreShell.Services) {
            _adapterService = Services.GetService<IVsEditorAdaptersFactoryService>();
        }

        protected override void OnTextViewGotAggregateFocus(ITextView textView, ITextBuffer textBuffer) {
            // Only attach controllers if the document is editable
            if (textView.Roles.Contains(PredefinedTextViewRoles.Editable)) {
                // Check if another buffer already attached a command controller to the view.
                // Don't allow two to be attached, or commands could be run twice.
                // This currently can only happen with inline diff views.
                var mainController = MdMainController.FromTextView(textView);
                if (textBuffer == mainController.TextBuffer) {
                    // Connect main controller to VS text view filter chain. The chain looks like
                    // VS IDE -> HTML main controller -> Core editor
                    // However, IDE wants IOleCommandTarget and core editor, although managed,
                    // is represented by OLE command target as well. Since HTML controller
                    // is not specific to VS and does not use OLE, we create OLE-to-managed target shim
                    // and managed target-to-OLE shims. 
                    OleControllerChain.ConnectController(Services, textView, mainController);
                }
            }

            base.OnTextViewGotAggregateFocus(textView, textBuffer);
        }

        protected override void OnTextViewCreated(ITextView textView) {
            // Ensure editor inherits core editor key bindings
            BaseEditorFactory.InitKeyBindings(textView);
            base.OnTextViewCreated(textView);
        }

        protected override void OnTextViewDisconnected(ITextView textView, ITextBuffer textBuffer) {
            if (textBuffer == textView.TextDataModel.DocumentBuffer && _oleController != null) {
                OleControllerChain.DisconnectController(_adapterService, textView, _oleController);
                _oleController = null;
            }
            base.OnTextViewDisconnected(textView, textBuffer);
        }

        protected override void OnTextBufferCreated(ITextView textView, ITextBuffer textBuffer) {
            VsAppShell.EnsurePackageLoaded(MdGuidList.MdPackageGuid);
            OleControllerChain.InitEditorInstance(textBuffer, Services);
            base.OnTextBufferCreated(textView, textBuffer);
        }
    }
}

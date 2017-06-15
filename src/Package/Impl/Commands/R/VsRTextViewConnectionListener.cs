// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Commands;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.R.Package.Editors;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Commands.R {
    [Export(typeof(IWpfTextViewConnectionListener))]
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [Name("Visual Studio R Editor Text View Connection Listener")]
    [Order(Before = "Default")]
    internal sealed class VsRTextViewConnectionListener : RTextViewConnectionListener {
        private readonly IVsEditorAdaptersFactoryService _adapterService;
        private CommandTargetToOleShim _oleController;

        [ImportingConstructor]
        public VsRTextViewConnectionListener(ICoreShell coreShell) : base(coreShell.Services) {
            _adapterService = coreShell.GetService<IVsEditorAdaptersFactoryService>();
        }

        protected override void OnTextViewGotAggregateFocus(ITextView textView, ITextBuffer textBuffer) {
            bool isProjected = (textBuffer != textView.TextDataModel.DocumentBuffer);
            // Only attach controllers if the document is editable
            if (!isProjected && textView.Roles.Contains(PredefinedTextViewRoles.Editable)) {
                // Check if another buffer already attached a command controller to the view.
                // Don't allow two to be attached, or commands could be run twice.
                // This currently can only happen with inline diff views.
                var mainController = RMainController.FromTextView(textView);
                if (textBuffer == mainController?.TextBuffer) {
                    // Connect main controller to VS text view filter chain.
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
            var clh = ContainedLanguageHost.GetHost(textView, textBuffer, Services);
            VsAppShell.EnsurePackageLoaded(RGuidList.RPackageGuid);
            OleControllerChain.InitEditorInstance(textBuffer, Services);
            base.OnTextBufferCreated(textView, textBuffer);
        }
    }
}

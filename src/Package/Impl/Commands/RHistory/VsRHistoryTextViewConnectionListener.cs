// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.History;
using Microsoft.R.Editor.Commands;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Commands.RHistory {
    [Export(typeof(IWpfTextViewConnectionListener))]
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(RHistoryContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    [Name("Visual Studio R History Text View Connection Listener")]
    [Order(Before = "Default")]
    internal sealed class VsRHistoryTextViewConnectionListener : RTextViewConnectionListener {
        [ImportingConstructor]
        public VsRHistoryTextViewConnectionListener(ICoreShell coreShell): base(coreShell.Services) { }

        protected override void OnTextViewGotAggregateFocus(ITextView textView, ITextBuffer textBuffer) {
            // Only attach controllers if the document is editable
            if (textView.Roles.Contains(PredefinedTextViewRoles.Interactive)) {
                // Check if another buffer already attached a command controller to the view.
                // Don't allow two to be attached, or commands could be run twice.
                // This currently can only happen with inline diff views.
                var mainController = textView.GetService<RMainController>();
                if (textBuffer == mainController.TextBuffer) {
                    // Connect main controller to VS text view filter chain. The chain looks like
                    // VS IDE -> main controller -> Core editor
                    // However, IDE wants IOleCommandTarget and core editor, although managed,
                    // is represented by OLE command target as well. Since HTML controller
                    // is not specific to VS and does not use OLE, we create OLE-to-managed target shim
                    // and managed target-to-OLE shims. 

                    var adapterService = Services.GetService<IVsEditorAdaptersFactoryService>();
                    var viewAdapter = adapterService.GetViewAdapter(textView);

                    if (viewAdapter != null) {
                        // Create OLE shim that wraps main controller ICommandTarget and represents
                        // it as IOleCommandTarget that is accepted by VS IDE.
                        var oleController = new CommandTargetToOleShim(textView, mainController);
                        var es = Services.GetService<IEditorSupport>();

                        viewAdapter.AddCommandFilter(oleController, out IOleCommandTarget nextOleTarget);

                        // nextOleTarget is typically a core editor wrapped into OLE layer.
                        // Create a wrapper that will present OLE target as ICommandTarget to
                        // HTML main controller so controller can operate in platform-agnostic way.
                        var nextCommandTarget = es.TranslateCommandTarget(textView.ToEditorView(), nextOleTarget);

                        mainController.ChainedController = nextCommandTarget;
                    }
                }
            }
            base.OnTextViewGotAggregateFocus(textView, textBuffer);
        }
    }
}

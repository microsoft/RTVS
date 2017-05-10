// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.ViewModel;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal static class OleControllerChain {
        public static CommandTargetToOleShim ConnectController(IServiceContainer services, ITextView textView, Controller controller) {
            var adapterService = services.GetService<IVsEditorAdaptersFactoryService>();
            var viewAdapter = adapterService.GetViewAdapter(textView);
            CommandTargetToOleShim oleControllerShim = null;

            // Connect main controller to VS text view filter chain.
            // The chain looks like VS IDE -> language main controller -> Core editor
            // However, IDE wants IOleCommandTarget and core editor, although managed,
            // is represented by OLE command target as well. Since R controller
            // is not specific to VS and does not use OLE, we create OLE-to-managed target shim
            // and managed target-to-OLE shims. 
            if (viewAdapter != null) {
                // Create OLE shim that wraps main controller ICommandTarget and represents
                // it as IOleCommandTarget that is accepted by VS IDE.
                oleControllerShim = new CommandTargetToOleShim(textView, controller);
                viewAdapter.AddCommandFilter(oleControllerShim, out var nextOleTarget);

                // nextOleTarget is typically a core editor wrapped into OLE layer.
                // Create a wrapper that will present OLE target as ICommandTarget to
                // the main controller so controller can operate in platform-agnostic way.
                var es = services.GetService<IEditorSupport>();
                var nextCommandTarget = es.TranslateCommandTarget(textView.ToEditorView(), nextOleTarget);
                controller.ChainedController = nextCommandTarget;
            }
            return oleControllerShim;
        }

        public static void DisconnectController(IVsEditorAdaptersFactoryService adapterService, ITextView textView, CommandTargetToOleShim oleControllerShim) {
            var viewAdapter = adapterService.GetViewAdapter(textView);
            viewAdapter?.RemoveCommandFilter(oleControllerShim);
        }

        public static void InitEditorInstance(ITextBuffer textBuffer, IServiceContainer services) {
            if (textBuffer.GetService<IEditorViewModel>() == null) {
                var locator = services.GetService<IContentTypeServiceLocator>();
                var viewModelFactory = locator.GetService<IEditorViewModelFactory>(textBuffer.ContentType.TypeName);
                viewModelFactory.CreateEditorViewModel(textBuffer);
            }
        }
    }
}

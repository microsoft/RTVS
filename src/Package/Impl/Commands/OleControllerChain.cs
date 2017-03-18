// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Document;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal static class OleControllerChain {
        public static CommandTargetToOleShim ConnectController(IVsEditorAdaptersFactoryService adapterService, ITextView textView, Controller controller) {
            IVsTextView viewAdapter = adapterService.GetViewAdapter(textView);
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

                IOleCommandTarget nextOleTarget;
                viewAdapter.AddCommandFilter(oleControllerShim, out nextOleTarget);

                // nextOleTarget is typically a core editor wrapped into OLE layer.
                // Create a wrapper that will present OLE target as ICommandTarget to
                // HTML main controller so controller can operate in platform-agnostic way.
                var es = VsAppShell.Current.GetService<IApplicationEditorSupport>();
                var nextCommandTarget = es.TranslateCommandTarget(textView, nextOleTarget);
                controller.ChainedController = nextCommandTarget;
            }
            return oleControllerShim;
        }

        public static void DisconnectController(IVsEditorAdaptersFactoryService adapterService, ITextView textView, CommandTargetToOleShim oleControllerShim) {
            IVsTextView viewAdapter = adapterService.GetViewAdapter(textView);
            if (viewAdapter != null) {
                viewAdapter.RemoveCommandFilter(oleControllerShim);
             }
        }

        public static void InitEditorInstance(ITextBuffer textBuffer) {
            if (ServiceManager.GetService<IEditorInstance>(textBuffer) == null) {
                var cs = VsAppShell.Current.GetService<ICompositionService>();
                var importComposer1 = new ContentTypeImportComposer<IEditorFactory>(cs);
                var editorInstanceFactory = importComposer1.GetImport(textBuffer.ContentType.TypeName);

                var importComposer2 = new ContentTypeImportComposer<IVsEditorDocumentFactory>(cs);
                var documentFactory = importComposer2.GetImport(textBuffer.ContentType.TypeName);

                var editorInstance = editorInstanceFactory.CreateEditorInstance(textBuffer, documentFactory);
            }
        }
    }
}

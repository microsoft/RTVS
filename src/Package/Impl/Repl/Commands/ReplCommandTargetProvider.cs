// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Controller;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl {

    [Export(typeof(IVsInteractiveWindowOleCommandTargetProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal sealed class ReplCommandTargetProvider : IVsInteractiveWindowOleCommandTargetProvider {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public ReplCommandTargetProvider(ICoreShell shell) {
            _shell = shell;
        }

        public IOleCommandTarget GetCommandTarget(IWpfTextView textView, IOleCommandTarget nextTarget) {
            IOleCommandTarget target = ServiceManager.GetService<IOleCommandTarget>(textView);
            if (target == null) {
                ReplCommandController controller = ReplCommandController.Attach(textView, textView.TextBuffer);

                // Wrap controller into OLE command target
                target = VsAppShell.Current.TranslateToHostCommandTarget(textView, controller) as IOleCommandTarget;
                Debug.Assert(target != null);

                ServiceManager.AddService(target, textView, _shell);

                // Wrap next OLE target in the chain into ICommandTarget so we can have 
                // chain like: OLE Target -> Shim -> ICommandTarget -> Shim -> Next OLE target
                ICommandTarget nextCommandTarget = VsAppShell.Current.TranslateCommandTarget(textView, nextTarget);
                controller.ChainedController = nextCommandTarget;

                // We need to listed when R projected buffer is attached and 
                // create R editor document over it.
                textView.BufferGraph.GraphBuffersChanged += OnGraphBuffersChanged;
                IProjectionBuffer pb = textView.TextBuffer as IProjectionBuffer;
                if (pb != null) {
                    pb.SourceBuffersChanged += OnSourceBuffersChanged;
                }

                textView.Closed += TextView_Closed;
            }

            return target;
        }

        private void TextView_Closed(object sender, EventArgs e) {
            IWpfTextView textView = sender as IWpfTextView;
            if (textView != null) {
                if (textView.BufferGraph != null) {
                    textView.BufferGraph.GraphBuffersChanged -= OnGraphBuffersChanged;
                }

                IProjectionBuffer pb = textView.TextBuffer as IProjectionBuffer;
                if (pb != null) {
                    pb.SourceBuffersChanged -= OnSourceBuffersChanged;
                }

                textView.Closed -= TextView_Closed;
                ReplCommandController controller = ReplCommandController.FromTextView(textView);
                if (controller != null) {
                    controller.Dispose();
                }
            }
        }

        private void OnSourceBuffersChanged(object sender, ProjectionSourceBuffersChangedEventArgs e) {
            HandleAddRemoveBuffers(e.AddedBuffers, e.RemovedBuffers);
        }

        private void OnGraphBuffersChanged(object sender, GraphBuffersChangedEventArgs e) {
            HandleAddRemoveBuffers(e.AddedBuffers, e.RemovedBuffers);
        }

        private void HandleAddRemoveBuffers(ReadOnlyCollection<ITextBuffer> addedBuffers, ReadOnlyCollection<ITextBuffer> removedBuffers) {
            foreach (ITextBuffer tb in addedBuffers) {
                if (tb.ContentType.IsOfType(RContentTypeDefinition.ContentType)) {
                    IREditorDocument doc = REditorDocument.TryFromTextBuffer(tb);
                    if (doc == null) {
                        var editorDocument = new REditorDocument(tb, _shell);
                    }
                }
            }

            foreach (ITextBuffer tb in removedBuffers) {
                if (tb.ContentType.IsOfType(RContentTypeDefinition.ContentType)) {
                    IREditorDocument doc = REditorDocument.TryFromTextBuffer(tb);
                    if (doc != null) {
                        doc.Close();
                    }
                }
            }
        }
    }
}

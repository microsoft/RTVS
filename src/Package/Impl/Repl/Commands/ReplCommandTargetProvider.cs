// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
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
            EditorView.Create(textView);
            var target = textView.GetService<IOleCommandTarget>();
            if (target == null) {
                var controller = ReplCommandController.Attach(textView, textView.TextBuffer, _shell.Services);
                var es = _shell.GetService<IEditorSupport>();
                // Wrap controller into OLE command target
                target = es.TranslateToHostCommandTarget(textView.ToEditorView(), controller) as IOleCommandTarget;
                Debug.Assert(target != null);

                textView.AddService(target);

                // Wrap next OLE target in the chain into ICommandTarget so we can have 
                // chain like: OLE Target -> Shim -> ICommandTarget -> Shim -> Next OLE target
                var nextCommandTarget = es.TranslateCommandTarget(textView.ToEditorView(), nextTarget);
                controller.ChainedController = nextCommandTarget;

                // We need to listed when R projected buffer is attached and 
                // create R editor document over it.
                textView.BufferGraph.GraphBuffersChanged += OnGraphBuffersChanged;
                var pb = textView.TextBuffer as IProjectionBuffer;
                if (pb != null) {
                    pb.SourceBuffersChanged += OnSourceBuffersChanged;
                }

                textView.Closed += TextView_Closed;
            }
            return target;
        }

        private void TextView_Closed(object sender, EventArgs e) {
            var textView = sender as IWpfTextView;
            if (textView != null) {
                if (textView.BufferGraph != null) {
                    textView.BufferGraph.GraphBuffersChanged -= OnGraphBuffersChanged;
                }

                var pb = textView.TextBuffer as IProjectionBuffer;
                if (pb != null) {
                    pb.SourceBuffersChanged -= OnSourceBuffersChanged;
                }

                textView.Closed -= TextView_Closed;
                var controller = ReplCommandController.FromTextView(textView);
                    controller?.Dispose();
            }
        }

        private void OnSourceBuffersChanged(object sender, ProjectionSourceBuffersChangedEventArgs e)
            => HandleAddRemoveBuffers(e.AddedBuffers, e.RemovedBuffers);

        private void OnGraphBuffersChanged(object sender, GraphBuffersChangedEventArgs e)
            => HandleAddRemoveBuffers(e.AddedBuffers, e.RemovedBuffers);

        private void HandleAddRemoveBuffers(ReadOnlyCollection<ITextBuffer> addedBuffers, ReadOnlyCollection<ITextBuffer> removedBuffers) {
            foreach (var tb in addedBuffers) {
                if (tb.ContentType.IsOfType(RContentTypeDefinition.ContentType)) {
                    var doc = tb.GetEditorDocument<IREditorDocument>();
                    if (doc == null) {
                        var eb = EditorBuffer.Create(tb, _shell.GetService<ITextDocumentFactoryService>());
                        new REditorDocument(eb, _shell.Services);
                    }
                }
            }

            foreach (var tb in removedBuffers) {
                if (tb.ContentType.IsOfType(RContentTypeDefinition.ContentType)) {
                    var doc = tb.GetEditorDocument<IREditorDocument>();
                    doc?.Close();
                }
            }
        }
    }
}

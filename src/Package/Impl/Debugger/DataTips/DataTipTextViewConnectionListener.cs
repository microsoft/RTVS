// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Debugger.DataTips {
    [Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Debuggable)]
    internal sealed class DataTipTextViewConnectionListener : IWpfTextViewConnectionListener {
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public DataTipTextViewConnectionListener(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        public void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers) {
            if (!textView.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType)) {
                return;
            }

            _coreShell.MainThread().Post(() => {
                var debugger = _coreShell.GetService<IVsDebugger>();
                var adaptersFactoryService = _coreShell.GetService<IVsEditorAdaptersFactoryService>();
                DataTipTextViewFilter.GetOrCreate(textView, adaptersFactoryService, debugger);
            });
        }

        public void SubjectBuffersDisconnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers) {
            if (!textView.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType)) {
                DataTipTextViewFilter.TryGet(textView)?.Dispose();
            }
        }
    }
}

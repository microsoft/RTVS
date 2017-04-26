// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class EditorDocumentMock : IREditorDocument {
        public EditorDocumentMock(string content, string filePath = null) {
            var tb = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            EditorTree = new EditorTreeMock(new EditorBuffer(tb), RParser.Parse(content));
            tb.AddService(this);
            if (!string.IsNullOrEmpty(filePath)) {
                tb.Properties.AddProperty(typeof(ITextDocument), new TextDocumentMock(tb, filePath));
            }
        }

        public EditorDocumentMock(IREditorTree tree) {
            EditorTree = tree;
            tree.EditorBuffer.AddService(this);
        }

        public IREditorTree EditorTree { get; private set; }

        public void Close() { }

        public bool IsTransient => false;

        public bool IsClosed => false;

        public bool IsMassiveChangeInProgress => false;
        public bool IsProjected { get; set; }

        public IEditorBuffer EditorBuffer => EditorTree.EditorBuffer;

        public string FilePath { get; set; }

#pragma warning disable 67
        private readonly object _syncObj = new object();

        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;

        public EventHandler<EventArgs> Closing { get; private set; }
        event EventHandler<EventArgs> IEditorDocument.Closing {
            add {
                lock (_syncObj) {
                    Closing = (EventHandler<EventArgs>)Delegate.Combine(Closing, value);
                }
            }
            remove {
                lock (_syncObj) {
                    Closing = (EventHandler<EventArgs>)Delegate.Remove(Closing, value);
                }
            }
        }

        public event EventHandler<EventArgs> MassiveChangeBegun;
        public event EventHandler<EventArgs> MassiveChangeEnded;

        public void BeginMassiveChange() { }
        public void Dispose() { }
        public bool EndMassiveChange() => true;

        public IEditorView PrimaryView => null;

    }
}

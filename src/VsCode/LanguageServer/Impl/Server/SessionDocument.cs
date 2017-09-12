// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Based on https://github.com/CXuesong/LanguageServer.NET

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageServer.VsCode.Contracts;
using LanguageServer.VsCode.Server;

namespace Microsoft.R.LanguageServer.Server {
    public sealed class SessionDocument {
        /// <summary>
        /// Actually makes the changes to the inner document per this milliseconds.
        /// </summary>
        private const int RenderChangesDelay = 100;

        public SessionDocument(TextDocumentItem doc) {
            Document = TextDocument.Load<FullTextDocument>(doc);
        }

        private Task updateChangesDelayTask;

        private readonly object syncLock = new object();

        private List<TextDocumentContentChangeEvent> impendingChanges = new List<TextDocumentContentChangeEvent>();

        public event EventHandler DocumentChanged;

        public TextDocument Document { get; set; }

        public void NotifyChanges(IEnumerable<TextDocumentContentChangeEvent> changes) {
            lock (syncLock) {
                if (impendingChanges == null) {
                    impendingChanges = changes.ToList();
                } else {
                    impendingChanges.AddRange(changes);
                }
            }
            if (updateChangesDelayTask == null || updateChangesDelayTask.IsCompleted) {
                updateChangesDelayTask = Task.Delay(RenderChangesDelay);
                updateChangesDelayTask.ContinueWith(t => Task.Run((Action)MakeChanges));
            }
        }

        private void MakeChanges() {
            List<TextDocumentContentChangeEvent> localChanges;
            lock (syncLock) {
                localChanges = impendingChanges;
                if (localChanges == null || localChanges.Count == 0) {
                    return;
                }

                impendingChanges = null;
            }
            Document = Document.ApplyChanges(localChanges);
            if (impendingChanges == null) {
                localChanges.Clear();
                lock (syncLock) {
                    if (impendingChanges == null) {
                        impendingChanges = localChanges;
                    }
                }
            }
            DocumentChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

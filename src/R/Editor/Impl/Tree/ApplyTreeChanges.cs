// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Languages.Core.Text;

namespace Microsoft.R.Editor.Tree {
    public partial class EditorTree {
        internal List<TreeChangeEventRecord> ApplyChangesFromQueue(Queue<EditorTreeChange> queue) {
            if (_ownerThread != Thread.CurrentThread.ManagedThreadId)
                throw new ThreadStateException("Method should only be called on the main thread");

            var changesToFire = new List<TreeChangeEventRecord>();

            if (queue == null || queue.Count == 0)
                return changesToFire;

            // Since we have write lock we cannot fire events. If we fire an event,
            // listener may try and access the tree while a) tree not ready and
            // b) accessing AstRoot may check tree readiness and since tree is not
            // ready yet (as it is still applying changes) it may try and update
            // tree on its own and end up hanging trying to acquire write lock again.
            // Hence we must store events in a list and fire then when update 
            // is done and the lock is released.

            try {
                AcquireWriteLock();

                while (queue.Count > 0) {
                    var change = queue.Dequeue();

                    switch (change.ChangeType) {
                        case TreeChangeType.NewTree: {
                                var c = change as EditorTreeChange_NewTree;
                                _astRoot = c.NewTree;
                                changesToFire.Add(new TreeChangeEventRecord(change.ChangeType));
                            }
                            break;

                        default:
                            Debug.Fail("Unknown tree change");
                            break;
                    }
                }
            } finally {
                ReleaseWriteLock();
            }
            return changesToFire;
        }

        internal void FirePostUpdateEvents(TreeChangeEventRecord changes, bool fullParse) {
            FireOnUpdatesPending(changes);
            FireOnUpdateBegin();

            foreach (var a in _actionsToInvokeOnReady.Values) {
                a.Action(a.Parameter);
            }
            _actionsToInvokeOnReady.Clear();
            FireOnUpdateCompleted(TreeUpdateType.NewTree);
        }
    }
}

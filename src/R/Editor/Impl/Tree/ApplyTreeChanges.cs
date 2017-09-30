// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.R.Editor.Tree {
    public partial class EditorTree {
        internal void ApplyChanges(IEnumerable<EditorTreeChange> changes) {
            Check.InvalidOperation(() => _ownerThread == Thread.CurrentThread.ManagedThreadId, _threadCheckMessage);
            if (changes == null || !changes.Any()) {
                return;
            }

            Debug.WriteLine("AST: changes applied");

            // Since we have write lock we cannot fire events. If we fire an event,
            // listener may try and access the tree while a) tree not ready and
            // b) accessing AstRoot may check tree readiness and since tree is not
            // ready yet (as it is still applying changes) it may try and update
            // tree on its own and end up hanging trying to acquire write lock again.
            // Hence we must store events in a list and fire then when update 
            // is done and the lock is released.

            try {
                AcquireWriteLock();
                foreach (var change in changes) {
                    switch (change.ChangeType) {
                        case TreeChangeType.NewTree: {
                                var c = change as EditorTreeChange_NewTree;
                                _astRoot = c.NewTree;
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
        }

        internal void FirePostUpdateEvents() {
            FireOnUpdatesPending();
            FireOnUpdateBegin();

            foreach (var a in _actionsToInvokeOnReady.Values.ToList()) {
                a.Action(a.Parameter);
            }
            _actionsToInvokeOnReady.Clear();
            FireOnUpdateCompleted(TreeUpdateType.NewTree);
        }
    }
}

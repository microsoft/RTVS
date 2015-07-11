using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Editor.Tree
{
    public partial class EditorTree
    {
        internal List<TreeChangeEventRecord> ApplyChangesFromQueue(Queue<EditorTreeChange> queue)
        {
            if (_creatorThread != Thread.CurrentThread.ManagedThreadId)
                throw new ThreadStateException("Method should only be called on the main thread");

            Stopwatch sw = null;
            Stopwatch sw1 = null;
            if (TreeUpdateTask.TraceParse.Enabled)
            {
                sw = new Stopwatch();
                sw.Start();

                sw1 = new Stopwatch();
            }

            var changesToFire = new List<TreeChangeEventRecord>();

            if (queue == null || queue.Count == 0)
                return changesToFire;

            // Since we have write lock we cannot fire events. If we fire an event, listener may try 
            // and access the tree while a) tree not ready and b) calling HtmlEditorTree.RootNode will 
            // check tree readiness and since we are not ready yet (still applying changes) it may try
            // and update tree on its own and end up hanging trying to acquire write lock again. 
            // Hence we must store events in a list and fire then when update is done and lock is released.

            try
            {
                AcquireWriteLock();

                while (queue.Count > 0)
                {
                    if (TreeUpdateTask.TraceParse.Enabled)
                    {
                        sw1.Start();
                    }

                    var change = queue.Dequeue();

                    switch (change.ChangeType)
                    {
                        case TreeChangeType.NewTree:
                            {
                                var c = change as EditorTreeChange_NewTree;
                                _astRoot = c.NewTree;

                                changesToFire.Add(new TreeChangeEventRecord(change.ChangeType));
                            }
                            break;

                        case TreeChangeType.TokenChange:
                            {
                                var c = change as EditorTreeChange_TokenNodeChanged;
                                IAstNode node = AstRoot.GetNode(c.NodeKey);
                                changesToFire.Add(new TreeChangeEventRecord(change.ChangeType, node));
                            }
                            break;

                        default:
                            Debug.Fail("Unknown tree change");
                            break;
                    }

                    if (TreeUpdateTask.TraceParse.Enabled)
                    {
                        sw1.Stop();
                        Debug.WriteLine(String.Format(CultureInfo.CurrentCulture, "Tree apply change {0} -- {1} ms", change.ChangeType.ToString(), sw1.ElapsedMilliseconds));
                        sw1.Reset();
                    }
                }
            }
            finally
            {
                ReleaseWriteLock();
            }

            if (TreeUpdateTask.TraceParse.Enabled)
            {
                sw.Stop();
                Debug.WriteLine(String.Format(CultureInfo.CurrentCulture, "HTML ApplyChangesFromQueue: {0} ms", sw.ElapsedMilliseconds));
            }

            return changesToFire;
        }

        internal void FirePostUpdateEvents(List<TreeChangeEventRecord> changes, bool fullParse)
        {
            // Fire update/begin end even if there are no actual changes. This allows
            // listeners to better track changes like clean element delete. On clean delete
            // element removed from the tree right away without waiting for background
            // parse to happen. First event comes as simple tree update after text change 
            // with tree still in a dirty state. After the parse 'tree updated' event comes 
            // telling that tree is no longer dirty but there are no actual tree changes
            // since element has been removed on the text change.

            // Fire UpdatesPending notification, even though we don't have ranges for the event
            List<TextChangeEventArgs> textChanges = new List<TextChangeEventArgs>();
            FireOnUpdatesPending(textChanges);

            FireOnUpdateBegin();

            if (changes.Count > 0)
            {
                foreach (var change in changes)
                {
                    switch (change.ChangeType)
                    {
                        case TreeChangeType.TokenChange:
                            FireOnTokenNodeChanged(change.Node);
                            break;

                        default:
                            Debug.Fail("Unknown tree change");
                            break;
                    }
                }
            }

            FireOnUpdateCompleted(TreeUpdateType.NodesChanged, fullParse);
        }
    }
}

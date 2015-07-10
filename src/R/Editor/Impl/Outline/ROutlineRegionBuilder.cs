using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Threading;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Outline;
using Microsoft.Languages.Editor.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Outline
{
    internal sealed class ROutlineRegionBuilder : OutlineRegionBuilder, IAstVisitor
    {
        class OutliningContext
        {
            public int LastRegionStartLineNumber = -1;
            public int LastRegionEndLineNumber = -1;

            public OutlineRegionCollection Regions;
        }

        private static readonly Guid _treeUserId = new Guid("15B63323-6670-4D24-BDD7-FF71DD14CD5A");
        private const int _minLinesToOutline = 3;

        private EditorDocument _document;
        private EditorTree _tree;

        private object _threadLock = new object();

        public ROutlineRegionBuilder(EditorDocument document)
            : base(document.EditorTree.TextBuffer)
        {
            _document = document;
            _document.OnDocumentClosing += OnDocumentClosing;

            _tree = document.EditorTree;
            _tree.UpdateCompleted += OnTreeUpdateCompleted;
        }

        protected override void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            if (e.Before.LineCount != e.After.LineCount)
            {
                BackgroundTask.DoTaskOnIdle();
            }

            base.OnTextBufferChanged(sender, e);
        }

        private void OnTreeUpdateCompleted(object sender, TreeUpdatedEventArgs e)
        {
            if (e.UpdateType == TreeUpdateType.NodesChanged)
            {
                BackgroundTask.DoTaskOnIdle();
            }
        }

        private void OnDocumentClosing(object sender, EventArgs e)
        {
            // Make sure background thread is not building regions
            lock (_threadLock)
            {
                Dispose();
            }
        }

        protected override bool BuildRegions(OutlineRegionCollection newRegions)
        {
            lock (_threadLock)
            {
                if (IsDisposed || !_tree.IsReady)
                {
                    return false;
                }

                AstRoot rootNode = null;

                try
                {
                    rootNode = _tree.AcquireReadLock(_treeUserId);
                    if (rootNode != null)
                    {
                        OutliningContext context = new OutliningContext();
                        context.Regions = newRegions;

                        rootNode.Accept(this, context);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Fail(String.Format(CultureInfo.CurrentCulture, "Exception in outliner: {0}", ex.Message));
                }
                finally
                {
                    if (rootNode != null)
                    {
                        _tree.ReleaseReadLock(_treeUserId);
                    }
                    else
                    {
                        // Tree was busy. Will try again later.
                        GuardedOperations.DispatchInvoke(() => BackgroundTask.DoTaskOnIdle(), DispatcherPriority.Normal);
                    }
                }

                return true;
            }
        } 

        protected override void MainThreadAction(object backgroundProcessingResult)
        {
            if (!IsDisposed)
            {
                base.MainThreadAction(backgroundProcessingResult);
            }
        }

        #region IAstVisitor
        public bool Visit(IAstNode node, object param)
        {
            OutliningContext context = param as OutliningContext;
            int startLineNumber, endLineNumber;

            if (OutlineNode(node, out startLineNumber, out endLineNumber))
            {
                if (context.LastRegionStartLineNumber == startLineNumber && context.LastRegionEndLineNumber != endLineNumber)
                {
                    // Always prefer outer (bigger) region.
                    var lastRegion = context.Regions[context.Regions.Count - 1];
                    if (lastRegion.Length < node.Length)
                    {
                        context.Regions.RemoveAt(context.Regions.Count - 1);
                        context.Regions.Add(new ROutlineRegion(_document.TextBuffer, node));
                    }
                }
                else if (context.LastRegionStartLineNumber != startLineNumber)
                {
                    context.Regions.Add(new ROutlineRegion(_document.TextBuffer, node));

                    context.LastRegionStartLineNumber = startLineNumber;
                    context.LastRegionEndLineNumber = endLineNumber;
                }
            }

            return true;
        }
        #endregion

        private static bool OutlineRange(ITextSnapshot snapshot, ITextRange range, bool trimEmptyLines, out int startLineNumber, out int endLineNumber)
        {
            int start = Math.Max(0, range.Start);
            int end = Math.Min(range.End, snapshot.Length);

            startLineNumber = endLineNumber = 0;

            if (start < end)
            {
                startLineNumber = snapshot.GetLineNumberFromPosition(start);
                endLineNumber = snapshot.GetLineNumberFromPosition(end);

                if (trimEmptyLines)
                {
                    var startLine = snapshot.GetLineFromPosition(start);
                    var endLine = snapshot.GetLineFromPosition(end);

                    var text = snapshot.GetText(start, startLine.EndIncludingLineBreak - start);
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        startLineNumber++;
                    }

                    text = snapshot.GetText(endLine.Start, end - endLine.Start);
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        endLineNumber--;
                    }
                }

                return endLineNumber - startLineNumber + 1 >= _minLinesToOutline;
            }

            return false;
        }

        private bool OutlineNode(IAstNode node, out int startLineNumber, out int endLineNumber)
        {
            if (node is AstRoot)
            {
                startLineNumber = endLineNumber = 0;
                return false;
            }

            return OutlineRange(_tree.TextSnapshot, node, false, out startLineNumber, out endLineNumber);
        }

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                _document.OnDocumentClosing -= OnDocumentClosing;
                _document = null;

                _tree.UpdateCompleted -= OnTreeUpdateCompleted;
                _tree = null;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}

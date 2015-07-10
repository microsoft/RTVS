using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Outline;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Outline
{
    internal sealed class ROutlineRegionBuilder : OutlineRegionBuilder, IAstVisitor
    {
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
                        rootNode.Accept(this, newRegions);
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
                        BackgroundTask.DoTaskOnIdle();
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

        #region IHtmlTreeVisitor
        public bool Visit(IAstNode node, object param)
        {
            var regions = param as TextRangeCollection<OutlineRegion>;

            if (OutlineNode(node))
            {
                regions.Add(new ROutlineRegion(_document.TextBuffer, node));
            }

            return true;
        }
        #endregion

        private static bool OutlineRange(ITextSnapshot snapshot, ITextRange range, bool trimEmptyLines = false)
        {
            int start = Math.Max(0, range.Start);
            int end = Math.Min(range.End, snapshot.Length);

            if (start < end)
            {
                var startLineNumber = snapshot.GetLineNumberFromPosition(start);
                var endLineNumber = snapshot.GetLineNumberFromPosition(end);

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

        private bool OutlineNode(IAstNode node)
        {
            if (node is AstRoot)
            {
                return false;
            }

            return OutlineRange(_tree.TextSnapshot, node);
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

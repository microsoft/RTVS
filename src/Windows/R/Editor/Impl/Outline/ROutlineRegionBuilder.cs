// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Outline;
using Microsoft.Languages.Editor.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Outline {
    /// <summary>
    /// Code outline region builder. Runs asynchronously but starts
    /// on next idle slot after the most recent tree change.
    /// </summary>
    internal sealed class ROutlineRegionBuilder : OutlineRegionBuilder, IAstVisitor {
        class OutliningContext {
            public int LastRegionStartLineNumber = -1;
            public int LastRegionEndLineNumber = -1;

            public OutlineRegionCollection Regions;
        }

        private static readonly Guid _treeUserId = new Guid("15B63323-6670-4D24-BDD7-FF71DD14CD5A");
        private const int _minLinesToOutline = 3;
        private readonly object _threadLock = new object();
        private RSectionsCollection _sections;
        private bool _forceRegionsChange = false;

        internal IREditorDocument EditorDocument { get; }
        internal IREditorTree EditorTree { get; }

        public ROutlineRegionBuilder(IREditorDocument document, IServiceContainer services)
            : base(document.EditorTree.TextBuffer(), services, services.GetService<IREditorSettings>().EnableOutlining) {
            EditorDocument = document;
            EditorDocument.Closing += OnDocumentClosing;

            EditorTree = document.EditorTree;
            EditorTree.UpdateCompleted += OnTreeUpdateCompleted;
            EditorTree.Closing += OnEditorTreeClosing;
        }

        protected override bool IsEnabled => Services.GetService<IREditorSettings>().EnableOutlining;

        protected override void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) {
            if (IsEnabled && e.Before.LineCount != e.After.LineCount) {
                BackgroundTask.DoTaskOnIdle();
            }
            base.OnTextBufferChanged(sender, e);
        }

        private void OnTreeUpdateCompleted(object sender, TreeUpdatedEventArgs e) {
            if (IsEnabled && (e.UpdateType != TreeUpdateType.PositionsOnly || (_sections != null && _sections.Changed))) {
                BackgroundTask.DoTaskOnIdle();
            }
        }

        private void OnDocumentClosing(object sender, EventArgs e) {
            // Make sure background thread is not building regions
            lock (_threadLock) {
                Dispose();
            }
        }

        private void OnEditorTreeClosing(object sender, EventArgs e) {
            // Make sure background thread is not building regions
            lock (_threadLock) {
                Dispose();
            }
        }

        public override bool BuildRegions(OutlineRegionCollection newRegions) {
            lock (_threadLock) {
                if (IsDisposed || !EditorTree.IsReady) {
                    return false;
                }

                AstRoot rootNode = null;
                try {
                    // We are in a background thread so in order to walk the tree
                    // we must obtain the read lock first.
                    rootNode = EditorTree.AcquireReadLock(_treeUserId);
                    if (rootNode != null) {
                        var context = new OutliningContext();
                        context.Regions = newRegions;
                        // Walk the tree and construct new regions
                        rootNode.Accept(this, context);
                        OutlineSections(rootNode, context);
                    }
                } catch (Exception) { } finally {
                    if (rootNode != null) {
                        EditorTree.ReleaseReadLock(_treeUserId);
                    } else {
                        // Tree was busy. Will try again later.
                        GuardedOperations.DispatchInvoke(() => BackgroundTask.DoTaskOnIdle(), DispatcherPriority.Normal);
                    }
                }
                return true;
            }
        }


        protected override void MainThreadAction(object backgroundProcessingResult) {
            if (!IsDisposed) {
                base.MainThreadAction(backgroundProcessingResult);
            }
        }

        protected override ITextRange CompareRegions(OutlineRegionCollection newRegions, OutlineRegionCollection oldRegions, int upperBound) {
            // Determine if we must force change on the editor if section name(s) changed.
            // Regular change may or may not re-create collapsible regions since if regions
            // positions and lengths match optimization will not be rebuilding regions.
            if (!_forceRegionsChange) {
                return base.CompareRegions(newRegions, oldRegions, upperBound);
            }
            _forceRegionsChange = false;
            return new TextRange(0, EditorTree.EditorBuffer.CurrentSnapshot.Length);
        }

        #region IAstVisitor
        public bool Visit(IAstNode node, object param) {
            OutliningContext context = param as OutliningContext;
            int startLineNumber, endLineNumber;

            if (OutlineNode(node, out startLineNumber, out endLineNumber)) {
                if (context.LastRegionStartLineNumber == startLineNumber && context.LastRegionEndLineNumber != endLineNumber) {
                    // Always prefer outer (bigger) region.
                    var lastRegion = context.Regions[context.Regions.Count - 1];
                    if (lastRegion.Length < node.Length) {
                        context.Regions.RemoveAt(context.Regions.Count - 1);
                        context.Regions.Add(new ROutlineRegion(EditorDocument.TextBuffer(), node));
                    }
                } else if (context.LastRegionStartLineNumber != startLineNumber) {
                    context.Regions.Add(new ROutlineRegion(EditorDocument.TextBuffer(), node));
                    context.LastRegionStartLineNumber = startLineNumber;
                    context.LastRegionEndLineNumber = endLineNumber;
                }
            }

            return true;
        }
        #endregion

        /// <summary>
        /// Outlines comments that define sections such as
        /// # NAME ---
        /// </summary>
        /// <returns>True if names changed and outline regions need to be rebuilt</returns>
        private void OutlineSections(AstRoot ast, OutliningContext context) {
            // Collect comments that define sections
            var snapshot = EditorTree.TextSnapshot();
            var sections = ast.Comments.Where(c => {
                var text = snapshot.GetText(new Span(c.Start, c.Length));
                // Section looks like # [NAME] --------
                return text.TrimEnd().EndsWithOrdinal("---") && text.IndexOfAny(CharExtensions.LineBreakChars) < 0;
            }).ToArray();

            // Construct collapsible regions
            var ranges = new List<ITextRange>();
            for (var i = 0; i < sections.Length; i++) {
                var startLine = snapshot.GetLineFromPosition(sections[i].Start);
                var end = -1;

                var text = snapshot.GetText(new Span(sections[i].Start, sections[i].Length));
                var displayText = text.Substring(0, text.IndexOfOrdinal("---")).Trim();

                if (i < sections.Length - 1) {
                    var endLineNumber = snapshot.GetLineNumberFromPosition(sections[i + 1].Start);
                    if (endLineNumber > startLine.LineNumber) {
                        end = snapshot.GetLineFromLineNumber(endLineNumber - 1).End;
                    }
                } else {
                    end = snapshot.Length;
                }

                if (end > startLine.Start) {
                    ranges.Add(sections[i]);

                    // Trim trailing whitespace in user-defined regions
                    var range = TextRange.FromBounds(startLine.Start, end);
                    text = snapshot.GetText(new Span(range.Start, range.Length));
                    var trimBy = text.Length - text.TrimEnd().Length;
                    range = TextRange.FromBounds(range.Start, range.End - trimBy);

                    context.Regions.Add(new ROutlineRegion(EditorDocument.TextBuffer(), range, displayText));
                }
            }

            // Track changes in section names
            _forceRegionsChange = _sections != null && _sections.Changed;

            _sections?.Dispose();
            _sections = new RSectionsCollection(EditorTree, ranges);
        }


        /// <summary>
        /// Determines if range is large enough to be outlined
        /// </summary>
        private static bool OutlineRange(ITextSnapshot snapshot, ITextRange range, out int startLineNumber, out int endLineNumber) {
            var start = Math.Max(0, range.Start);
            var end = Math.Min(range.End, snapshot.Length);

            startLineNumber = endLineNumber = 0;
            if (start < end) {
                startLineNumber = snapshot.GetLineNumberFromPosition(start);
                endLineNumber = snapshot.GetLineNumberFromPosition(end);

                return endLineNumber - startLineNumber + 1 >= _minLinesToOutline;
            }
            return false;
        }

        /// <summary>
        /// Determines if particular AST node produces collapsible region
        /// </summary>
        private bool OutlineNode(IAstNode node, out int startLineNumber, out int endLineNumber) {
            if (node is AstRoot || node is GlobalScope) {
                startLineNumber = endLineNumber = 0;
                return false;
            }
            return OutlineRange(EditorTree.TextSnapshot(), node, out startLineNumber, out endLineNumber);
        }

        #region IDisposable
        protected override void Dispose(bool disposing) {
            if (!IsDisposed) {
                EditorDocument.Closing -= OnDocumentClosing;
                EditorTree.UpdateCompleted -= OnTreeUpdateCompleted;
                EditorTree.Closing -= OnEditorTreeClosing;
            }

            _sections?.Dispose();
            base.Dispose(disposing);
        }
        #endregion
    }
}

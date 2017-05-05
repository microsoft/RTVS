// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Editor.Tree {
    /// <summary>
    /// Describes complete context of the text change
    /// including text ranges, affected editor tree
    /// and changed AST node.
    /// </summary>
    internal class TextChangeContext {
        public IREditorTree EditorTree { get; }

        /// <summary>
        /// Most recent change start in the previous snapshot
        /// </summary>
        public int OldStart { get; }
        /// <summary>
        /// Most recent change length in the previous snapshot
        /// </summary>
        public int OldLength { get; }

        /// <summary>
        /// Most recent change start in the new snapshot
        /// </summary>
        public int NewStart { get; }

        /// <summary>
        /// Most recent change length in the new snapshot
        /// </summary>
        public int NewLength { get; }

        /// <summary>
        /// Previous snapshot text
        /// </summary>
        public ITextProvider OldTextProvider { get; }

        /// <summary>
        /// New snapshot text
        /// </summary>
        public ITextProvider NewTextProvider { get; }

        /// <summary>
        /// Changes accumulated since last tree update
        /// </summary>
        public TextChange PendingChanges { get; }

        /// <summary>
        /// Most recently changed node (if change was AST node change)
        /// </summary>
        public IAstNode ChangedNode { get; set; }

        /// <summary>
        /// Most recently changed comment (if change was inside comments)
        /// </summary>
        public RToken ChangedComment { get; set; }

        private TextRange _oldRange;
        private TextRange _newRange;
        private string _oldText;
        private string _newText;

        public TextChangeContext(IREditorTree editorTree, TextChangeEventArgs change, TextChange pendingChanges) {
            EditorTree = editorTree;
            NewStart = change.Start;
            OldStart = change.OldStart;
            OldLength = change.OldLength;
            NewLength = change.NewLength;

            OldTextProvider = change.OldText ?? editorTree.AstRoot.TextProvider;
            NewTextProvider = change.NewText ?? editorTree.AstRoot.TextProvider;
            PendingChanges = pendingChanges;

            var textChange = new TextChange {
                Start = this.OldRange.Start,
                OldEnd = this.OldRange.End,
                OldTextProvider = this.OldTextProvider,
                NewEnd = this.NewRange.End,
                NewTextProvider = this.NewTextProvider,
                Version = this.NewTextProvider.Version
            };

            PendingChanges.Combine(textChange);
        }

        /// <summary>
        /// Range of changes in the previous snapshot
        /// </summary>
        public TextRange OldRange {
            get {
                _oldRange = _oldRange ?? new TextRange(OldStart, OldLength);
                return _oldRange;
            }
        }

        /// <summary>
        /// Range of changes in the new snapshot
        /// </summary>
        public TextRange NewRange {
            get {
                _newRange = _newRange ?? new TextRange(NewStart, NewLength);
                return _newRange;
            }
        }

        /// <summary>
        /// Changed text in the previous snapshot
        /// </summary>
        public string OldText {
            get {
                _oldText = _oldText ?? OldTextProvider.GetText(OldRange);
                return _oldText;
            }
        }

        /// <summary>
        /// Changed text in the new snapshot
        /// </summary>
        public string NewText {
            get {
                _newText = _newText ?? NewTextProvider.GetText(NewRange);
                return _newText;
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Editor.Tree {
    /// <summary>
    /// Describes complete context of the text change
    /// including text ranges, affected editor tree
    /// and changed AST node.
    /// </summary>
    internal class TextChangeContext
    {
        public EditorTree EditorTree { get; private set; }

        /// <summary>
        /// Most recent change start in the previous snapshot
        /// </summary>
        public int OldStart { get; private set; }
        /// <summary>
        /// Most recent change length in the previous snapshot
        /// </summary>
        public int OldLength { get; private set; }

        /// <summary>
        /// Most recent change start in the new snapshot
        /// </summary>
        public int NewStart { get; private set; }

        /// <summary>
        /// Most recent change length in the new snapshot
        /// </summary>
        public int NewLength { get; private set; }

        /// <summary>
        /// Previous snapshot text
        /// </summary>
        public ITextProvider OldTextProvider { get; private set; }
        
        /// <summary>
        /// New snapshot text
        /// </summary>
        public ITextProvider NewTextProvider { get; private set; }

        /// <summary>
        /// Changes accumulated since last tree update
        /// </summary>
        public TextChange PendingChanges { get; private set; }

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

        public TextChangeContext(EditorTree editorTree, TextChangeEventArgs change, TextChange pendingChanges)
        {
            EditorTree = editorTree;
            NewStart = change.Start;
            OldStart = change.OldStart;
            OldLength = change.OldLength;
            NewLength = change.NewLength;

            OldTextProvider = change.OldText != null ? change.OldText : editorTree.AstRoot.TextProvider;
            NewTextProvider = change.NewText != null ? change.NewText : new TextProvider(editorTree.TextBuffer.CurrentSnapshot, partial: true);

            PendingChanges = pendingChanges;

            TextChange textChange = new TextChange();

            textChange.OldRange = this.OldRange;
            textChange.OldTextProvider = this.OldTextProvider;

            textChange.NewRange = this.NewRange;
            textChange.NewTextProvider = this.NewTextProvider;

            textChange.Version = this.NewTextProvider.Version;

            PendingChanges.Combine(textChange);
        }

        /// <summary>
        /// Range of changes in the previous snapshot
        /// </summary>
        public TextRange OldRange
        {
            get
            {
                if (_oldRange == null)
                    _oldRange = new TextRange(OldStart, OldLength);

                return _oldRange;
            }
        }

        /// <summary>
        /// Range of changes in the new snapshot
        /// </summary>
        public TextRange NewRange
        {
            get
            {
                if (_newRange == null)
                    _newRange = new TextRange(NewStart, NewLength);

                return _newRange;
            }
        }

        /// <summary>
        /// Changed text in the previous snapshot
        /// </summary>
        public string OldText
        {
            get
            {
                if (_oldText == null)
                    _oldText = OldTextProvider.GetText(OldRange);

                return _oldText;
            }
        }

        /// <summary>
        /// Changed text in the new snapshot
        /// </summary>
        public string NewText
        {
            get
            {
                if (_newText == null)
                    _newText = NewTextProvider.GetText(NewRange);

                return _newText;
            }
        }
    }
}

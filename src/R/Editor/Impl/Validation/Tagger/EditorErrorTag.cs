// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.TaskList.Definitions;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Tree.Definitions;
using Microsoft.R.Editor.Validation.Definitions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.R.Editor.Validation.Tagger {
    /// <summary>
    /// This represents an underlined syntax error in the editor
    /// </summary>
    internal class EditorErrorTag : ErrorTag, ITagSpan<IErrorTag>, IExpandableTextRange, IEditorTaskListItem {
        private ITextBuffer _textBuffer;
        private ITextRange _range;

        public EditorErrorTag(IEditorTree editorTree, IValidationError error)
            : base(GetErrorType(error), error.Message) {
            _textBuffer = editorTree.TextBuffer;

            Description = error.Message;
            TaskType = GetTaskType(error);

            _range = error;

            if (_range == null || _range.Start < 0)
                _range = TextRange.EmptyRange;
        }

        #region ITagSpan<IErrorTag> Members
        public IErrorTag Tag {
            get { return this; }
        }

        public SnapshotSpan Span {
            get {
                // Positions may be out of date: editor is asking about current snapshot
                // while tree can still be holding on the earlier one since tree snapshot
                // is updated when background parsing completes. However, tag positions
                // are constantly updated when text buffer changes and hence they match
                // current text buffer snapshot.

                var snapshot = _textBuffer.CurrentSnapshot;
                var start = Math.Max(0, Math.Min(_range.Start, snapshot.Length - 1));
                var end = Math.Max(0, Math.Min(_range.End, snapshot.Length));

                return new SnapshotSpan(snapshot, start, end - start);
            }
        }
        #endregion

        static string GetErrorType(IValidationError error) {
            string errorType = PredefinedErrorTypeNames.SyntaxError;

            switch (error.Severity) {
                case ErrorSeverity.Fatal:
                case ErrorSeverity.Error:
                    errorType = PredefinedErrorTypeNames.SyntaxError;
                    break;
                case ErrorSeverity.Warning:
                    errorType = PredefinedErrorTypeNames.Warning;
                    break;

                case ErrorSeverity.Informational:
                    errorType = PredefinedErrorTypeNames.OtherError;
                    break;
            }

            return errorType;
        }

        static TaskType GetTaskType(IValidationError error) {
            switch (error.Severity) {
                case ErrorSeverity.Fatal:
                case ErrorSeverity.Error:
                    return TaskType.Error;

                case ErrorSeverity.Warning:
                    return TaskType.Warning;

                case ErrorSeverity.Informational:
                    return TaskType.Informational;
            }

            return TaskType.Error;
        }

        #region ITextRange Members
        public int Start {
            get { return _range.Start; }
        }

        public int End {
            get { return _range.End; }
        }

        public int Length {
            get { return _range.Length; }
        }

        public bool Contains(int position) {
            return _range.Contains(position);
        }

        public void Shift(int offset) {
            _range.Shift(offset);
        }

        #endregion

        #region IExpandableTextRange
        public void Expand(int startOffset, int endOffset) {
            var expandable = _range as IExpandableTextRange;

            if (expandable != null)
                expandable.Expand(startOffset, endOffset);
        }

        public bool AllowZeroLength {
            get {
                return false;
            }
        }

        public bool IsStartInclusive {
            get {
                return true;
            }
        }

        public bool IsEndInclusive {
            get {
                return false;
            }
        }

        public bool ContainsUsingInclusion(int position) {
            return Contains(position);
        }

        public bool IsWellFormed {
            get { return true; }
        }
        #endregion

        #region IEditorTaskListItem
        public string Description { get; private set; }
        public TaskType TaskType { get; private set; }

        public int Line {
            get {
                if (Span.Start < Span.Snapshot.Length) {
                    // Add 1 for WebMatrix compatability, 
                    // remember to subtract 1 in VS-specific code
                    return Span.Snapshot.GetLineNumberFromPosition(Span.Start) + 1;
                }

                return 0;
            }
        }

        public int Column {
            get {
                if (Span.Start < Span.Snapshot.Length) {
                    var line = Span.Snapshot.GetLineFromPosition(Span.Start);
                    // Add 1 for WebMatrix compatability, 
                    // remember to subtract 1 in VS-specific code
                    return Span.Start.Position - line.Start + 1;
                }

                return 0;
            }
        }

        public string FileName {
            get {
                return _textBuffer.GetFilePath();
            }
        }

        public string HelpKeyword {
            get {
                return "vs.r.validationerror";
            }
        }
        #endregion
    }
}

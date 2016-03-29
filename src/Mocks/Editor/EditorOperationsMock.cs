// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class EditorOperationsMock : IEditorOperations {
        public EditorOperationsMock(ITextView textView) {
            TextView = textView;
            Options = new EditorOptionsMock();
        }

        public bool CanCut { get; } = true;
        public bool CanDelete { get; } = true;
        public bool CanPaste { get; } = true;

        public IEditorOptions Options { get; private set; }

        public ITrackingSpan ProvisionalCompositionSpan {
            get {
                throw new NotImplementedException();
            }
        }

        public string SelectedText {
            get {
                return TextView.Selection.StreamSelectionSpan.GetText();
            }
        }

        public ITextView TextView { get; private set; }

        public void AddAfterTextBufferChangePrimitive() {
            throw new NotImplementedException();
        }

        public void AddBeforeTextBufferChangePrimitive() {
            throw new NotImplementedException();
        }

        public bool Backspace() {
            throw new NotImplementedException();
        }

        public bool Capitalize() {
            throw new NotImplementedException();
        }

        public bool ConvertSpacesToTabs() {
            throw new NotImplementedException();
        }

        public bool ConvertTabsToSpaces() {
            throw new NotImplementedException();
        }

        public bool CopySelection() {
            throw new NotImplementedException();
        }

        public bool CutFullLine() {
            throw new NotImplementedException();
        }

        public bool CutSelection() {
            throw new NotImplementedException();
        }

        public bool DecreaseLineIndent() {
            throw new NotImplementedException();
        }

        public bool Delete() {
            throw new NotImplementedException();
        }

        public bool DeleteBlankLines() {
            throw new NotImplementedException();
        }

        public bool DeleteFullLine() {
            throw new NotImplementedException();
        }

        public bool DeleteHorizontalWhiteSpace() {
            throw new NotImplementedException();
        }

        public bool DeleteToBeginningOfLine() {
            throw new NotImplementedException();
        }

        public bool DeleteToEndOfLine() {
            throw new NotImplementedException();
        }

        public bool DeleteWordToLeft() {
            throw new NotImplementedException();
        }

        public bool DeleteWordToRight() {
            throw new NotImplementedException();
        }

        public void ExtendSelection(int newEnd) {
            throw new NotImplementedException();
        }

        public string GetWhitespaceForVirtualSpace(VirtualSnapshotPoint point) {
            throw new NotImplementedException();
        }

        public void GotoLine(int lineNumber) {
            throw new NotImplementedException();
        }

        public bool IncreaseLineIndent() {
            throw new NotImplementedException();
        }

        public bool Indent() {
            throw new NotImplementedException();
        }

        public bool InsertFile(string filePath) {
            throw new NotImplementedException();
        }

        public bool InsertNewLine() {
            throw new NotImplementedException();
        }

        public bool InsertProvisionalText(string text) {
            throw new NotImplementedException();
        }

        public bool InsertText(string text) {
            throw new NotImplementedException();
        }

        public bool InsertTextAsBox(string text, out VirtualSnapshotPoint boxStart, out VirtualSnapshotPoint boxEnd) {
            throw new NotImplementedException();
        }

        public bool MakeLowercase() {
            throw new NotImplementedException();
        }

        public bool MakeUppercase() {
            throw new NotImplementedException();
        }

        public void MoveCaret(ITextViewLine textLine, double horizontalOffset, bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveCurrentLineToBottom() {
            throw new NotImplementedException();
        }

        public void MoveCurrentLineToTop() {
            throw new NotImplementedException();
        }

        public void MoveLineDown(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveLineUp(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveToBottomOfView(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveToEndOfDocument(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveToEndOfLine(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveToHome(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveToLastNonWhiteSpaceCharacter(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveToNextCharacter(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveToNextWord(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveToPreviousCharacter(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveToPreviousWord(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveToStartOfDocument(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveToStartOfLine(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveToStartOfLineAfterWhiteSpace(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveToStartOfNextLineAfterWhiteSpace(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveToStartOfPreviousLineAfterWhiteSpace(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void MoveToTopOfView(bool extendSelection) {
            throw new NotImplementedException();
        }

        public bool NormalizeLineEndings(string replacement) {
            throw new NotImplementedException();
        }

        public bool OpenLineAbove() {
            throw new NotImplementedException();
        }

        public bool OpenLineBelow() {
            throw new NotImplementedException();
        }

        public void PageDown(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void PageUp(bool extendSelection) {
            throw new NotImplementedException();
        }

        public bool Paste() {
            throw new NotImplementedException();
        }

        public int ReplaceAllMatches(string searchText, string replaceText, bool matchCase, bool matchWholeWord, bool useRegularExpressions) {
            throw new NotImplementedException();
        }

        public bool ReplaceSelection(string text) {
            throw new NotImplementedException();
        }

        public bool ReplaceText(Span replaceSpan, string text) {
            throw new NotImplementedException();
        }

        public void ResetSelection() {
            throw new NotImplementedException();
        }

        public void ScrollColumnLeft() {
            throw new NotImplementedException();
        }

        public void ScrollColumnRight() {
            throw new NotImplementedException();
        }

        public void ScrollDownAndMoveCaretIfNecessary() {
            throw new NotImplementedException();
        }

        public void ScrollLineBottom() {
            throw new NotImplementedException();
        }

        public void ScrollLineCenter() {
            throw new NotImplementedException();
        }

        public void ScrollLineTop() {
            throw new NotImplementedException();
        }

        public void ScrollPageDown() {
            throw new NotImplementedException();
        }

        public void ScrollPageUp() {
            throw new NotImplementedException();
        }

        public void ScrollUpAndMoveCaretIfNecessary() {
            throw new NotImplementedException();
        }

        public void SelectAll() {
            throw new NotImplementedException();
        }

        public void SelectAndMoveCaret(VirtualSnapshotPoint anchorPoint, VirtualSnapshotPoint activePoint) {
            throw new NotImplementedException();
        }

        public void SelectAndMoveCaret(VirtualSnapshotPoint anchorPoint, VirtualSnapshotPoint activePoint, TextSelectionMode selectionMode) {
            throw new NotImplementedException();
        }

        public void SelectAndMoveCaret(VirtualSnapshotPoint anchorPoint, VirtualSnapshotPoint activePoint, TextSelectionMode selectionMode, EnsureSpanVisibleOptions? scrollOptions) {
            throw new NotImplementedException();
        }

        public void SelectCurrentWord() {
            throw new NotImplementedException();
        }

        public void SelectEnclosing() {
            throw new NotImplementedException();
        }

        public void SelectFirstChild() {
            throw new NotImplementedException();
        }

        public void SelectLine(ITextViewLine viewLine, bool extendSelection) {
            throw new NotImplementedException();
        }

        public void SelectNextSibling(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void SelectPreviousSibling(bool extendSelection) {
            throw new NotImplementedException();
        }

        public void SwapCaretAndAnchor() {
            throw new NotImplementedException();
        }

        public bool Tabify() {
            throw new NotImplementedException();
        }

        public bool ToggleCase() {
            throw new NotImplementedException();
        }

        public bool TransposeCharacter() {
            throw new NotImplementedException();
        }

        public bool TransposeLine() {
            throw new NotImplementedException();
        }

        public bool TransposeWord() {
            throw new NotImplementedException();
        }

        public bool Unindent() {
            throw new NotImplementedException();
        }

        public bool Untabify() {
            throw new NotImplementedException();
        }

        public void ZoomIn() {
            throw new NotImplementedException();
        }

        public void ZoomOut() {
            throw new NotImplementedException();
        }

        public void ZoomTo(double zoomLevel) {
            throw new NotImplementedException();
        }
    }
}

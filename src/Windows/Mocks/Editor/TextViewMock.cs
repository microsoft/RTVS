// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;
using NSubstitute;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public class TextViewMock : ITextView {

        public TextViewMock(ITextBuffer textBuffer) :
            this(textBuffer, 0) {
        }

        public TextViewMock(IEnumerable<ITextBuffer> textBuffers, int caretPosition): 
            this(textBuffers.First(), caretPosition) {
            BufferGraph = new BufferGraphMock(textBuffers);
        }

        public TextViewMock(ITextBuffer textBuffer, int caretPosition) {
            TextBuffer = textBuffer;

            TextDataModel = new TextDataModelMock(TextBuffer);
            TextViewModel = new TextViewModelMock(textBuffer);

            Caret = new TextCaretMock(this, caretPosition);
            Selection = new TextSelectionMock(this, new TextRange(caretPosition, 0));

            TextViewLines = new TextViewLineCollectionMock(TextBuffer);
            BufferGraph = new BufferGraphMock(TextBuffer);

            var view = new EditorView(this);
            Properties.AddProperty(typeof(IEditorView), view);
        }

        public IBufferGraph BufferGraph { get; }
        public ITextCaret Caret { get; set; }
        public bool HasAggregateFocus => true;
        public bool InLayout => false;
        public bool IsClosed => false;
        public bool IsMouseOverViewOrAdornments => true;
        public double LineHeight => 12;
        public double MaxTextRightCoordinate => 100;

        public IEditorOptions Options => Substitute.For<IEditorOptions>();
        public PropertyCollection Properties { get; } = new PropertyCollection();

        public ITrackingSpan ProvisionalTextHighlight {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public ITextViewRoleSet Roles => Substitute.For<ITextViewRoleSet>();

        public ITextSelection Selection { get; }

        public ITextBuffer TextBuffer { get; }

        public ITextDataModel TextDataModel { get; }

        public ITextSnapshot TextSnapshot => TextBuffer.CurrentSnapshot;

        public ITextViewLineCollection TextViewLines { get; }

        public ITextViewModel TextViewModel { get; }

        public double ViewportBottom => 100;
        public double ViewportHeight => 100;
        public double ViewportLeft { get; set; }
        public double ViewportRight { get; set; }
        public double ViewportTop => 0;
        public double ViewportWidth => 100;

        public IViewScroller ViewScroller => throw new NotImplementedException();

        public ITextSnapshot VisualSnapshot => this.TextSnapshot;

        public void Close() {
        }

        public void DisplayTextLineContainingBufferPosition(SnapshotPoint bufferPosition, double verticalDistance, ViewRelativePosition relativeTo) { }

        public void DisplayTextLineContainingBufferPosition(SnapshotPoint bufferPosition, double verticalDistance, ViewRelativePosition relativeTo, double? viewportWidthOverride, double? viewportHeightOverride) { }
        public SnapshotSpan GetTextElementSpan(SnapshotPoint point) => new SnapshotSpan(this.TextSnapshot, point, 0);
        public ITextViewLine GetTextViewLineContainingBufferPosition(SnapshotPoint bufferPosition) => throw new NotImplementedException();
        public void QueueSpaceReservationStackRefresh() { }

#pragma warning disable 0067
        public event EventHandler Closed;
        public event EventHandler GotAggregateFocus;
        public event EventHandler<TextViewLayoutChangedEventArgs> LayoutChanged;
        public event EventHandler LostAggregateFocus;
        public event EventHandler<MouseHoverEventArgs> MouseHover;
        public event EventHandler ViewportHeightChanged;
        public event EventHandler ViewportLeftChanged;
        public event EventHandler ViewportWidthChanged;
    }
}

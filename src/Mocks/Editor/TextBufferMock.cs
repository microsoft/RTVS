// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public class TextBufferMock : ITextBuffer {
        public TextBufferMock(string content, string contentTypeName) {
            ContentType = new ContentTypeMock(contentTypeName, new IContentType[] { ContentTypeMock.TextContentType });
            TextVersionMock initialVersion = new TextVersionMock(this, 0, content.Length);
            CurrentSnapshot = new TextSnapshotMock(content, this, initialVersion);
            EditorBuffer.Create(this, null);
        }
        public void Clear() => Replace(new Span(0, CurrentSnapshot.Length), string.Empty);

        #region ITextBuffer Members

        public void ChangeContentType(IContentType newContentType, object editTag) {
            var before = ContentType;
            ContentType = newContentType;
            ContentTypeChanged?.Invoke(this, new ContentTypeChangedEventArgs(CurrentSnapshot, CurrentSnapshot, before, newContentType, new object()));
        }

        public event EventHandler<TextContentChangedEventArgs> BeforeChanged; // unit tests only, for internal mock use

        public event EventHandler<TextContentChangedEventArgs> Changed;
        public event EventHandler<TextContentChangedEventArgs> ChangedHighPriority;
        public event EventHandler<TextContentChangedEventArgs> ChangedLowPriority;
        public event EventHandler<TextContentChangingEventArgs> Changing;
        public event EventHandler PostChanged;
        public event EventHandler<ContentTypeChangedEventArgs> ContentTypeChanged;

#pragma warning disable 0067
        public event EventHandler<SnapshotSpanEventArgs> ReadOnlyRegionsChanged;
#pragma warning restore 0067

        public bool CheckEditAccess() => true;
        public IContentType ContentType { get; private set; }
        public ITextEdit CreateEdit() => new TextEditMock(this);
        public ITextEdit CreateEdit(EditOptions options, int? reiteratedVersionNumber, object editTag) => new TextEditMock(this);
        public IReadOnlyRegionEdit CreateReadOnlyRegionEdit() => throw new NotImplementedException();

        public ITextSnapshot CurrentSnapshot { get; private set; }

        public ITextSnapshot Delete(Span deleteSpan) {
            var sb = new StringBuilder();
            var oldText = CurrentSnapshot.GetText(deleteSpan.Start, deleteSpan.Length);

            sb.Append(CurrentSnapshot.GetText(0, deleteSpan.Start));
            sb.Append(CurrentSnapshot.GetText(deleteSpan.End, CurrentSnapshot.Length - deleteSpan.End));

            TextChangeMock change = new TextChangeMock(deleteSpan.Start, deleteSpan.Length, oldText, String.Empty);
            TextSnapshotMock nextSnapshot = ((TextSnapshotMock)CurrentSnapshot).CreateNextSnapshot(sb.ToString(), change);

            ApplyChange(nextSnapshot);

            return CurrentSnapshot;
        }

        public bool EditInProgress => false;
        public NormalizedSpanCollection GetReadOnlyExtents(Span span) => new NormalizedSpanCollection(new Span(0, 0));

        public ITextSnapshot Insert(int position, string text) {
            var sb = new StringBuilder();

            sb.Append(CurrentSnapshot.GetText(0, position));
            sb.Append(text);
            sb.Append(CurrentSnapshot.GetText(position, CurrentSnapshot.Length - position));

            var change = new TextChangeMock(position, 0, String.Empty, text);
            var nextSnapshot = ((TextSnapshotMock)CurrentSnapshot).CreateNextSnapshot(sb.ToString(), change);

            ApplyChange(nextSnapshot);

            return CurrentSnapshot;
        }

        public bool IsReadOnly(Span span, bool isEdit) => false;
        public bool IsReadOnly(Span span) => false;
        public bool IsReadOnly(int position, bool isEdit) => false;
        public bool IsReadOnly(int position) => false;

        public ITextSnapshot Replace(Span replaceSpan, string replaceWith) {
            var sb = new StringBuilder();
            var oldText = CurrentSnapshot.GetText(replaceSpan);

            sb.Append(CurrentSnapshot.GetText(0, replaceSpan.Start));
            sb.Append(replaceWith);
            sb.Append(CurrentSnapshot.GetText(replaceSpan.End, CurrentSnapshot.Length - replaceSpan.End));

            var change = new TextChangeMock(replaceSpan.Start, replaceSpan.Length, oldText, replaceWith);

            var nextSnapshot = ((TextSnapshotMock)CurrentSnapshot).CreateNextSnapshot(sb.ToString(), change);
            ApplyChange(nextSnapshot);

            return CurrentSnapshot;
        }

        public void TakeThreadOwnership() {
        }

        #endregion

        #region IPropertyOwner Members
        public PropertyCollection Properties { get; private set; } = new PropertyCollection();
        #endregion

        private void ApplyChange(TextSnapshotMock snapshot) {
            Changing?.Invoke(this, new TextContentChangingEventArgs(CurrentSnapshot, new object(), CancelAction));

            var before = CurrentSnapshot;
            CurrentSnapshot = snapshot;

            var args = new TextContentChangedEventArgs(before, CurrentSnapshot, EditOptions.None, new object());

            BeforeChanged?.Invoke(this, args);
            ChangedHighPriority?.Invoke(this, args);
            Changed?.Invoke(this, args);
            ChangedLowPriority?.Invoke(this, args);
            PostChanged?.Invoke(this, EventArgs.Empty);
        }

        void CancelAction(TextContentChangingEventArgs e) { }
    }
}

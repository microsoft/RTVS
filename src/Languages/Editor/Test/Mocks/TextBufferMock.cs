using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Test.Mocks
{
    [ExcludeFromCodeCoverage]
    public class TextBufferMock : ITextBuffer
    {
        public TextBufferMock(string content, string contentTypeName)
        {
            Properties = new PropertyCollection();
            ContentType = new ContentTypeMock(contentTypeName, new IContentType[] { ContentTypeMock.TextContentType });
            TextVersionMock initialVersion = new TextVersionMock(this, 0, content.Length);
            CurrentSnapshot = new TextSnapshotMock(content, this, ContentType, initialVersion);
        }

        #region ITextBuffer Members

        public void ChangeContentType(IContentType newContentType, object editTag)
        {
            var before = ContentType;

            ContentType = newContentType;

            if (ContentTypeChanged != null)
                ContentTypeChanged(this, new ContentTypeChangedEventArgs(CurrentSnapshot, CurrentSnapshot, before, newContentType, new object()));
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

        public bool CheckEditAccess()
        {
            return true;
        }

        public IContentType ContentType { get; private set; }

        public ITextEdit CreateEdit()
        {
            return new TextEditMock(this);
        }

        public ITextEdit CreateEdit(EditOptions options, int? reiteratedVersionNumber, object editTag)
        {
            return new TextEditMock(this);
        }

        public IReadOnlyRegionEdit CreateReadOnlyRegionEdit()
        {
            throw new NotImplementedException();
        }

        public ITextSnapshot CurrentSnapshot { get; private set; }

        public ITextSnapshot Delete(Span deleteSpan)
        {
            var sb = new StringBuilder();
            var oldText = CurrentSnapshot.GetText(deleteSpan.Start, deleteSpan.Length);

            sb.Append(CurrentSnapshot.GetText(0, deleteSpan.Start));
            sb.Append(CurrentSnapshot.GetText(deleteSpan.End, CurrentSnapshot.Length - deleteSpan.End));

            TextChangeMock change = new TextChangeMock(deleteSpan.Start, deleteSpan.Length, oldText, String.Empty);
            TextSnapshotMock nextSnapshot = ((TextSnapshotMock)CurrentSnapshot).CreateNextSnapshot(sb.ToString(), change);

            ApplyChange(nextSnapshot);

            return CurrentSnapshot;
        }

        public bool EditInProgress
        {
            get { return false; }
        }

        public NormalizedSpanCollection GetReadOnlyExtents(Span span)
        {
            return new NormalizedSpanCollection(new Span(0, 0));
        }

        public ITextSnapshot Insert(int position, string text)
        {
            var sb = new StringBuilder();

            sb.Append(CurrentSnapshot.GetText(0, position));
            sb.Append(text);
            sb.Append(CurrentSnapshot.GetText(position, CurrentSnapshot.Length - position));

            TextChangeMock change = new TextChangeMock(position, 0, String.Empty, text);
            TextSnapshotMock nextSnapshot = ((TextSnapshotMock)CurrentSnapshot).CreateNextSnapshot(sb.ToString(), change);

            ApplyChange(nextSnapshot);

            return CurrentSnapshot;
        }

        public bool IsReadOnly(Span span, bool isEdit)
        {
            return false;
        }

        public bool IsReadOnly(Span span)
        {
            return false;
        }

        public bool IsReadOnly(int position, bool isEdit)
        {
            return false;
        }

        public bool IsReadOnly(int position)
        {
            return false;
        }

        public ITextSnapshot Replace(Span replaceSpan, string replaceWith)
        {
            var sb = new StringBuilder();
            var oldText = CurrentSnapshot.GetText(replaceSpan);

            sb.Append(CurrentSnapshot.GetText(0, replaceSpan.Start));
            sb.Append(replaceWith);
            sb.Append(CurrentSnapshot.GetText(replaceSpan.End, CurrentSnapshot.Length - replaceSpan.End));

            TextChangeMock change = new TextChangeMock(replaceSpan.Start, replaceSpan.Length, oldText, replaceWith);

            TextSnapshotMock nextSnapshot = ((TextSnapshotMock)CurrentSnapshot).CreateNextSnapshot(sb.ToString(), change);
            ApplyChange(nextSnapshot);

            return CurrentSnapshot;
        }

        public void TakeThreadOwnership()
        {
        }

        #endregion

        #region IPropertyOwner Members
        public PropertyCollection Properties { get; private set; }
        #endregion

        private void ApplyChange(TextSnapshotMock snapshot)
        {
            if (Changing != null)
                Changing(this, new TextContentChangingEventArgs(CurrentSnapshot, new object(), CancelAction));

            var before = CurrentSnapshot;
            CurrentSnapshot = snapshot;

            var args = new TextContentChangedEventArgs(before, CurrentSnapshot, EditOptions.None, new object());

            if (BeforeChanged != null)
                BeforeChanged(this, args);

            if (ChangedHighPriority != null)
                ChangedHighPriority(this, args);

            if (Changed != null)
                Changed(this, args);

            if (ChangedLowPriority != null)
                ChangedLowPriority(this, args);

            if (PostChanged != null)
                PostChanged(this, EventArgs.Empty);
        }

        void CancelAction(TextContentChangingEventArgs e) { }
    }
}

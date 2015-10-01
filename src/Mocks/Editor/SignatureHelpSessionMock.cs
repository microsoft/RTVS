using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Editor.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class SignatureHelpSessionMock : ISignatureHelpSession
    {
        public SignatureHelpSessionMock(ITextBuffer textBuffer, int caretPosition)
        {
            this.TextView = new TextViewMock(textBuffer, caretPosition);
        }

        public SnapshotPoint SnapshotPoint { get; set; }
        public ITrackingPoint TrackingPoint { get; set; }

        public bool IsDismissed { get; set; }

        public IIntellisensePresenter Presenter { get; set; }

        public PropertyCollection Properties { get; } = new PropertyCollection();

        public ISignature SelectedSignature { get; set; }

        public ReadOnlyObservableCollection<ISignature> Signatures { get; set; }

        public ITextView TextView { get; set; }

        public void Collapse() { }

        public void Dismiss() { }

        public SnapshotPoint? GetTriggerPoint(ITextSnapshot textSnapshot)
        {
            return SnapshotPoint;
        }

        public ITrackingPoint GetTriggerPoint(ITextBuffer textBuffer)
        {
            return TrackingPoint;
        }

        public bool Match()
        {
            return true;
        }

        public void Recalculate()
        {
        }

        public void Start()
        {
        }

#pragma warning disable 67
        public event EventHandler Dismissed;
        public event EventHandler PresenterChanged;
        public event EventHandler Recalculated;
        public event EventHandler<SelectedSignatureChangedEventArgs> SelectedSignatureChanged;
    }
}

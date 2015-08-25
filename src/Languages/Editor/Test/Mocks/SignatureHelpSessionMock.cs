using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Test.Mocks
{
    public sealed class SignatureHelpSessionMock : ISignatureHelpSession
    {
        public SnapshotPoint SnapshotPoint { get; set; }
        public ITrackingPoint TrackingPoint { get; set; }

        public bool IsDismissed { get; set; }

        public IIntellisensePresenter Presenter { get; set; }

        public PropertyCollection Properties { get; } = new PropertyCollection();

        public ISignature SelectedSignature { get; set; }

        public ReadOnlyObservableCollection<ISignature> Signatures { get; set; }

        public ITextView TextView { get; set; }

        public event EventHandler Dismissed;
        public event EventHandler PresenterChanged;
        public event EventHandler Recalculated;
        public event EventHandler<SelectedSignatureChangedEventArgs> SelectedSignatureChanged;

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
    }
}

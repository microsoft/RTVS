using System;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Test.Mocks
{
    public sealed class QuickInfoSessionMock : IQuickInfoSession
    {
        public SnapshotPoint TriggerPoint { get; set; }

        public ITrackingPoint TrackingPoint { get; set; }

        public ITrackingSpan ApplicableToSpan { get; set; }

        public bool IsDismissed { get; set; }

        public IIntellisensePresenter Presenter { get; set; }

        public PropertyCollection Properties { get; } = new PropertyCollection();

        public BulkObservableCollection<object> QuickInfoContent { get; set; }

        public ITextView TextView { get; set; }

        public bool TrackMouse { get; set; }

        public event EventHandler ApplicableToSpanChanged;
        public event EventHandler Dismissed;
        public event EventHandler PresenterChanged;
        public event EventHandler Recalculated;

        public void Collapse()
        {
        }

        public void Dismiss()
        {
        }

        public SnapshotPoint? GetTriggerPoint(ITextSnapshot textSnapshot)
        {
            return TriggerPoint;
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

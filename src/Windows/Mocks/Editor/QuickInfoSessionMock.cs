// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Editor.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class QuickInfoSessionMock : IQuickInfoSession
    {

        public QuickInfoSessionMock(ITextBuffer textBuffer, int position)
        {
            this.TextView = new TextViewMock(textBuffer, position);
        }

        public SnapshotPoint TriggerPoint { get; set; }

        public ITrackingPoint TrackingPoint { get; set; }

        public ITrackingSpan ApplicableToSpan { get; set; }

        public bool IsDismissed { get; set; }

        public IIntellisensePresenter Presenter { get; set; }

        public PropertyCollection Properties { get; } = new PropertyCollection();

        public BulkObservableCollection<object> QuickInfoContent { get; set; }

        public ITextView TextView { get; set; }

        public bool TrackMouse { get; set; }

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

#pragma warning disable 67
        public event EventHandler ApplicableToSpanChanged;
        public event EventHandler Dismissed;
        public event EventHandler PresenterChanged;
        public event EventHandler Recalculated;
    }
}

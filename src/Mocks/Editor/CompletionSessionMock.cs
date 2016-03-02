// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Editor.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class CompletionSessionMock : ICompletionSession
    {
        private IList<CompletionSet> _completionSets;
        private int _position;

        public CompletionSessionMock(ITextView textView, IList<CompletionSet> completionSets, int position)
        {
            TextView = textView;
            _completionSets = completionSets;
            _position = position;
        }

        public ReadOnlyObservableCollection<CompletionSet> CompletionSets
        {
            get { return new ReadOnlyObservableCollection<CompletionSet>(new ObservableCollection<CompletionSet>(_completionSets)); }
        }

        public bool IsDismissed
        {
            get { return false; }
        }

        public bool IsStarted
        {
            get { return true; }
        }

        public IIntellisensePresenter Presenter { get; set; }

        public PropertyCollection Properties { get; private set; } = new PropertyCollection();

        public CompletionSet SelectedCompletionSet { get; set; }

        public ITextView TextView { get; private set; }

        public void Collapse()
        {
        }

        public void Commit()
        {
        }

        public void Dismiss()
        {
        }

        public void Filter()
        {
        }

        public SnapshotPoint? GetTriggerPoint(ITextSnapshot textSnapshot)
        {
            return new SnapshotPoint(TextView.TextBuffer.CurrentSnapshot, _position);
        }

        public ITrackingPoint GetTriggerPoint(ITextBuffer textBuffer)
        {
            return new TrackingPointMock(TextView.TextBuffer, _position, PointTrackingMode.Positive, TrackingFidelityMode.Forward);
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
        public event EventHandler Committed;
        public event EventHandler Dismissed;
        public event EventHandler PresenterChanged;
        public event EventHandler Recalculated;
        public event EventHandler<ValueChangedEventArgs<CompletionSet>> SelectedCompletionSetChanged;
    }
}

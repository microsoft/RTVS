// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Editor.Mocks {
    public sealed class PeekSessionMock : IPeekSession {
        private int _triggerPoint;

        public PeekSessionMock(ITextView tv, int triggerPoint) {
            TextView = tv;
            _triggerPoint = triggerPoint;
        }

        public PeekSessionCreationOptions CreationOptions {
            get {
                throw new NotImplementedException();
            }
        }

        public bool IsDismissed => false;

        public ReadOnlyObservableCollection<IPeekableItem> PeekableItems {
            get {
                throw new NotImplementedException();
            }
        }

        public IIntellisensePresenter Presenter => null;

        public PropertyCollection Properties => new PropertyCollection();

        public string RelationshipName { get; set; }

        public ITextView TextView { get; }

        public void Collapse() { }

        public void Dismiss() { }

        public SnapshotPoint? GetTriggerPoint(ITextSnapshot textSnapshot) {
            return new SnapshotPoint(TextView.TextBuffer.CurrentSnapshot, _triggerPoint);
        }

        public ITrackingPoint GetTriggerPoint(ITextBuffer textBuffer) {
            throw new NotImplementedException();
        }

        public bool Match() => true;

        public IPeekResultQuery QueryPeekResults(IPeekableItem peekableItem, string relationshipName) {
            throw new NotImplementedException();
        }

        public void Recalculate() { }

        public void Start() { }

        public void TriggerNestedPeekSession(IPeekSession nestedSession) {
            throw new NotImplementedException();
        }

#pragma warning disable 67
        public event EventHandler Dismissed;
        public event EventHandler<NestedPeekTriggeredEventArgs> NestedPeekTriggered;
        public event EventHandler PresenterChanged;
        public event EventHandler Recalculated;
    }
}

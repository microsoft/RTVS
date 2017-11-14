// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class SignatureHelpSessionMock : ISignatureHelpSession {
        public SignatureHelpSessionMock(IServiceContainer services, ITextBuffer textBuffer, int caretPosition) {
            TextView = new TextViewMock(textBuffer, caretPosition);
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

        public SnapshotPoint? GetTriggerPoint(ITextSnapshot textSnapshot) => SnapshotPoint;

        public ITrackingPoint GetTriggerPoint(ITextBuffer textBuffer) => TrackingPoint;

        public bool Match() => true;

        public void Recalculate() { }

        public void Start() { }

#pragma warning disable 67
        public event EventHandler Dismissed;
        public event EventHandler PresenterChanged;
        public event EventHandler Recalculated;
        public event EventHandler<SelectedSignatureChangedEventArgs> SelectedSignatureChanged;
    }
}

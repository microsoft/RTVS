// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.QuickInfo {
    internal sealed class QuickInfoController : IIntellisenseController {
        private readonly IList<ITextBuffer> _subjectBuffers;
        private readonly IQuickInfoBroker _quickInfoBroker;
        private readonly ISignatureHelpBroker _signatureHelpBroker;
        private ITextView _textView;

        public QuickInfoController(ITextView textView, IList<ITextBuffer> subjectBuffers, IServiceContainer services) {
            _quickInfoBroker = services.GetService<IQuickInfoBroker>();
            _signatureHelpBroker = services.GetService<ISignatureHelpBroker>();
            _textView = textView;
            _subjectBuffers = subjectBuffers;

            _textView.MouseHover += OnViewMouseHover;
            _textView.TextBuffer.Changing += OnTextBufferChanging;
        }

        private void OnTextBufferChanging(object sender, TextContentChangingEventArgs e) {
            if (_quickInfoBroker.IsQuickInfoActive(_textView)) {
                var sessions = _quickInfoBroker.GetSessions(_textView);
                foreach (var session in sessions) {
                    session.Dismiss();
                }
            }
        }

        private void OnViewMouseHover(object sender, MouseHoverEventArgs e) {
            //find the mouse position by mapping down to the subject buffer
            var point = _textView.BufferGraph.MapDownToFirstMatch
                 (new SnapshotPoint(_textView.TextSnapshot, e.Position),
                PointTrackingMode.Positive,
                snapshot => _subjectBuffers.Contains(snapshot.TextBuffer),
                PositionAffinity.Predecessor);

            if (point != null) {
                var triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position, PointTrackingMode.Positive);
                if (!_quickInfoBroker.IsQuickInfoActive(_textView) && !_signatureHelpBroker.IsSignatureHelpActive(_textView)) {
                    _quickInfoBroker.TriggerQuickInfo(_textView, triggerPoint, true);
                }
            }
        }

        #region IIntellisenseController

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer) { }
        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer) { }

        public void Detach(ITextView textView) {
            if (textView == _textView) {
                textView.RemoveService(this);
                _textView.TextBuffer.Changing -= OnTextBufferChanging;
                _textView.MouseHover -= OnViewMouseHover;
                _textView = null;
            }
        }
        #endregion
    }
}

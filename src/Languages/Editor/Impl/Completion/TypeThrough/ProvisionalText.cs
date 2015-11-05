using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Completion.TypeThrough {

    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class ProvisionalTextHighlightFactory : IWpfTextViewCreationListener {
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("ProvisionalTextHighlight")]
        [Order(After = PredefinedAdornmentLayers.Outlining)]
        [Order(Before = PredefinedAdornmentLayers.CurrentLineHighlighter)]
        [TextViewRole(PredefinedTextViewRoles.Document)]
        public AdornmentLayerDefinition EditorAdornmentLayer { get; set; }

        public void TextViewCreated(IWpfTextView textView) { }
    }

    public class ProvisionalText {
        public static bool IgnoreChange { get; set; }

        public event EventHandler<EventArgs> Overtyping;
        public event EventHandler<EventArgs> Closing;

        public char ProvisionalChar { get; private set; }
        public ITrackingSpan TrackingSpan { get; private set; }

        private ITextView _textView;
        private IAdornmentLayer _layer;
        private Path _highlightAdornment;
        private Brush _highlightBrush;
        private ITrackingSpan _overtypeSpan;
        private bool _delete;
        private bool _adornmentRemoved;
        private bool _removingAdornment;
        private IProjectionBuffer _projectionBuffer;

        public ProvisionalText(ITextView textView, Span textSpan) {
            IgnoreChange = false;

            _textView = textView;

            var wpfTextView = _textView as IWpfTextView;
            _layer = wpfTextView.GetAdornmentLayer("ProvisionalTextHighlight");

            var textBuffer = _textView.TextBuffer;
            var snapshot = textBuffer.CurrentSnapshot;
            var provisionalCharSpan = new Span(textSpan.End - 1, 1);

            TrackingSpan = snapshot.CreateTrackingSpan(textSpan, SpanTrackingMode.EdgeExclusive);
            _textView.Caret.PositionChanged += OnCaretPositionChanged;

            textBuffer.Changed += OnTextBufferChanged;
            textBuffer.PostChanged += OnPostChanged;

            _projectionBuffer = _textView.TextBuffer as IProjectionBuffer;
            if (_projectionBuffer != null) {
                _projectionBuffer.SourceSpansChanged += OnSourceSpansChanged;
            }

            Color highlightColor = SystemColors.HighlightColor;
            Color baseColor = Color.FromArgb(96, highlightColor.R, highlightColor.G, highlightColor.B);
            _highlightBrush = new SolidColorBrush(baseColor);

            ProvisionalChar = snapshot[provisionalCharSpan.Start];
            HighlightSpan(provisionalCharSpan.Start);
        }

        public Span CurrentSpan {
            get {
                return TrackingSpan.GetSpan(_textView.TextBuffer.CurrentSnapshot);
            }
        }

        private void EndTracking() {
            if (_textView != null) {
                if (Closing != null)
                    Closing(this, EventArgs.Empty);

                ClearHighlight();

                if (_projectionBuffer != null) {
                    _projectionBuffer.SourceSpansChanged -= OnSourceSpansChanged;
                    _projectionBuffer = null;
                }

                if (_adornmentRemoved)
                    _adornmentRemoved = false;

                _layer = null;
                _overtypeSpan = null;

                _textView.TextBuffer.Changed -= OnTextBufferChanged;
                _textView.TextBuffer.PostChanged -= OnPostChanged;

                _textView.Caret.PositionChanged -= OnCaretPositionChanged;
                _textView = null;

                TrackingSpan = null;
            }
        }

        public bool IsPositionInSpan(int position) {
            if (_textView != null) {
                if (CurrentSpan.Contains(position) && position > CurrentSpan.Start)
                    return true;
            }

            return false;
        }

        private void OnSourceSpansChanged(object sender, ProjectionSourceSpansChangedEventArgs e) {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => ResoreHighlight()));
        }

        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e) {
            if (_textView != null) {
                // If caret moves outside of the text tracking span, consider text final
                var position = _textView.Caret.Position.BufferPosition;

                if (!CurrentSpan.Contains(position) || position == CurrentSpan.Start) {
                    EndTracking();
                }
            }
        }

        private void OnPostChanged(object sender, EventArgs e) {
            if (_textView != null && !IgnoreChange) {
                if (_overtypeSpan != null || _delete) {
                    // We must dismiss any existing intellisense since we are moving outside
                    // of the current context. For example, if we are in style="" and overtyping
                    // closing quote, we need to dismiss CSS intellisense.

                    var completionBroker = EditorShell.Current.ExportProvider.GetExport<ICompletionBroker>().Value;
                    var signatureBroker = EditorShell.Current.ExportProvider.GetExport<ISignatureHelpBroker>().Value;

                    completionBroker.DismissAllSessions(_textView);
                    signatureBroker.DismissAllSessions(_textView);

                    if (_overtypeSpan != null) {
                        ProvisionalText.IgnoreChange = true;
                        _textView.TextBuffer.Replace(_overtypeSpan.GetSpan(_textView.TextBuffer.CurrentSnapshot), String.Empty);
                        ProvisionalText.IgnoreChange = false;

                        // move the caret to the end of the provisional span, which may have moved further away.
                        SnapshotPoint moveToPosition = _overtypeSpan.GetEndPoint(_textView.TextBuffer.CurrentSnapshot);
                        _textView.Caret.MoveTo(moveToPosition);
                    } else {
                        // _delete the provisional text.  Caret doesn't need to be moved.
                        Span deleteSpan = new Span(CurrentSpan.End - 1, 1);
                        _textView.TextBuffer.Replace(deleteSpan, String.Empty);
                    }

                    // We need to notify last, as the HTML quote completion needs to remain be suppressed 
                    // after this operation completes
                    if (Overtyping != null)
                        Overtyping(this, EventArgs.Empty);

                    EndTracking();
                } else {
                    HighlightSpan(CurrentSpan.End - 1);
                }
            }
        }

        /// <summary>
        /// Sees if there is one non-whitespace character in the span, and returns it.
        /// Returns zero otherwise.
        /// </summary>
        public static char GetOneTypedCharacter(ITextSnapshot snapshot, Span span) {
            char ch = '\0';

            for (int i = span.Start; i < span.Start + span.Length; i++) {
                char curChar = snapshot[i];
                if (!char.IsWhiteSpace(curChar)) {
                    if (ch == '\0') {
                        ch = curChar;
                    } else {
                        // there are two non-whitespace chars
                        return '\0';
                    }
                }
            }

            return ch;
        }

        private static bool IsOnlyWhiteSpace(SnapshotSpan span) {
            for (int i = span.Start; i < span.Start + span.Length; i++) {
                char ch = span.Snapshot[i];
                if (!char.IsWhiteSpace(ch)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The span only covers the area between the character that was typed and the
        /// existing closing character. If this function returns true, then the span
        /// and the character after it will be deleted.
        /// </summary>
        protected virtual bool CanOvertype(SnapshotSpan span) {
            return IsOnlyWhiteSpace(span);
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs args) {
            // Zero changes typically means secondary buffer regeneration
            if (args.Changes.Count == 0) {
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => ResoreHighlight()));
            } else if (_textView != null && !IgnoreChange) {
                bool keepTracking = false;
                ITextSnapshot snapshot = _textView.TextBuffer.CurrentSnapshot;

                // If there is a change outside text span or change over provisional
                // text, we are done here: commit provisional text and disconnect.

                if (CurrentSpan.Length > 0 && args.Changes.Count == 1 && CurrentSpan.Contains(args.Changes[0].NewSpan)) {
                    ITextChange change = args.Changes[0];

                    // Check provisional text overtype
                    int delta = change.NewLength - change.OldLength;
                    if (delta > 0) {
                        Span newSpan = change.NewSpan;
                        if (change.NewText.StartsWith(change.OldText, StringComparison.Ordinal)) {
                            // Dev12 706739: C# commit doesn't have an oldLength of zero, but rather the applicable span's length.
                            newSpan = new Span(newSpan.Start + change.OldLength, newSpan.Length - change.OldLength);
                        }

                        char ch = GetOneTypedCharacter(_textView.TextBuffer.CurrentSnapshot, newSpan);

                        if (ch == ProvisionalChar) {
                            SnapshotSpan spanToEnd = new SnapshotSpan(snapshot, newSpan.End, CurrentSpan.End - newSpan.End - 1);

                            if (CanOvertype(spanToEnd)) {
                                // Dev12 bug 673486 - The span must include extra whitespace that was added by autoformatting
                                _overtypeSpan = snapshot.CreateTrackingSpan(
                                    new Span(newSpan.End, CurrentSpan.End - newSpan.End),
                                    SpanTrackingMode.EdgeExclusive);
                            }
                        }
                    } else if (delta < 0 && change.OldPosition == CurrentSpan.Start) {
                        // Deleting open quote or brace should also delete provisional character
                        _delete = true;
                    }

                    keepTracking = true;
                } else if (CurrentSpan.Length > 0 && args.Changes.Count == 1 && CurrentSpan.End == args.Changes[0].NewSpan.Start) {
                    // Extending span such as when autoformatting inserts whitespace before the provisional text.
                    // In this case we need to include said whitespace into the tracked span.
                    ITextChange change = args.Changes[0];
                    if (change.OldText.TrimStart() == change.NewText.TrimStart()) {
                        TrackingSpan = snapshot.CreateTrackingSpan(Span.FromBounds(CurrentSpan.Start, change.NewSpan.End), SpanTrackingMode.EdgeExclusive);
                        keepTracking = true;
                    }
                } else if (CurrentSpan.Length > 0 && args.Changes.Count > 1 && _textView.Properties.ContainsProperty("InFormatting")) {
                    // Autoformatting can cause multiple simultaneous changes, 
                    // but don't allow them to end tracking. Autoformat start
                    // at the beginning of the previous line.
                    keepTracking = true;
                }

                if (!keepTracking) {
                    EndTracking();
                }
            }
        }

        private void ResoreHighlight() {
            if (_textView != null && _adornmentRemoved) {
                HighlightSpan(CurrentSpan.End - 1);
            }

            _adornmentRemoved = false;
        }

        private void HighlightSpan(int bufferPosition) {
            ClearHighlight();

            var wpfTextView = _textView as IWpfTextView;
            var snapshotSpan = new SnapshotSpan(wpfTextView.TextBuffer.CurrentSnapshot, new Span(bufferPosition, 1));

            Geometry highlightGeometry = wpfTextView.TextViewLines.GetTextMarkerGeometry(snapshotSpan);
            if (highlightGeometry != null) {
                _highlightAdornment = new Path();
                _highlightAdornment.Data = highlightGeometry;
                _highlightAdornment.Fill = _highlightBrush;
            }

            if (_highlightAdornment != null) {
                _layer.AddAdornment(
                    AdornmentPositioningBehavior.TextRelative, snapshotSpan,
                    this, _highlightAdornment, new AdornmentRemovedCallback(OnAdornmentRemoved));
            }
        }

        private void OnAdornmentRemoved(object tag, UIElement element) {
            if (_removingAdornment)
                return;

            _adornmentRemoved = true;
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => ResoreHighlight()));
        }

        private void ClearHighlight() {
            if (_highlightAdornment != null) {
                _removingAdornment = true;

                _layer.RemoveAdornment(_highlightAdornment);
                _highlightAdornment = null;

                _removingAdornment = false;
            }
        }
    }
}

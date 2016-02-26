using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Selection;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.R.Editor.Commands {
    public sealed class RMouseProcessor : MouseProcessorBase {
        private IWpfTextView _wpfTextView;
        public RMouseProcessor(IWpfTextView wpfTextView) {
            _wpfTextView = wpfTextView;
        }

        public override void PreprocessMouseLeftButtonDown(MouseButtonEventArgs e) {
            if ((e.ClickCount == 1 && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)) ||
                 e.ClickCount == 2) {

                // Check if token is URL. If it is, don't try and select and instead
                // let core editor deal with it since it may open URL on Ctrl+Click.
                if (!IsOverHotUrl(_wpfTextView, e)) {
                    // If this is a Ctrl+Click or double-click then post the select word command.
                    var command = new SelectWordCommand(_wpfTextView, _wpfTextView.TextBuffer);
                    var o = new object();
                    var result = command.Invoke(typeof(VSConstants.VSStd2KCmdID).GUID, (int)VSConstants.VSStd2KCmdID.SELECTCURRENTWORD, null, ref o);
                    if (result.Result == CommandResult.Executed.Result) {
                        e.Handled = true;
                        return;
                    }
                }
            }
            base.PreprocessMouseLeftButtonDown(e);
        }

        private bool IsOverHotUrl(ITextView textView, MouseButtonEventArgs e) {
            Point pt = e.GetPosition(_wpfTextView.VisualElement);
            ITextViewLine viewLine = _wpfTextView.TextViewLines.GetTextViewLineContainingYCoordinate(pt.Y);
            if (viewLine != null) {
                SnapshotPoint? bufferPosition = viewLine.GetBufferPositionFromXCoordinate(pt.X);
                if (bufferPosition.HasValue) {
                    var snapshot = textView.TextBuffer.CurrentSnapshot;
                    ITextSnapshotLine line = snapshot.GetLineFromPosition(bufferPosition.Value);

                    var tagAggregator = EditorShell.Current.ExportProvider.GetExportedValue<IViewTagAggregatorFactoryService>();
                    using (var urlClassificationAggregator = tagAggregator.CreateTagAggregator<IUrlTag>(textView)) {

                        var tags = urlClassificationAggregator.GetTags(new SnapshotSpan(snapshot, line.Start, line.Length));
                        return tags.Any(t => {
                            SnapshotPoint? start = t.Span.Start.GetPoint(textView.TextBuffer, PositionAffinity.Successor);
                            SnapshotPoint? end = t.Span.End.GetPoint(textView.TextBuffer, PositionAffinity.Successor);

                            return start.HasValue && end.HasValue &&
                                   start.Value <= bufferPosition.Value &&
                                   bufferPosition.Value < end.Value;
                        });
                    }
                }
            }
            return false;
        }
    }
}

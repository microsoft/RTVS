// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.R.Components.Controller;
using Microsoft.R.Editor.Selection;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.R.Editor.Commands {
    public sealed class RMouseProcessor : MouseProcessorBase {
        private readonly IWpfTextView _wpfTextView;
        private readonly ICoreShell _shell;

        public RMouseProcessor(IWpfTextView wpfTextView, ICoreShell shell) {
            _wpfTextView = wpfTextView;
            _shell = shell;
        }

        public override void PreprocessMouseLeftButtonDown(MouseButtonEventArgs e) {
            if ((e.ClickCount == 1 && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)) ||
                 e.ClickCount == 2) {

                var url = GetHotUrl(_wpfTextView, e);
                if (!string.IsNullOrEmpty(url)) {
                    _shell.Services.GetService<IProcessServices>().Start(url);
                    return;
                }
                // If this is a Ctrl+Click or double-click then post the select word command.
                var command = new SelectWordCommand(_wpfTextView, _wpfTextView.TextBuffer);
                var o = new object();
                var result = command.Invoke(typeof(VSConstants.VSStd2KCmdID).GUID, (int)VSConstants.VSStd2KCmdID.SELECTCURRENTWORD, null, ref o);
                if (result.Result == CommandResult.Executed.Result) {
                    e.Handled = true;
                    return;
                }
            }
            base.PreprocessMouseLeftButtonDown(e);
        }

        private string GetHotUrl(ITextView textView, MouseButtonEventArgs e) {
            Point pt = e.GetPosition(_wpfTextView.VisualElement);
            ITextViewLine viewLine = _wpfTextView.TextViewLines.GetTextViewLineContainingYCoordinate(pt.Y + _wpfTextView.ViewportTop);
            if (viewLine != null) {
                SnapshotPoint? bufferPosition = viewLine.GetBufferPositionFromXCoordinate(pt.X);
                if (bufferPosition.HasValue) {
                    var snapshot = textView.TextBuffer.CurrentSnapshot;
                    ITextSnapshotLine line = snapshot.GetLineFromPosition(bufferPosition.Value);

                    var tagAggregator = _shell.GetService<IViewTagAggregatorFactoryService>();
                    using (var urlClassificationAggregator = tagAggregator.CreateTagAggregator<IUrlTag>(textView)) {

                        var tags = urlClassificationAggregator.GetTags(new SnapshotSpan(snapshot, line.Start, line.Length));
                        return tags.Select(t => {
                            SnapshotPoint? start = t.Span.Start.GetPoint(textView.TextBuffer, PositionAffinity.Successor);
                            SnapshotPoint? end = t.Span.End.GetPoint(textView.TextBuffer, PositionAffinity.Successor);

                            if (start.HasValue && end.HasValue && start.Value <= bufferPosition.Value && bufferPosition.Value < end.Value) {
                                return textView.TextBuffer.CurrentSnapshot.GetText(Span.FromBounds(start.Value.Position, end.Value.Position));
                            }
                            return null;
                        }).FirstOrDefault(x => x != null);
                    }
                }
            }
            return null;
        }
    }
}

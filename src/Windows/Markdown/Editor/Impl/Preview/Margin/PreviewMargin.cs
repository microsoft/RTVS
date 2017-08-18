// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Common.Core.Idle;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.Preview.Browser;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Preview.Margin {
    // Based on https://github.com/madskristensen/MarkdownEditor/blob/master/src/Margin/BrowserMargin.cs
    public sealed class PreviewMargin : Border, IWpfTextViewMargin, IMarkdownPreview {
        private static object _idleActionTag;

        private readonly IIdleTimeService _idleTime;
        private readonly ITextView _textView;
        private readonly IRMarkdownEditorSettings _settings;
        private readonly MarginControls _marginControls;

        private bool _textChanged;
        private int _lastLineNumber;
        private int _lastBrowserLineNumber;
        private Task _browserUpdateTask;
        private bool _ignoreLayoutChange;

        public BrowserView Browser { get; }

        public PreviewMargin(ITextView textView, IServiceContainer services) {
            _idleActionTag = _idleActionTag ?? GetType();

            _textView = textView;
            _settings = services.GetService<IRMarkdownEditorSettings>();
            _idleTime = services.GetService<IIdleTimeService>();

            Browser = new BrowserView(_textView.TextBuffer.GetFileName(), services);
            Browser.ViewportChange += OnBrowserViewportChange;

            _marginControls = new MarginControls(this, textView, Browser.Control, _settings);

            UpdateBrowser();

            _textView.TextBuffer.Changed += OnTextBufferChanged;
            _textView.Caret.PositionChanged += OnCaretPositionChanged;
            _textView.LayoutChanged += OnLayoutChanged;

            textView.AddService(this);
        }

        private void OnBrowserViewportChange(object sender, EventArgs e) {
            if (_settings.ScrollEditorWithPreview) {
                var browserLineNum = Browser.GetFirstVisibleLineNumber();
                if (browserLineNum >= 0 && _lastBrowserLineNumber != browserLineNum) {
                    // Scroll matching text line into view
                    _lastBrowserLineNumber = browserLineNum;
                    var line = _textView.TextSnapshot.GetLineFromLineNumber(browserLineNum);
                    var firstVisibleLineNumber = _textView.TextSnapshot.GetLineNumberFromPosition(_textView.TextViewLines.FirstVisibleLine.Start);
                    var delta = line.LineNumber - firstVisibleLineNumber;
                    if (delta != 0) {
                        _ignoreLayoutChange = true;
                        var scrollDirection = delta < 0 ? ScrollDirection.Up : ScrollDirection.Down;
                        _textView.ViewScroller.ScrollViewportVerticallyByLines(scrollDirection, Math.Abs(delta));
                    }
                }
            }
        }

        public void Dispose() {
            Browser.ViewportChange -= OnBrowserViewportChange;
            Browser.Dispose();

            _textView.TextBuffer.Changed -= OnTextBufferChanged;
            _textView.Caret.PositionChanged -= OnCaretPositionChanged;
            _textView.RemoveService(this);
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e) {
            if (!_ignoreLayoutChange) {
                UpdateBrowserScrollPosition();
            }
            _ignoreLayoutChange = false;
        }

        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e) {
            UpdateBrowserScrollPosition();

            if (!_textChanged) {
                return;
            }
            var snapshot = _textView.TextBuffer.CurrentSnapshot;
            if (_textView.IsCaretInRCode()) {
                // In R Code. only update if caret line changes
                var caretPosition = _textView.Caret.Position.BufferPosition;
                var currentLineNumber = snapshot.GetLineNumberFromPosition(caretPosition);
                if (currentLineNumber == _lastLineNumber) {
                    return;
                }
                _lastLineNumber = currentLineNumber;
            } else if (_lastLineNumber < snapshot.LineCount) {
                // Did the caret move out?
                if (!_textView.IsPositionInRCode(snapshot.GetLineFromLineNumber(_lastLineNumber).Start)) {
                    // No, it did not, do nothing
                    return;
                }
            }

            UpdateOnIdle();
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) {
            // Update as-you-type outside of code blocks only.
            // Code blocks are updated on caret line change
            if (!_textView.IsCaretInRCode()) {
                UpdateOnIdle();
            }
            _textChanged = true;
        }

        private void UpdateBrowserScrollPosition() {
            if (!_settings.ScrollPreviewWithEditor) {
                return;
            }

            IdleTimeAction.Create(() => {
                var lineNumber = _textView.TextSnapshot.GetLineNumberFromPosition(_textView.TextViewLines.FirstVisibleLine.Start);
                Browser.UpdatePosition(lineNumber);
            }, 5, GetType(), _idleTime);
        }

        private void UpdateOnIdle() {
            IdleTimeAction.Cancel(_idleActionTag);
            IdleTimeAction.Create(() => Update(force: true), 0, _idleActionTag, _idleTime);
        }

        #region IMarkdownPreview
        public void Reload() => Browser.Reload(_textView.TextBuffer.CurrentSnapshot);

        public void Update(bool force) {
            if (_textChanged || force) {
                UpdateBrowser();
            }
        }

        public Task RunCurrentChunkAsync() {
            var index = _textView.GetCurrentRCodeBlockNumber();
            return index.HasValue ? Browser.UpdateBlocksAsync(_textView.TextBuffer.CurrentSnapshot, index.Value, 1) : Task.CompletedTask;
        }

        public Task RunAllChunksAboveAsync() {
            var index = _textView.GetCurrentRCodeBlockNumber();
            return index.HasValue ? Browser.UpdateBlocksAsync(_textView.TextBuffer.CurrentSnapshot, 0, index.Value) : Task.CompletedTask;
        }
        #endregion

        private void UpdateBrowser() {
            if (_browserUpdateTask == null) {
                _textChanged = false;
                _browserUpdateTask = Browser
                    .UpdateBrowserAsync(_textView.TextBuffer.CurrentSnapshot)
                    .ContinueWith(t => _browserUpdateTask = null);
            } else {
                // Still running, try again later
                IdleTimeAction.Create(() => Update(true), 500, _idleActionTag, _idleTime);
            }
        }

        #region IWpfTextViewMargin
        public ITextViewMargin GetTextViewMargin(string marginName) => this;
        public bool Enabled => true;
        public double MarginSize => 500;
        public FrameworkElement VisualElement => this;
        #endregion
    }
}

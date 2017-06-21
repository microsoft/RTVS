// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        private bool _textChanged;
        private int _lastLineNumber;
        private Task _browserUpdateTask;

        public BrowserView Browser { get; }

        public PreviewMargin(ITextView textView, IServiceContainer services) {
            _idleActionTag = _idleActionTag ?? GetType();

            _textView = textView;
            _settings = services.GetService<IRMarkdownEditorSettings>();

            Browser = new BrowserView(_textView.TextBuffer.GetFileName(), services);

            if (_settings.PreviewPosition == RMarkdownPreviewPosition.Below) {
                CreateBottomMarginControls();
            } else {
                CreateRightMarginControls();
            }

            UpdateBrowser();

            // TODO: separate R code changes from markdown changes
            _idleTime = services.GetService<IIdleTimeService>();
            _textView.TextBuffer.Changed += OnTextBufferChanged;
            _textView.Caret.PositionChanged += OnCaretPositionChanged;

            textView.AddService(this);
        }

        public void Dispose() {
            Browser?.Dispose();
            _textView?.RemoveService(this);
        }

        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e) {
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

        private void UpdateOnIdle() {
            IdleTimeAction.Cancel(_idleActionTag);
            IdleTimeAction.Create(Update, 0, _idleActionTag, _idleTime);
        }

        #region IMarkdownPreview
        public void Update() => UpdateBrowser();
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
                IdleTimeAction.Create(Update, 500, _idleActionTag, _idleTime);
            }
        }

        #region IWpfTextViewMargin
        public ITextViewMargin GetTextViewMargin(string marginName) => this;
        public bool Enabled => true;
        public double MarginSize => 500;
        public FrameworkElement VisualElement => this;
        #endregion


        private void CreateRightMarginControls() {
            var width = _settings.PreviewWidth;

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(width, GridUnitType.Pixel), MinWidth = 150 });
            grid.RowDefinitions.Add(new RowDefinition());
            Child = grid;

            grid.Children.Add(Browser.Control);
            Grid.SetColumn(Browser.Control, 2);
            Grid.SetRow(Browser.Control, 0);

            var splitter = new GridSplitter {
                Width = 5,
                ResizeDirection = GridResizeDirection.Columns,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            splitter.DragCompleted += RightDragCompleted;

            grid.Children.Add(splitter);
            Grid.SetColumn(splitter, 1);
            Grid.SetRow(splitter, 0);

            var fixWidth = new Action(() => {
                // previewWindow maxWidth = current total width - textView minWidth
                var newWidth = (_textView.ViewportWidth + grid.ActualWidth) - 150;

                // preveiwWindow maxWidth < previewWindow minWidth
                if (newWidth < 150) {
                    // Call 'get before 'set for performance
                    if (grid.ColumnDefinitions[2].MinWidth > 0) {
                        grid.ColumnDefinitions[2].MinWidth = 0;
                        grid.ColumnDefinitions[2].MaxWidth = 0;
                    }
                } else {
                    grid.ColumnDefinitions[2].MaxWidth = newWidth;
                    // Call 'get before 'set for performance
                    if (grid.ColumnDefinitions[2].MinWidth > 0)
                        grid.ColumnDefinitions[2].MinWidth = 150;
                }
            });

            // Listen sizeChanged event of both marginGrid and textView
            grid.SizeChanged += (e, s) => fixWidth();
            _textView.ViewportWidthChanged += (e, s) => fixWidth();
        }

        private void CreateBottomMarginControls() {
            var height = _settings.PreviewHeight;

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5, GridUnitType.Pixel) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(height, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.Children.Add(Browser.Control);
            Child = grid;

            Grid.SetColumn(Browser.Control, 0);
            Grid.SetRow(Browser.Control, 2);

            var splitter = new GridSplitter {
                Height = 5,
                ResizeDirection = GridResizeDirection.Rows,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            splitter.DragCompleted += BottomDragCompleted;

            grid.Children.Add(splitter);
            Grid.SetColumn(splitter, 0);
            Grid.SetRow(splitter, 1);
        }

        private void RightDragCompleted(object sender, DragCompletedEventArgs e) {
            if (!double.IsNaN(Browser.Control.ActualWidth)) {
                _settings.PreviewWidth = (int)Browser.Control.ActualWidth;
            }
        }

        private void BottomDragCompleted(object sender, DragCompletedEventArgs e) {
            if (!double.IsNaN(Browser.Control.ActualHeight)) {
                _settings.PreviewHeight = (int)Browser.Control.ActualHeight;
            }
        }
    }
}

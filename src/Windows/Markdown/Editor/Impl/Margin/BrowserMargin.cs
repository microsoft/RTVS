// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Margin {
    // Based on https://github.com/madskristensen/MarkdownEditor/blob/master/src/Margin/BrowserMargin.cs
    public sealed class BrowserMargin : DockPanel, IWpfTextViewMargin {
        private readonly IServiceContainer _services;
        private readonly IIdleTimeService _idleTime;
        private readonly ITextDocument _document;
        private readonly ITextView _textView;
        private readonly RMarkdownOptions _options;

        private bool _connectedToIdle;
        private bool _updateContent;
        private bool _updatePosition;
        private int _lastLineNumber;

        public bool Enabled => true;
        public double MarginSize => 500;
        public FrameworkElement VisualElement => this;
        public BrowserView Browser { get; }

        public BrowserMargin(ITextView textView, ITextDocument document, IServiceContainer services) {
            _textView = textView;
            _document = document;
            _services = services;
            _options = services.GetService<IREditorSettings>().MarkdownOptions;

            Browser = new BrowserView(_document.FilePath, services);

            if (_options.PreviewPosition == RMarkdownPreviewPosition.Below) {
                CreateBottomMarginControls();
            } else {
                CreateRightMarginControls();
            }

            UpdateBrowser();

            // TODO: separate R code changes from markdown changes
            _idleTime = _services.GetService<IIdleTimeService>();
            _textView.TextBuffer.Changed += OnTextBufferChanged;
            _textView.Caret.PositionChanged += OnCaretPositionChanged;
        }

        public void Dispose() => Browser?.Dispose();

        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e) {
            _updatePosition = true;
            ConnectToIdle();
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) {
            _updateContent = true;
            ConnectToIdle();
        }

        private void OnIdle(object sender, EventArgs e) {
            DisconnectFromIdle();

            if (_updateContent) {
                UpdateBrowser();
                _updateContent = false;
            }

            if (_updatePosition) {
                UpdatePosition();
                _updatePosition = false;
            }
        }

        private void UpdatePosition() {
            var lineNumber = _textView.TextSnapshot.GetLineNumberFromPosition(_textView.TextViewLines.FirstVisibleLine.Start.Position);
            if (lineNumber != _lastLineNumber) {
                _lastLineNumber = lineNumber;
                Browser.UpdatePosition(lineNumber);
            }
        }

        private void UpdateBrowser() => Browser.UpdateBrowserAsync(_document.TextBuffer.CurrentSnapshot).DoNotWait();

        public ITextViewMargin GetTextViewMargin(string marginName) => this;

        private void CreateRightMarginControls() {
            var width = _options.PreviewWidth;

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(width, GridUnitType.Pixel), MinWidth = 150 });
            grid.RowDefinitions.Add(new RowDefinition());
            Children.Add(grid);

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
            var height = _options.PreviewHeight;

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5, GridUnitType.Pixel) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(height, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.Children.Add(Browser.Control);
            Children.Add(grid);

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
                _options.PreviewWidth = (int)Browser.Control.ActualWidth;
            }
        }

        private void BottomDragCompleted(object sender, DragCompletedEventArgs e) {
            if (!double.IsNaN(Browser.Control.ActualHeight)) {
                _options.PreviewHeight = (int)Browser.Control.ActualHeight;
            }
        }

        private void ConnectToIdle() {
            if (!_connectedToIdle) {
                _idleTime.Idle += OnIdle;
                _connectedToIdle = true;
            }
        }

        private void DisconnectFromIdle() {
            _idleTime.Idle -= OnIdle;
            _connectedToIdle = false;
        }
    }
}

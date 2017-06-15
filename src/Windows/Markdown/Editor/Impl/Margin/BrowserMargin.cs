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
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Margin {
    // Based on https://github.com/madskristensen/MarkdownEditor/blob/master/src/Margin/BrowserMargin.cs
    public sealed class BrowserMargin : DockPanel, IWpfTextViewMargin {
        private static object _idleActionTag;

        private readonly IIdleTimeService _idleTime;
        private readonly ITextDocument _document;
        private readonly ITextView _textView;
        private readonly IRMarkdownEditorSettings _settings;

        private bool _updateContent;
        private bool _updatePosition;
        private int _lastLineNumber;
        private Task _browserUpdateTask;

        public bool Enabled => true;
        public double MarginSize => 500;
        public FrameworkElement VisualElement => this;
        public Browser Browser { get; }

        public BrowserMargin(ITextView textView, ITextDocument document, IServiceContainer services) {
            _idleActionTag = _idleActionTag ?? this.GetType();

            _textView = textView;
            _document = document;
            _settings = services.GetService<IRMarkdownEditorSettings>();

            Browser = new Browser(_document.FilePath, services);

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
        }

        public void Dispose() {
            _updateContent = _updatePosition = false;
            Browser?.Dispose();
        }

        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e) {
            _updatePosition = true;
            if (IsCaretInRCode()) {
                var lineNumber =
                    _textView.TextBuffer.CurrentSnapshot.GetLineNumberFromPosition(_textView.Caret.Position
                        .BufferPosition);
                if (lineNumber == _lastLineNumber) {
                    _updateContent = false;
                }
                _lastLineNumber = lineNumber;
            }

            IdleTimeAction.Cancel(_idleActionTag);
            IdleTimeAction.Create(Update, 500, _idleActionTag, _idleTime);
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) => _updateContent = true;

        private void Update() {
            if (_updateContent) {
                UpdateBrowser();
                _updateContent = false;
            }

            if (_updatePosition) {
                Browser.UpdatePosition(_lastLineNumber);
                _updatePosition = false;
            }
        }

        private void UpdateBrowser() {
            if (_browserUpdateTask == null) {
                _browserUpdateTask = Browser
                    .UpdateBrowserAsync(_document.TextBuffer.CurrentSnapshot)
                    .ContinueWith(t => _browserUpdateTask = null);
            } else {
                // Still running, try again later
                IdleTimeAction.Create(Update, 500, _idleActionTag, _idleTime);
            }
        }

        public ITextViewMargin GetTextViewMargin(string marginName) => this;

        private void CreateRightMarginControls() {
            var width = _settings.PreviewWidth;

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
            var height = _settings.PreviewHeight;

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
                _settings.PreviewWidth = (int)Browser.Control.ActualWidth;
            }
        }

        private void BottomDragCompleted(object sender, DragCompletedEventArgs e) {
            if (!double.IsNaN(Browser.Control.ActualHeight)) {
                _settings.PreviewHeight = (int)Browser.Control.ActualHeight;
            }
        }

        private bool IsCaretInRCode() {
            var containedLanguageHandler = _textView.TextBuffer.GetService<IContainedLanguageHandler>();
            var block = containedLanguageHandler?.GetCodeBlockOfLocation(_textView.Caret.Position.BufferPosition);
            return block != null;
        }
    }
}

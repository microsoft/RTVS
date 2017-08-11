// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Preview.Margin {
    internal sealed class MarginControls {
        private readonly IRMarkdownEditorSettings _settings;
        private readonly ITextView _textView;
        private readonly WebBrowser _webBrowser;

        public MarginControls(Decorator parent, ITextView textView, WebBrowser webBrowser, IRMarkdownEditorSettings settings) {
            _textView = textView;
            _webBrowser = webBrowser;
            _settings = settings;

            if (_settings.PreviewPosition == RMarkdownPreviewPosition.Below) {
                CreateBottomMarginControls(parent);
            } else {
                CreateRightMarginControl(parent);
            }
        }

        private void CreateRightMarginControl(Decorator parent) {
            var width = _settings.PreviewWidth;

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(width, GridUnitType.Pixel), MinWidth = 150 });
            grid.RowDefinitions.Add(new RowDefinition());
            parent.Child = grid;

            grid.Children.Add(_webBrowser);
            Grid.SetColumn(_webBrowser, 2);
            Grid.SetRow(_webBrowser, 0);

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

        private void CreateBottomMarginControls(Decorator parent) {
            var height = _settings.PreviewHeight;

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5, GridUnitType.Pixel) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(height, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.Children.Add(_webBrowser);
            parent.Child = grid;

            Grid.SetColumn(_webBrowser, 0);
            Grid.SetRow(_webBrowser, 2);

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
            if (!double.IsNaN(_webBrowser.ActualWidth)) {
                _settings.PreviewWidth = (int)_webBrowser.ActualWidth;
            }
        }

        private void BottomDragCompleted(object sender, DragCompletedEventArgs e) {
            if (!double.IsNaN(_webBrowser.ActualHeight)) {
                _settings.PreviewHeight = (int)_webBrowser.ActualHeight;
            }
        }
    }
}

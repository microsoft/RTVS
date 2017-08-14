// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Common.Wpf.Extensions;
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

            var splitter = new GridSplitter {
                Width = 5,
                ResizeDirection = GridResizeDirection.Columns,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            splitter.DragCompleted += RightDragCompleted;

            var grid = new Grid {
                ColumnDefinitions = {
                    new ColumnDefinition {Width = new GridLength(0, GridUnitType.Star)},
                    new ColumnDefinition {Width = new GridLength(5, GridUnitType.Pixel)},
                    new ColumnDefinition {Width = new GridLength(width, GridUnitType.Pixel), MinWidth = 150}
                },
                RowDefinitions = { new RowDefinition() },
                Children = {
                    splitter.SetGridPosition(0, 1),
                    _webBrowser.SetGridPosition(0, 2)
                }
            };

            parent.Child = grid;

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

            var splitter = new GridSplitter {
                Height = 5,
                ResizeDirection = GridResizeDirection.Rows,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            splitter.DragCompleted += BottomDragCompleted;

            var grid = new Grid {
                RowDefinitions = {
                    new RowDefinition { Height = new GridLength(0, GridUnitType.Star)},
                    new RowDefinition { Height = new GridLength(5, GridUnitType.Pixel)},
                    new RowDefinition { Height = new GridLength(height, GridUnitType.Pixel)}
                },
                ColumnDefinitions = { new ColumnDefinition() },
                Children = {
                    splitter.SetGridPosition(1, 0),
                    _webBrowser.SetGridPosition(2, 0)
                }
            };
            parent.Child = grid;
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

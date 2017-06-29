// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace Microsoft.R.Components.History.Implementation {
    internal class HistorySelectionTextAdornment {
        private const string ActiveSelectionPropertiesName = "Selected Text";
        private const string InactiveSelectionPropertiesName = "Inactive Selected Text";

        private readonly IAdornmentLayer _layer;
        private readonly IWpfTextView _textView;
        private readonly IEditorFormatMap _editorFormatMap;
        private readonly IRHistoryVisual _history;

        private double _lastWidth;

        private VisualToolset _activeVisualToolset;
        private VisualToolset _inactiveVisualToolset;
        private bool _isTextViewActive;

        public HistorySelectionTextAdornment(IWpfTextView textView, IEditorFormatMapService editorFormatMapService, IRHistoryProvider historyProvider) {
            _textView = textView;
            _layer = textView.GetAdornmentLayer("HistorySelectionTextAdornment");

            _editorFormatMap = editorFormatMapService.GetEditorFormatMap(_textView);
            _history = historyProvider.GetAssociatedRHistory(_textView);

            // Advise to events
            _editorFormatMap.FormatMappingChanged += OnFormatMappingChanged;
            _textView.VisualElement.GotKeyboardFocus += OnGotKeyboardFocus;
            _textView.VisualElement.LostKeyboardFocus += OnLostKeyboardFocus;
            _textView.LayoutChanged += OnLayoutChanged;
            _textView.Closed += OnClosed;
            _history.SelectionChanged += OnSelectionChanged;

            _activeVisualToolset = CreateVisualToolset(ActiveSelectionPropertiesName, SystemColors.HighlightColor);
            _inactiveVisualToolset = CreateVisualToolset(InactiveSelectionPropertiesName, SystemColors.GrayTextColor);
            Redraw();
        }

        private void OnFormatMappingChanged(object sender, FormatItemsEventArgs e) {
            if (e.ChangedItems.Contains(ActiveSelectionPropertiesName)) {
                _activeVisualToolset = CreateVisualToolset(ActiveSelectionPropertiesName, SystemColors.HighlightColor);

                if (_isTextViewActive) {
                    Redraw();
                }
            }

            if (e.ChangedItems.Contains(InactiveSelectionPropertiesName)) {
                _inactiveVisualToolset = CreateVisualToolset(InactiveSelectionPropertiesName, SystemColors.GrayTextColor);

                if (!_isTextViewActive) {
                    Redraw();
                }
            }
        }

        private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (!_textView.IsClosed && !_isTextViewActive) {
                _isTextViewActive = true;
                Redraw();
            }
        }

        private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (!_textView.IsClosed && _isTextViewActive) {
                _isTextViewActive = false;
                Redraw();
            }
        }

        private void OnClosed(object sender, EventArgs e) {
            _editorFormatMap.FormatMappingChanged -= OnFormatMappingChanged;
            _textView.VisualElement.GotKeyboardFocus -= OnGotKeyboardFocus;
            _textView.VisualElement.LostKeyboardFocus -= OnLostKeyboardFocus;
            _textView.LayoutChanged -= OnLayoutChanged;
            _textView.Closed -= OnClosed;
            _history.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, EventArgs e) {
            Redraw();
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e) {
            if (_layer.TextView.ViewportRight > _lastWidth) {
                _lastWidth = _layer.TextView.ViewportRight + 100;
                Redraw();
            } else if (e.NewOrReformattedLines.Any()) {
                Redraw();
            }
        }

        private void Redraw() {
            _layer.RemoveAllAdornments();

            if (!_history.HasSelectedEntries) {
                return;
            }

            var selectedSpans = _history.GetSelectedHistoryEntrySpans();
            foreach (var span in selectedSpans) {
                ProcessLine(span);
            }
        }

        private void ProcessLine(SnapshotSpan span) {
            var textMarkerGeometry = _textView.TextViewLines.GetTextMarkerGeometry(span);
            if (textMarkerGeometry == null) {
                return;
            }

            var bounds = textMarkerGeometry.Bounds;

            VisualToolset visualTools = _isTextViewActive ? _activeVisualToolset : _inactiveVisualToolset;
            var geometry = new RectangleGeometry(new Rect(0, bounds.Top, _lastWidth, (int) (bounds.Height + 0.5)));

            DrawHighlight(geometry, visualTools, span);
        }

        private void DrawHighlight(Geometry g, VisualToolset visualTools, SnapshotSpan span) {
            if (g.CanFreeze) {
                g.Freeze();
            }

            var dv = new DrawingVisual();
            DrawingContext dContext = dv.RenderOpen();
            dContext.DrawGeometry(visualTools.Brush, visualTools.Pen, g);
            dContext.Close();

            var uiElement = new DrawingVisualHost(dv);

            _layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, new object(), uiElement, null);
        }

        private VisualToolset CreateVisualToolset(string category, Color defaultColor) {
            var dictionary = _editorFormatMap.GetProperties(category);
            var toolset = new VisualToolset();

            if (dictionary.Contains(EditorFormatDefinition.BackgroundBrushId)) {
                toolset.Brush = ((Brush) (dictionary[EditorFormatDefinition.BackgroundBrushId])).Clone();
            } else if (dictionary.Contains(EditorFormatDefinition.BackgroundColorId)) { 
                toolset.Brush = new SolidColorBrush((Color) dictionary[EditorFormatDefinition.BackgroundColorId]);
            } else {
               toolset.Brush = new SolidColorBrush(defaultColor);
            }

            if (toolset.Brush.CanFreeze) {
                toolset.Brush.Freeze();
            }

            const string penResourceName = "BackgroundPen";
            if (_textView.Options.IsSimpleGraphicsEnabled() || !dictionary.Contains(penResourceName)) {
                var useReducedOpacityForHighContrast = _textView.Options.GetOptionValue(DefaultWpfViewOptions.UseReducedOpacityForHighContrastOptionId);
                
                // Layer opacity should match selection painters
                _layer.Opacity = (useReducedOpacityForHighContrast || !SystemParameters.HighContrast) && (toolset.Brush is SolidColorBrush) ? 0.4 : 1;
                return toolset;
            }

            toolset.Pen = ((Pen)dictionary[penResourceName]).Clone();

            if (toolset.Pen.CanFreeze) {
                toolset.Pen.Freeze();
            }

            _layer.Opacity = 1;
            return toolset;
        }

        private class VisualToolset {
            public Brush Brush { get; set; }
            public Pen Pen { get; set; }
        }
    }

    public class DrawingVisualHost : FrameworkElement {
        private readonly DrawingVisual _child;

        public DrawingVisualHost(DrawingVisual child) {
            _child = child;
            AddVisualChild(child);
        }

        protected override Visual GetVisualChild(int index) {
            return _child;
        }

        protected override int VisualChildrenCount => 1;
    }
}
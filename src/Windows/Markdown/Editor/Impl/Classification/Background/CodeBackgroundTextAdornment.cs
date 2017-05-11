// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Projection;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.Markdown.Editor.Classification {
    /// <summary>
    /// Inmplements background highlight of server-side code regions
    /// as well as script and style blocks. Highlights entire lines
    /// across the view, including empty lines.
    /// </summary>
    internal sealed class CodeBackgroundTextAdornment {
        private readonly IAdornmentLayer _layer;
        private readonly IWpfTextView _view;
        private readonly IClassificationFormatMap _classificationFormatMap;
        private readonly IClassificationTypeRegistryService _classificationTypeRegistry;
        private readonly IContainedLanguageHandler _contanedLanguageHandler;

        private double _lastWidth = 0;
        private int _reprocessFrom = -1;
        private Brush _backgroudColorBrush;

        public CodeBackgroundTextAdornment(
            IWpfTextView view,
            IClassificationFormatMapService classificationFormatMapService,
            IClassificationTypeRegistryService classificationTypeRegistry) {

            _view = view;
            _layer = view.GetAdornmentLayer("CodeBackgroundTextAdornment");

            _classificationTypeRegistry = classificationTypeRegistry;
            _classificationFormatMap = classificationFormatMapService.GetClassificationFormatMap(view);

            // Advise to events
            _classificationFormatMap.ClassificationFormatMappingChanged += OnClassificationFormatMappingChanged;
            _view.LayoutChanged += OnLayoutChanged;
            _view.Closed += OnClosed;

            var projectionBufferManager = ProjectionBufferManager.FromTextBuffer(view.TextBuffer);
            if (projectionBufferManager != null) {
                projectionBufferManager.MappingsChanged += OnMappingsChanged;
                _contanedLanguageHandler = projectionBufferManager.DiskBuffer.GetService<IContainedLanguageHandler>();
            }

            FetchColors();
        }

        private void OnMappingsChanged(object sender, EventArgs e) {
            // If projections changed, we need to reprocess the entire view
            // since removal or addition of server code separators, changing
            // script or style blocks may affect highlight in dramatic ways.
            _reprocessFrom = 0;
            ReprocessEntireView();
        }

        private void OnClosed(object sender, EventArgs e) {
            _classificationFormatMap.ClassificationFormatMappingChanged -= OnClassificationFormatMappingChanged;
            _view.LayoutChanged -= OnLayoutChanged;
            _view.Closed -= OnClosed;
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e) {
            if (_layer.TextView.ViewportRight > _lastWidth) {
                // View width has changed - reprocess the entire view
                OnViewWidthChanged();
                return;
            }

            if (_reprocessFrom >= 0) {
                for (int i = 0; i < _view.TextViewLines.Count; i++) {
                    var line = _view.TextViewLines[i];
                    if (_reprocessFrom > line.End.Position) {
                        continue;
                    }
                    ProcessLine(line);
                }
                _reprocessFrom = -1;
            } else {
                foreach (var line in e.NewOrReformattedLines) {
                    ProcessLine(line);
                }
            }
        }

        private void OnViewWidthChanged() {
            // View width has changed - reprocess the entire view
            _lastWidth = _layer.TextView.ViewportRight + 100;
            ReprocessEntireView();
        }

        private void ReprocessEntireView() {
            _layer.RemoveAllAdornments();
            foreach (var line in _view.TextViewLines) {
                ProcessLine(line);
            }
        }

        private void ProcessLine(ITextViewLine line) {
            // If it is collapsed then in the view buffer line start and end map 
            // to the same line while in editor buffer they map to different lines
            _layer.RemoveAdornmentsByTag(line);

            var bufferSnapshot = line.Snapshot;
            var bufferLineStart = bufferSnapshot.GetLineFromPosition(line.Start);
            var bufferLineEnd = bufferSnapshot.GetLineFromPosition(line.End);

            if (bufferLineStart.LineNumber == bufferLineEnd.LineNumber) {
                var wpfLine = line as IWpfTextViewLine;
                if (wpfLine != null) {
                    CreateVisuals(wpfLine);
                }
            }
        }

        private void OnClassificationFormatMappingChanged(object sender, EventArgs e) {
            FetchColors();
            ReprocessEntireView();
        }

        private void CreateVisuals(IWpfTextViewLine line) {
            if (ShouldHighlightEntireLine(line)) {
                IWpfTextViewLineCollection textViewLines = _view.TextViewLines;
                var g = new RectangleGeometry(new Rect(0, line.TextTop, _lastWidth, (double)(int)(line.Height + 0.5)));
                SnapshotSpan span = new SnapshotSpan(line.Snapshot, new Span(line.Start, line.Length));
                CreateHighlight(line, g, span);
            }
        }

        private bool ShouldHighlightEntireLine(IWpfTextViewLine line) {
            if (_contanedLanguageHandler != null) {
                return _contanedLanguageHandler.GetCodeBlockOfLocation(line.Start.Position) != null ||
                       _contanedLanguageHandler.GetCodeBlockOfLocation(line.End.Position) != null;
            }
            return false;
        }

        private void CreateHighlight(IWpfTextViewLine line, Geometry g, SnapshotSpan span) {
            if (g != null) {
                var uiElement = new Rectangle();

                uiElement.Width = g.Bounds.Width;
                uiElement.Height = g.Bounds.Height;
                uiElement.Fill = _backgroudColorBrush;

                //Align the image with the top of the bounds of the text geometry
                Canvas.SetLeft(uiElement, g.Bounds.Left);
                Canvas.SetTop(uiElement, g.Bounds.Top);

                _layer.RemoveAdornmentsByTag(line);
                _layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, line, uiElement, null);
            }
        }

        private void FetchColors() {
            var ct = _classificationTypeRegistry.GetClassificationType(MarkdownClassificationTypes.CodeBackground);
            var props = _classificationFormatMap.GetTextProperties(ct);
            _backgroudColorBrush = props.BackgroundBrush;
        }
    }
}

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class TextVisual : DrawingVisual {

        public TextVisual() {
            Padding = 3.0;
        }

        public Brush Foreground { get; set; }

        public Typeface Typeface { get; set; }

        public double FontSize { get; set; }

        public int Row { get; set; }

        public int Column { get; set; }

        public double Padding { get; set; }

        private string _text;
        public string Text {
            get {
                return _text;
            }
            set {
                _text = value;
                _formattedText = null;
                _drawValid = false;
            }
        }

        private FormattedText _formattedText;
        public FormattedText GetFormattedText() {
            if (_formattedText == null) {
                _formattedText = new FormattedText(
                    Text,
                    CultureInfo.CurrentUICulture,
                    CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                    Typeface,
                    FontSize,
                    Foreground);
            }
            return _formattedText;
        }

        public Size Size { get; set; }

        private bool _drawValid = false;
        public bool Draw(Size refSize) {
            if (_drawValid) return false;
            DrawingContext dc = RenderOpen();
            try {
                var formattedText = GetFormattedText();

                Size = new Size(
                    Math.Max(refSize.Width, formattedText.Width + (2 * Padding)),
                    Math.Max(refSize.Height, formattedText.Height + (2 * Padding)));

                if (_isHighlight) {
                    dc.DrawRectangle(Brushes.Blue, null, new Rect(new Point(0, 0), Size));
                } else {
                    dc.DrawRectangle(Brushes.Transparent, null, new Rect(new Point(0, 0), Size));
                }

                dc.DrawText(formattedText, new Point(Padding, Padding));
                _drawValid = true;
                return true;
            } finally {
                dc.Close();
            }
        }

        private bool _isHighlight = false;
        public void ToggleHighlight() {
            _isHighlight ^= true;
            _drawValid = false;

            Draw(Size);
        }

        protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters) {
            return base.HitTestCore(hitTestParameters);
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters) {
            return base.HitTestCore(hitTestParameters);
        }
    }
}

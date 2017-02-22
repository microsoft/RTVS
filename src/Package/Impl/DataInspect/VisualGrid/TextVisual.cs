// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class TextVisual : DrawingVisual {

        #region Dependency Properties
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextVisual), new PropertyMetadata());

        public virtual string Text {
            get { return (string)GetValue(TextProperty); }
            set {
                SetValue(TextProperty, value);
                Invalidate();
            }
        }

        public static readonly DependencyProperty RowProperty =
            DependencyProperty.Register("Row", typeof(long), typeof(TextVisual), new PropertyMetadata());

        public virtual long Row {
            get { return (long)GetValue(RowProperty); }
            set { SetValue(RowProperty, value); }
        }

        public static readonly DependencyProperty ColumnProperty =
            DependencyProperty.Register("Column", typeof(long), typeof(TextVisual), new PropertyMetadata());

        public virtual long Column {
            get { return (long)GetValue(ColumnProperty); }
            set { SetValue(ColumnProperty, value); }
        }
        #endregion

        public Brush Foreground { get; set; }
        public Typeface Typeface { get; set; }
        public double FontSize { get; set; }
        public double Margin { get; set; } = 3.0;
        public double X { get; set; }
        public double Y { get; set; }
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Right;

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

        public Rect CellBounds { get; set; }

        private bool _drawValid = false;
        public bool Draw() {
            if (_drawValid) {
                return false;
            }

            DrawingContext dc = RenderOpen();
            try {
                var formattedText = GetFormattedText();
                double offset;
                Size = GetRenderSize(formattedText, out offset);
                TextAlignment = IsNumerical(Text) ? TextAlignment.Right : TextAlignment.Left;

                dc.DrawText(formattedText, new Point(offset, 0));
                _drawValid = true;
                return true;
            } finally {
                dc.Close();
            }
        }

        protected virtual Size GetRenderSize(FormattedText formattedText, out double offset) {
            offset = 0;
            return new Size(formattedText.Width, formattedText.Height);
        }

        private bool _isHighlight = false;
        public void ToggleHighlight() {
            _isHighlight ^= true;
            _drawValid = false;

            Draw();
        }

        protected void Invalidate() {
            _formattedText = null;
            _drawValid = false;
        }

        private static bool IsNumerical(string text) {
            double d;
            int x;
            if(double.TryParse(text, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out d) || int.TryParse(text, out x)) {
                return true;
            }
            return false;
        }
    }
}

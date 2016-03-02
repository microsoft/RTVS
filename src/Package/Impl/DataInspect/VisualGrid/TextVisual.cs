// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class TextVisual : DrawingVisual {

        public TextVisual() {
            Margin = 3.0;
        }

        public Brush Foreground { get; set; }

        public Typeface Typeface { get; set; }

        public double FontSize { get; set; }

        public int Row { get; set; }

        public int Column { get; set; }

        public double Margin { get; set; }

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
        public bool Draw() {
            if (_drawValid) return false;
            DrawingContext dc = RenderOpen();
            try {
                var formattedText = GetFormattedText();

                Size = new Size(formattedText.Width, formattedText.Height);

                dc.DrawText(formattedText, new Point());
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

            Draw();
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Microsoft.Common.Core;
using Microsoft.Common.Wpf.Extensions;
using static System.Windows.DependencyProperty;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class TextVisual : DrawingVisual {
        private bool _drawValid;
        private bool _isSelected;
        private bool _isFocused;
        private Rect _cellBounds;
        private FormattedText _formattedText;
        private double _textOffset;

        #region Dependency Properties
        public static readonly DependencyProperty TextProperty = Register(nameof(Text), typeof(string), typeof(TextVisual), new PropertyMetadata(OnTextPropertyChanged));

        public virtual string Text {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var oldValue = (string) args.OldValue;
            var newValue = (string) args.NewValue;
            if (!newValue.EqualsOrdinal(oldValue)) {
                ((TextVisual)obj).Invalidate();
            }
        }

        public static readonly DependencyProperty RowProperty = Register(nameof(Row), typeof(long), typeof(TextVisual), new PropertyMetadata());

        public long Row {
            get => (long)GetValue(RowProperty);
            set => SetValue(RowProperty, value);
        }

        public static readonly DependencyProperty ColumnProperty = Register(nameof(Column), typeof(long), typeof(TextVisual), new PropertyMetadata());

        public long Column {
            get => (long)GetValue(ColumnProperty);
            set => SetValue(ColumnProperty, value);
        }
        #endregion

        public Brush Background { get; set; }
        public Brush Foreground { get; set; }
        public Brush SelectedBackground { get; set; }
        public Brush SelectedForeground { get; set; }
        public Typeface Typeface { get; set; }
        public double FontSize { get; set; }
        public double Margin { get; set; } = 3.0;
        public double X { get; set; }
        public double Y { get; set; }
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Right;
        public Size Size { get; set; }

        public Rect CellBounds {
            get => _cellBounds;
            set {
                if (_cellBounds.IsCloseTo(value)) {
                    return;        
                }
                _cellBounds = value;
                Invalidate();
            }
        }

        public bool IsSelected {
            get => _isSelected;
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    Invalidate();
                }
            }
        }

        public bool IsFocused {
            get => _isFocused;
            set {
                if (_isFocused != value) {
                    _isFocused = value;
                    Invalidate();
                }
            }
        }

        public FormattedText GetFormattedText() {
#pragma warning disable CS0618 // Type or member is obsolete
            return _formattedText ?? (_formattedText = new FormattedText(
                Text,
                CultureInfo.CurrentUICulture,
                CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft 
                    ? FlowDirection.RightToLeft 
                    : FlowDirection.LeftToRight,
                Typeface,
                FontSize,
                IsSelected && IsFocused ? SelectedForeground : Foreground));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public bool Measure() {
            if (_drawValid) {
                return false;
            }

            var formattedText = GetFormattedText();
            Size = GetRenderSize(formattedText, out _textOffset);
            return true;
        }

        public bool Draw() {
            if (_drawValid) {
                return false;
            }

            var dc = RenderOpen();
            try {
                var formattedText = GetFormattedText();
                dc.DrawRectangle(IsSelected && IsFocused ? SelectedBackground : Background, null, CellBounds);
                dc.DrawText(formattedText, new Point(_textOffset, 0));
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

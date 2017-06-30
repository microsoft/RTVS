// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Text that appears in grid header. Tracks sorting order
    /// and displays the appropriate sort order glyph.
    /// </summary>
    public sealed class HeaderTextVisual : TextVisual {
        private const char ArrowUp = (char)0x25B4;
        private const char ArrowDown = (char)0x25BE;
        private const char Nbsp = (char)0x00A0;
        private char _arrowChar = Nbsp;

        #region Dependency Properties
        public static readonly DependencyProperty SortOrderProperty =
            DependencyProperty.Register("SortOrder", typeof(SortOrderType), typeof(TextVisual), new PropertyMetadata(SortOrderType.None));

        public SortOrderType SortOrder {
            get { return (SortOrderType)GetValue(SortOrderProperty); }
            set {
                SetValue(SortOrderProperty, value);
                SetArrowDisplay();
            }
        }
        #endregion

        public HeaderTextVisual(long columnIndex) {
            ColumnIndex = columnIndex;
        }

        public long ColumnIndex { get; }

        /// <summary>
        /// Name of the column
        /// </summary>
        public string Name => base.Text;

        public override string Text {
            get { return base.Text + _arrowChar; }
            set { base.Text = value; }
        }
        /// <summary>
        /// Calculates column header render size taking into account
        /// optional sorting arrow, if any.
        /// </summary>
        protected override Size GetRenderSize(FormattedText formattedText, out double offset) {
            var baseSize = base.GetRenderSize(formattedText, out offset);

            var currentArrowGlyph = new FormattedText(_arrowChar.ToString(), CultureInfo.CurrentUICulture,
                                               FlowDirection.LeftToRight, Typeface, FontSize, Foreground);
            var largestArrowGlyph = new FormattedText(ArrowUp.ToString(), CultureInfo.CurrentUICulture,
                                               FlowDirection.LeftToRight, Typeface, FontSize, Foreground);

            offset = largestArrowGlyph.Width;
            return new Size(baseSize.Width - currentArrowGlyph.Width + 2* largestArrowGlyph.Width, baseSize.Height);
        }

        private void SetArrowDisplay() {
            switch (SortOrder) {
                case SortOrderType.None:
                    _arrowChar = Nbsp;
                    break;

                case SortOrderType.Ascending:
                    _arrowChar = ArrowUp;
                    break;

                case SortOrderType.Descending:
                    _arrowChar = ArrowDown;
                    break;
            }
            Invalidate();
        }

        private static bool HasArrow(string text) {
            return text.Length > 0 && (text[text.Length - 1] == ArrowUp || text[text.Length - 1] == ArrowDown);
        }

        public void ToggleSortOrder() {
            switch (SortOrder) {
                case SortOrderType.None:
                    SortOrder = SortOrderType.Ascending;
                    break;
                case SortOrderType.Ascending:
                    SortOrder = SortOrderType.Descending;
                    break;
                case SortOrderType.Descending:
                    SortOrder = SortOrderType.Ascending;
                    break;
            }
        }
    }
}

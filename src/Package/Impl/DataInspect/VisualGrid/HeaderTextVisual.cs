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

        private SortOrderType _sortOrder;
        public SortOrderType SortOrder {
            get { return _sortOrder; }
            set {
                _sortOrder = value;
                SetArrowDisplay();
            }
        }

        public string Name {
            get {
                return HasArrow(Text) ? Text.Substring(0, Text.Length - 1) : Text;
            }
        }
        protected override Size GetRenderSize(FormattedText formattedText, out double offset) {
            var arrowGlyph = new FormattedText(((char)0x25B4).ToString(), CultureInfo.CurrentUICulture,
                                                FlowDirection.LeftToRight, Typeface, FontSize, Foreground);

            var baseSize = base.GetRenderSize(formattedText, out offset);
            offset = arrowGlyph.Width;
            if (!HasArrow(Text)) {
                return new Size(baseSize.Width + 2*arrowGlyph.Width, baseSize.Height);
            }
            return new Size(baseSize.Width + arrowGlyph.Width, baseSize.Height);
        }

        private void SetArrowDisplay() {
            string text = Text;
            switch (SortOrder) {
                case SortOrderType.None:
                    SetArrow('\0');
                    break;

                case SortOrderType.Ascending:
                    SetArrow(ArrowDown);
                    break;

                case SortOrderType.Descending:
                    SetArrow(ArrowUp);
                    break;
            }
        }

        private void SetArrow(char arrow) {
            string text = HasArrow(Text) ? Text.Substring(0, Text.Length - 1) : Text;
            Text = text + (arrow != '\0' ? arrow.ToString() : string.Empty);
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

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Drawing;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    public static class IdeUtilities {
        ///<summary>
        /// Convert a UI Dialog Font to a System.Drawing.Font
        /// </summary>
        public static Font FontFromUiDialogFont(UIDLGLOGFONT logFont) {
            var conversion = new char[logFont.lfFaceName.Length];
            for(int i = 0; i < logFont.lfFaceName.Length; i++) {
                conversion[i] = (char)logFont.lfFaceName[i];
            }

            var familyName = new String(conversion);
            var emSize = Math.Abs(logFont.lfHeight);
            var style = FontStyle.Regular;
            int FW_NORMAL = 400;

            if (logFont.lfItalic > 0) {
                style |= FontStyle.Italic;
            }
            if (logFont.lfUnderline > 0) {
                style |= FontStyle.Underline;
            }
            if (logFont.lfStrikeOut > 0) {
                style |= FontStyle.Strikeout;
            }
            if (logFont.lfWeight > FW_NORMAL) {
                style |= FontStyle.Bold;
            }

            var unit = GraphicsUnit.Pixel;
            var gdiCharSet = logFont.lfCharSet;
            var ff = new FontFamily(familyName);

            var teststyles = new FontStyle[5];
            teststyles[0] = style;
            teststyles[1] = FontStyle.Regular;
            teststyles[2] = FontStyle.Bold;
            teststyles[3] = FontStyle.Italic;
            teststyles[4] = FontStyle.Bold | FontStyle.Italic;

            for (int i = 0; i < teststyles.Length; i++) {
                if (ff.IsStyleAvailable(teststyles[i])) {
                    style = teststyles[i];
                    return new Font(familyName, emSize, style, unit, gdiCharSet);
                }
            }

            // We can't find a valid style for this font - fallback to just size and unit
            return new Font(familyName, emSize, unit);
        }
    }
}

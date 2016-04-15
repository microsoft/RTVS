// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Components.Extensions {
    public static class TextBufferExtensions {
        public static bool IsContentEqualsOrdinal(this ITextBuffer textBuffer, ITrackingSpan span1, ITrackingSpan span2) {
            var snapshot = textBuffer.CurrentSnapshot;
            var snapshotSpan1 = span1.GetSpan(snapshot);
            var snapshotSpan2 = span2.GetSpan(snapshot);

            if (snapshotSpan1 == snapshotSpan2) {
                return true;
            }

            if (snapshotSpan1.Length != snapshotSpan2.Length) {
                return false;
            }

            var start1 = snapshotSpan1.Start;
            var start2 = snapshotSpan2.Start;
            for (int i = 0; i < snapshotSpan1.Length; i++) {
                if (snapshot[start1 + i] != snapshot[start2 + i]) {
                    return false;
                }
            }

            return true;
        }

        public static ITextDocument ToTextDocument(this ITextBuffer textBuffer) {
            ITextDocument textDocument = null;
            textBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDocument);
            return textDocument;
        }

        public static string GetFilePath(this ITextBuffer textBuffer) {
            return textBuffer.ToTextDocument()?.FilePath;
        }

        public static void Save(this ITextBuffer textBuffer, Encoding encoding = null) {
            ITextDocument textDocument = textBuffer.ToTextDocument();
            if (textDocument != null && textDocument.IsDirty) {
                if (encoding != null) {
                    textDocument.Encoding = encoding;
                }
                textDocument.Save();
            }
        }

        /// <summary>
        /// Checks if file contents can be represented in the specified encoding without data loss.
        /// </summary>
        /// <param name="encoding"></param>
        public static bool IsConververtibleTo(this ITextBuffer textBuffer, Encoding encoding) {
            string original = textBuffer.CurrentSnapshot.GetText();
            using (var ms = new MemoryStream(original.Length * 2)) {
                using (var sw = new StreamWriter(ms, encoding)) {
                    sw.Write(original);
                    sw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(ms, encoding)) {
                        var converted = sr.ReadToEnd();
                        return converted.EqualsOrdinal(original);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if file contents can be saved to disk in the current encoding without data loss.
        /// </summary>
        /// <param name="encoding"></param>
        public static bool CanBeSavedInCurrentEncoding(this ITextBuffer textBuffer) {
            ITextDocument textDocument = textBuffer.ToTextDocument();
            if (textDocument != null) {
                return textBuffer.IsConververtibleTo(textDocument.Encoding);
            }
            return false;
        }

        /// <summary>
        /// Checks if file contents can be saved to disk in the current encoding without data loss.
        /// </summary>
        /// <param name="encoding"></param>
        public static Encoding GetEncoding(this ITextBuffer textBuffer) {
            ITextDocument textDocument = textBuffer.ToTextDocument();
            return textDocument != null ? textDocument.Encoding : Encoding.Default;
        }
    }
}

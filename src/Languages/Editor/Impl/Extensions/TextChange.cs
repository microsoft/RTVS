// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Languages.Editor {
    // Utility functions for calculating incremental text changes
    public class TextChange {
        public TextChange(int position, int length, string newText) {
            Position = position;
            Length = length;
            NewText = newText ?? string.Empty;
        }

        public int Position { get; set; }
        public int Length { get; set; }
        public string NewText { get; set; }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null)
                return false;

            TextChange other = obj as TextChange;

            if (other == null)
                return false;

            if ((other.Length == Length) &&
                (other.Position == Position) &&
                (other.NewText == NewText)) {
                return true;
            }

            return false;
        }
    }

    public static class TextChanges {
        public static IList<TextChange> BuildChangeList(string oldText, string newText, int maxMilliseconds) {
            List<TextChange> changes = new List<TextChange>();
            var sw = new Stopwatch();
            sw.Start();

            // Simple witespace/nowhitespace tokenization and comparison
            int oldIndex = 0;
            int newIndex = 0;

            while (true) {
                bool thereIsMore = NextChunk(oldText, ref oldIndex, newText, ref newIndex, changes);
                if (!thereIsMore)
                    break;

                thereIsMore = NextChunk(oldText, ref oldIndex, newText, ref newIndex, changes);
                if (!thereIsMore)
                    break;

                if (sw.ElapsedMilliseconds > maxMilliseconds) {
                    return null; // time's up
                }
            }

            return changes;
        }

        static bool NextChunk(string oldText, ref int oldIndex, string newText, ref int newIndex, List<TextChange> changes) {
            int oldLength = oldText.Length;
            int newLength = newText.Length;

            if (oldIndex >= oldLength && newIndex >= newLength)
                return false;

            if (oldIndex >= oldLength && newIndex < newLength) {
                // new text is longer, this is the last chunk
                var tc = new TextChange(oldLength, 0, newText.Substring(newIndex));
                AddChange(tc, changes);
                return false;
            }

            if (newIndex >= newLength && oldIndex < oldLength) {
                // old text is longer, this is the last chunk
                var tc = new TextChange(oldIndex, oldLength - oldIndex, string.Empty);
                AddChange(tc, changes);
                return false;
            }

            int oldChunkStart = oldIndex;
            int newChunkStart = newIndex;

            // collect whitespace in the old text - next time both indices will be at non-ws
            while (oldIndex < oldLength && char.IsWhiteSpace(oldText[oldIndex]))
                oldIndex++;

            // collect whitespace in the new text - next time both indices will be at non-ws
            while (newIndex < newLength && char.IsWhiteSpace(newText[newIndex]))
                newIndex++;

            if (oldIndex > oldChunkStart || newIndex > newChunkStart) {
                // some ws collected
                AddChange(oldText, oldChunkStart, oldIndex - oldChunkStart, newText, newChunkStart, newIndex - newChunkStart, changes);
                return true;
            }

            bool oldStartsWithDelimiter = false;
            bool newStartsWithDelimiter = false;

            // collect non-whitespace in the old text - next time both indices will be at whitespace or a delimiter
            while (oldIndex < oldLength) {
                char oldChar = oldText[oldIndex];
                if (char.IsWhiteSpace(oldChar))
                    break;

                oldIndex++;
            }

            while (newIndex < newLength) {
                char newChar = newText[newIndex];
                if (char.IsWhiteSpace(newChar))
                    break;

                newIndex++;
            }

            // If both start with a delimiter, then move them past it
            if (oldStartsWithDelimiter && newStartsWithDelimiter) {
                oldIndex += 1;
                newIndex += 1;
            }

            AddChange(oldText, oldChunkStart, oldIndex - oldChunkStart, newText, newChunkStart, newIndex - newChunkStart, changes);
            return true;
        }

        static void AddChange(
            string oldText,
            int oldChunkStart,
            int oldChunkLength,
            string newText,
            int newChunkStart,
            int newChunkLength,
            List<TextChange> changes) {
            int maxCharsToTrim = Math.Min(oldChunkLength, newChunkLength);
            int i;

            // trim off matching characters from the start
            for (i = 0; i < maxCharsToTrim; i++) {
                if (oldText[oldChunkStart + i] != newText[newChunkStart + i])
                    break;
            }

            if (i > 0) {
                oldChunkStart += i;
                newChunkStart += i;
                oldChunkLength -= i;
                newChunkLength -= i;
                maxCharsToTrim -= i;
            }

            // trim off matching characters from the end
            int oldChunkLastIndex = oldChunkStart + oldChunkLength - 1;
            int newChunkLastIndex = newChunkStart + newChunkLength - 1;
            for (i = 0; i < maxCharsToTrim; i++) {
                if (oldText[oldChunkLastIndex - i] != newText[newChunkLastIndex - i])
                    break;
            }

            if (i > 0) {
                oldChunkLength -= i;
                newChunkLength -= i;
            }

            if (oldChunkLength != 0 || newChunkLength != 0) {
                if (newChunkLength > 0) {
                    // Dev12 933254: Don't allow a \r\n at the end to get broken apart
                    //  At some point we may want to handle more of the \r\n splitting cases,
                    //  but this is the only one that we know comes up (due to us converting
                    //  a \n to a \r\n
                    newChunkLastIndex = newChunkStart + newChunkLength - 1;
                    if ((newText[newChunkLastIndex] == '\r') && (newChunkLastIndex + 1 < newText.Length) && (newText[newChunkLastIndex + 1] == '\n')) {
                        newChunkLength += 1;
                        oldChunkLength += 1;
                    }
                    else if ((newText[newChunkLastIndex] == '\n') && (newChunkLastIndex + 1 < newText.Length) && (newText[newChunkLastIndex + 1] == '\r')) {
                        newChunkLength += 1;
                        oldChunkLength += 1;
                    }
                }

                string newChunkText = newText.Substring(newChunkStart, newChunkLength);

                var tc = new TextChange(oldChunkStart, oldChunkLength, newChunkText);
                AddChange(tc, changes);
            }
        }

        static void AddChange(TextChange tc, List<TextChange> changes) {
            if (changes.Count > 0) {
                TextChange lastChange = changes[changes.Count - 1];

                // Check if new change can be merged with the last one
                if (tc.Position == lastChange.Position + lastChange.Length) {
                    if (lastChange.NewText.Length == 0 && tc.NewText.Length == 0) // both delete
                    {
                        lastChange.Length += tc.Length;
                        return;
                    } else if (lastChange.NewText.Length > 0 && tc.NewText.Length > 0) // both insert or replace
                      {
                        lastChange.Length += tc.Length;
                        lastChange.NewText += tc.NewText;
                        return;
                    }
                }
            }

            changes.Add(tc);
        }
    }
}

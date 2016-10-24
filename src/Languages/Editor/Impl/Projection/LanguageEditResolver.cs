// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.Languages.Editor.Projection {
    internal sealed class LanguageEditResolver : IProjectionEditResolver {
        private static bool tracing = false;
        private readonly ITextBuffer _diskBuffer;

        public LanguageEditResolver(ITextBuffer diskBuffer) {
            _diskBuffer = diskBuffer;
        }

        #region IProjectionEditResolver
        public void FillInInsertionSizes(SnapshotPoint projectionInsertionPoint, ReadOnlyCollection<SnapshotPoint> sourceInsertionPoints, string insertionText, IList<int> insertionSizes) {
            int charactersResolved = 0;
            for (int i = 0; i < sourceInsertionPoints.Count; i++) {
                SnapshotPoint snapshotPoint = sourceInsertionPoints[i];
                if (charactersResolved == insertionText.Length) {
                    insertionSizes[i] = 0;
                } else if (snapshotPoint.Snapshot.TextBuffer == _diskBuffer) {
                    insertionSizes[i] = insertionText.Length;
                } else {
                    insertionSizes[i] = 0;
                }

                charactersResolved += insertionSizes[i];
            }
        }

        public void FillInReplacementSizes(SnapshotSpan projectionReplacementSpan,
                                           ReadOnlyCollection<SnapshotSpan> sourceReplacementSpans,
                                           string insertionText,
                                           IList<int> insertionSizes) {
            for (int i = 0; i < insertionSizes.Count; ++i) {
                insertionSizes[i] = 0;
            }

            // We want to maintain the invariant that the contents of the html buffer and
            // the contents of the view buffer are the same at the end of every edit transaction.

            // There are two ways to enter this routine:
            //   (1) the user performed a replace at the view level that abuts one or both ends of the embedded 
            //       language text (and possibly touching more than one nugget or script). In this case all inert buffer 
            //       source replacement spans will have length zero, since none of the inert text extends into the view.
            //   (2) the embedded language compiler has performed a replacement on the language buffer that
            //       either replaces some text in the inert buffer or replaces text on the boundary of the inert
            //       buffer and the html buffer.
            //

            // The first scenario we treat is when no text was deleted from the inert buffer. This always occurs in
            // case (1) and can occur in case(2). We want to route all the text into the html buffer, and it doesn't matter 
            // which html insertion point we choose because if there is more than one the insertions would all map to the same 
            // location anyhow.

            bool inertInvolvement = false;
            int htmlPos = -1;
            for (int i = 0; i < sourceReplacementSpans.Count; ++i) {
                SnapshotSpan sourceReplacementSpan = sourceReplacementSpans[i];
                if (sourceReplacementSpan.Snapshot.TextBuffer != _diskBuffer && sourceReplacementSpan.Length > 0) {
                    inertInvolvement = true;
                } else if (htmlPos == -1 && sourceReplacementSpan.Snapshot.TextBuffer == _diskBuffer) {
                    htmlPos = i;
                }
            }

            if (!inertInvolvement) {
                // No text deleted from inert buffer, everything goes in the first html segment.
                insertionSizes[htmlPos] = insertionText.Length;
                return;
            } else if ((sourceReplacementSpans[htmlPos].Length == 0) && (sourceReplacementSpans.Count == 2)) {
                // An edit occurred which involves a single inert span and an empty html span. The inert span
                //   should take the changes.
                int inertPos = (htmlPos == 0 ? 1 : 0);
                insertionSizes[inertPos] = insertionText.Length;
                return;
            }

            // we want to find the source from the html buffer, and try to find it's location in insertionText.
            string htmlSourceText = sourceReplacementSpans[htmlPos].GetText().ToLowerInvariant();
            string priorInertSource = (htmlPos > 0 ? sourceReplacementSpans[htmlPos - 1].GetText().ToLowerInvariant() : null);
            string trailingInertSource = (htmlPos < sourceReplacementSpans.Count - 1 ? sourceReplacementSpans[htmlPos + 1].GetText().ToLowerInvariant() : null);
            insertionText = insertionText.ToLowerInvariant();

            int htmlStartPos;
            int htmlEndPos;
            FindNewPositions(htmlSourceText, insertionText, priorInertSource, trailingInertSource, true, out htmlStartPos, out htmlEndPos);

            if (htmlStartPos < 0) {
                // This condition will be met if htmlSourceText only contains whitespace, or if no prefix of the html start delimiter
                //   is found in insertionText. In either case, we will try to recover by finding the delimiter at the 
                //   end of the priorInertSource and at the beginning of trailingInertSource
                int priorInertEndPos = 0;
                if (priorInertSource != null) {
                    int priorInertStartPos;
                    FindNewPositions(priorInertSource, insertionText, "", htmlSourceText, false, out priorInertStartPos, out priorInertEndPos);
                }

                string remainingInsertionText = insertionText.Substring(Math.Max(0, priorInertEndPos));
                int trailingInertStartPos = remainingInsertionText.Length;
                if (trailingInertSource != null) {
                    int trailingInertEndPos;
                    FindNewPositions(trailingInertSource, remainingInsertionText, htmlSourceText, "", false, out trailingInertStartPos, out trailingInertEndPos);
                }

                if ((priorInertEndPos >= 0) && (trailingInertStartPos >= 0)) {
                    // The best guess we have for the html range is [priorInertEndPos, trailingInertStartPos]
                    htmlStartPos = priorInertEndPos;
                    htmlEndPos = trailingInertStartPos + priorInertEndPos;
                } else if (priorInertEndPos >= 0) {
                    // We found a match in priorInertSource
                    htmlStartPos = priorInertEndPos;
                    htmlEndPos = Math.Min(priorInertEndPos + htmlSourceText.Length, insertionText.Length);
                } else if (trailingInertStartPos >= 0) {
                    // We found a match in trailingInertSource
                    htmlStartPos = Math.Max(0, trailingInertStartPos - htmlSourceText.Length);
                    htmlEndPos = trailingInertStartPos;
                } else {
                    // we couldn't find any non-ws match from any of the original strings. 
                    int priorInertSourceLen = (priorInertSource == null ? 0 : priorInertSource.Length);
                    htmlStartPos = Math.Min(priorInertSourceLen, insertionText.Length);
                    htmlEndPos = Math.Min(htmlStartPos + htmlSourceText.Length, insertionText.Length);
                }
            }

            UpdateInsertionSizes(sourceReplacementSpans, insertionText, htmlStartPos, htmlEndPos, insertionSizes);
        }

        public int GetTypicalInsertionPosition(SnapshotPoint projectionInsertionPoint, ReadOnlyCollection<SnapshotPoint> sourceInsertionPoints) {
            int insertionIndex = 0;
            while (insertionIndex < sourceInsertionPoints.Count - 1) {
                SnapshotPoint curSnapshotPoint = sourceInsertionPoints[insertionIndex];
                if (curSnapshotPoint.Snapshot.TextBuffer == _diskBuffer)
                    break;

                insertionIndex++;
            }

            return insertionIndex;
        }
        #endregion

        #region IProjectionEditResolver Helpers

        private void FindNewPositions(string sourceText, string insertionText, string priorSource, string trailingSource, bool maintainWSOnly, out int startPos, out int endPos) {
            string startDelimiter;
            string endDelimiter;

            // find the significant text delimiters on the edges of sourceText
            FindDelimiters(sourceText, out startDelimiter, out endDelimiter);

            startPos = -1;
            endPos = -1;

            int startDelimiterLen = 0;
            if (!String.IsNullOrWhiteSpace(startDelimiter)) {
                // we have a start delimiter, find it's location in insertion text as best as possible
                startPos = GetInsertionStartIndex(insertionText, priorSource, startDelimiter, maintainWSOnly, out startDelimiterLen);
            }

            if (startPos >= 0) {
                // Workaround Roslyn bug where insertionText is shorter than sourceText. IIS OOB 37504
                if (startPos + startDelimiterLen <= insertionText.Length) {
                    // Some representation of the start delimiter was found. Now determine where the end
                    //   delimiter lies.
                    string remainingInsertionText = insertionText.Substring(startPos + startDelimiterLen);
                    endPos = GetInsertionEndIndex(remainingInsertionText, trailingSource, endDelimiter, maintainWSOnly);

                    if (endPos >= 0) {
                        endPos = endPos + startPos + startDelimiterLen;
                    } else {
                        // There wasn't an end delimiter, the endPos will be the end of the start delimiter
                        endPos = startPos + startDelimiterLen;
                    }

                    EnsureWhitespaceAroundPositions(insertionText, sourceText, ref startPos, ref endPos);
                }
            }
        }

        private static void FindDelimiters(string text, out string startDelimiter, out string endDelimiter) {
            text = text.Trim();
            if (text.Length == 0) {
                // only whitespace in text, return empty delimiters
                startDelimiter = "";
                endDelimiter = "";
            } else {
                // count non-whitespace at the beginning to find the first delimiter
                int startNonWSCount = 0;
                while ((startNonWSCount < text.Length) && (!Char.IsWhiteSpace(text[startNonWSCount]))) {
                    startNonWSCount++;
                }

                // count non-whitespace at the end to find the end delimiter
                int endNonWSCount = 0;
                if (startNonWSCount < text.Length) {
                    while ((endNonWSCount < text.Length) && (!Char.IsWhiteSpace(text[text.Length - endNonWSCount - 1]))) {
                        endNonWSCount++;
                    }
                }

                startDelimiter = text.Substring(0, startNonWSCount);
                endDelimiter = text.Substring(text.Length - endNonWSCount, endNonWSCount);
            }
        }

        private int GetInsertionStartIndex(string insertionText, string priorSource, string startDelimiter, bool maintainWSOnly, out int delimiterLen) {
            int insertionStartIndex = -1;

            if (maintainWSOnly && String.IsNullOrWhiteSpace(priorSource)) {
                // priorSource is all whitespace, and they've indicated they don't want non-whitespace injected into it.
                //    the start delimiter will start at the first non-ws in insertionText
                string insertionTextTrimmed = insertionText.TrimStart();
                insertionStartIndex = insertionText.Length - insertionTextTrimmed.Length;
            } else {
                if (!String.IsNullOrEmpty(startDelimiter)) {
                    // Find the location in insertionText that matches startDelimiter
                    insertionStartIndex = GetInsertionStartIndexHelper(insertionText, priorSource, startDelimiter);

                    while ((insertionStartIndex < 0) && (startDelimiter.Length > 1)) {
                        // couldn't find a location, trim off the last char in startDelimiter and try again
                        startDelimiter = startDelimiter.Substring(0, startDelimiter.Length - 1);
                        insertionStartIndex = GetInsertionStartIndexHelper(insertionText, priorSource, startDelimiter);
                    }
                }
            }

            delimiterLen = startDelimiter.Length;

            return insertionStartIndex;
        }

        private static int GetInsertionStartIndexHelper(string insertionText, string priorSource, string startDelimiter) {
            int insertionIndex = insertionText.IndexOf(startDelimiter, StringComparison.Ordinal);

            if (insertionIndex >= 0) {
                // found a match. Determine how many times the delimiter appeared in priorSource, and skip
                //   that many matches.
                int inertMatchCount = GetMatchCount(priorSource, startDelimiter);
                while ((insertionIndex >= 0) && (inertMatchCount > 0)) {
                    insertionIndex = insertionText.IndexOf(startDelimiter, insertionIndex + startDelimiter.Length, StringComparison.Ordinal);
                    inertMatchCount--;
                }
            }

            return insertionIndex;
        }

        private int GetInsertionEndIndex(string insertionText, string trailingSource, string endDelimiter, bool maintainWSOnly) {
            int insertionEndIndex = -1;

            if (maintainWSOnly && String.IsNullOrWhiteSpace(trailingSource)) {
                // trailingSource is all whitespace, and they've indicated they don't want non-whitespace injected into it.
                //    the end delimiter will end at the last -ws in insertionText
                string insertionTextTrimmed = insertionText.TrimEnd();
                insertionEndIndex = insertionTextTrimmed.Length;
            } else {
                if (!String.IsNullOrEmpty(endDelimiter)) {
                    // Find the location in insertionText that matches endDelimiter
                    insertionEndIndex = GetInsertionEndIndexHelper(insertionText, trailingSource, endDelimiter);

                    while ((insertionEndIndex < 0) && (endDelimiter.Length > 1)) {
                        // couldn't find a location, trim off the first char in endDelimiter and try again
                        endDelimiter = endDelimiter.Substring(1, endDelimiter.Length - 1);
                        insertionEndIndex = GetInsertionEndIndexHelper(insertionText, trailingSource, endDelimiter);
                    }
                }
            }

            return insertionEndIndex;
        }

        private static int GetInsertionEndIndexHelper(string insertionText, string trailingSource, string endDelimiter) {
            int insertionIndex = insertionText.LastIndexOf(endDelimiter, StringComparison.Ordinal);

            if (insertionIndex >= 0) {
                // found a match. Determine how many times the delimiter appeared in trailingSource, and skip
                //   that many matches.
                int inertMatchCount = GetMatchCount(trailingSource, endDelimiter);
                while ((insertionIndex >= endDelimiter.Length) && (inertMatchCount > 0)) {
                    insertionIndex = insertionText.LastIndexOf(endDelimiter, insertionIndex - endDelimiter.Length, StringComparison.Ordinal);
                    inertMatchCount--;
                }

                if (insertionIndex >= 0) {
                    insertionIndex += endDelimiter.Length;
                }
            }

            return insertionIndex;
        }

        // Returns the number of times toFind is found within toSearch
        private static int GetMatchCount(string toSearch, string toFind) {
            int matchCount = 0;

            Debug.Assert(!String.IsNullOrEmpty(toFind));
            if (!String.IsNullOrEmpty(toFind)) {
                int index = toSearch.IndexOf(toFind, StringComparison.Ordinal);
                while (index >= 0) {
                    index = toSearch.IndexOf(toFind, index + toFind.Length, StringComparison.Ordinal);
                    matchCount++;
                }
            }

            return matchCount;
        }

        // Moves startPos and endPos to include an appropriate amount of whitespace. The amount of desired whitespace
        //   is determined by inspecting the whitespace at the beginning and end of sourceText.
        private static void EnsureWhitespaceAroundPositions(string insertionText, string sourceText, ref int startPos, ref int endPos) {
            string sourceTextTrimmedStart = sourceText.TrimStart();
            string sourceTextTrimmed = sourceTextTrimmedStart.TrimEnd();
            int leadingWhitespaceCount = sourceText.Length - sourceTextTrimmedStart.Length;
            int trailingWhitespaceCount = sourceTextTrimmedStart.Length - sourceTextTrimmed.Length;

            while ((leadingWhitespaceCount > 0) && (startPos > 0)) {
                if (!Char.IsWhiteSpace(insertionText[startPos - 1]))
                    break;
                startPos -= 1;
                leadingWhitespaceCount -= 1;
            }

            while ((trailingWhitespaceCount > 0) && (endPos < insertionText.Length)) {
                if (!Char.IsWhiteSpace(insertionText[endPos]))
                    break;
                endPos += 1;
                trailingWhitespaceCount -= 1;
            }
        }

        private void UpdateInsertionSizes(ReadOnlyCollection<SnapshotSpan> sourceReplacementSpans,
                                          string insertionText,
                                          int htmlStartPos,
                                          int htmlEndPos,
                                          IList<int> insertionSizes) {
            int claimedCount = 0;
            for (int i = 0; i < sourceReplacementSpans.Count - 1; ++i) {
                SnapshotSpan replacementSpan = sourceReplacementSpans[i];
                if (replacementSpan.Snapshot.TextBuffer != _diskBuffer) {
                    // inert buffer
                    if (claimedCount < htmlStartPos) {
                        insertionSizes[i] = (htmlStartPos - claimedCount);
                    } else if (claimedCount >= htmlEndPos) {
                        insertionSizes[i] = (insertionText.Length - claimedCount);
                    } else {
                        insertionSizes[i] = 0;
                    }
                } else {
                    // html buffer
                    if ((claimedCount >= htmlStartPos) && (claimedCount < htmlEndPos)) {
                        insertionSizes[i] = (htmlEndPos - claimedCount);
                    } else {
                        insertionSizes[i] = 0;
                    }
                }
                claimedCount += insertionSizes[i];
            }

            insertionSizes[sourceReplacementSpans.Count - 1] = insertionText.Length - claimedCount;

            if (tracing) {
                int pos = 0;
                Debug.WriteLine("*** FillInReplacementSizes request: \"" + insertionText + "\"");
                for (int i = 0; i < insertionSizes.Count; i++) {
                    string curText = insertionText.Substring(pos, insertionSizes[i]);
                    string contentType = sourceReplacementSpans[i].Snapshot.TextBuffer.ContentType.ToString();
                    Debug.WriteLine("***\t" + contentType + ": \"" + curText + "\"");

                    pos += insertionSizes[i];
                }
            }
        }

        #endregion
    }
}

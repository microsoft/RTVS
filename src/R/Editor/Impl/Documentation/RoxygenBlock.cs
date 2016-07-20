// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Support.Help;
using Microsoft.VisualStudio.Text;
using static System.FormattableString;

namespace Microsoft.R.Editor.Completion.Documentation {
    internal static class RoxygenBlock {
        /// <summary>
        /// Attempts to insert Roxygen documentation block based
        /// on the user function signature.
        /// </summary>
        public static bool TryInsertBlock(ITextBuffer textBuffer, AstRoot ast, int position) {
            // First determine if position is right before the function declaration
            var snapshot = textBuffer.CurrentSnapshot;
            ITextSnapshotLine line = null;
            var lineNumber = snapshot.GetLineNumberFromPosition(position);
            for (int i = lineNumber; i < snapshot.LineCount; i++) {
                line = snapshot.GetLineFromLineNumber(i);
                if (line.Length > 0) {
                    break;
                }
            }

            if (line == null || line.Length == 0) {
                return false;
            }

            Variable v;
            int offset = line.Length - line.GetText().TrimStart().Length + 1;
            if (line.Start + offset >= snapshot.Length) {
                return false;
            }

            IFunctionDefinition fd = FunctionDefinitionExtensions.FindFunctionDefinition(textBuffer, ast, line.Start + offset, out v);
            if (fd != null && v != null && !string.IsNullOrEmpty(v.Name)) {

                int definitionStart = Math.Min(v.Start, fd.Start);
                Span? insertionSpan = GetRoxygenBlockPosition(snapshot, definitionStart);
                if (insertionSpan.HasValue) {
                    string lineBreak = snapshot.GetLineFromPosition(position).GetLineBreakText();
                    if (string.IsNullOrEmpty(lineBreak)) {
                        lineBreak = "\n";
                    }
                    string block = GenerateRoxygenBlock(v.Name, fd, lineBreak);
                    if (block.Length > 0) {
                        if (insertionSpan.Value.Length == 0) {
                            textBuffer.Insert(insertionSpan.Value.Start, block + lineBreak);
                        } else {
                            textBuffer.Replace(insertionSpan.Value, block);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private static Span? GetRoxygenBlockPosition(ITextSnapshot snapshot, int definitionStart) {
            var line = snapshot.GetLineFromPosition(definitionStart);
            for (int i = line.LineNumber - 1; i >= 0; i--) {
                var currentLine = snapshot.GetLineFromLineNumber(i);
                string lineText = currentLine.GetText().TrimStart();
                if (lineText.Length > 0) {
                    if (lineText.EqualsOrdinal("##")) {
                        return new Span(currentLine.Start, currentLine.Length);
                    } else if (lineText.EqualsOrdinal("#'")) {
                        return null;
                    }
                    break;
                }
            }
            return new Span(line.Start, 0);
        }

        private static string GenerateRoxygenBlock(string functionName, IFunctionDefinition fd, string lineBreak) {
            var sb = new StringBuilder();

            IFunctionInfo fi = fd.MakeFunctionInfo(functionName);
            if (fi != null && fi.Signatures.Count > 0) {

                sb.Append(Invariant($"#' Title{lineBreak}"));
                sb.Append(Invariant($"#'{lineBreak}"));

                int length = sb.Length;
                foreach (var p in fi.Signatures[0].Arguments) {
                    if (!string.IsNullOrEmpty(p.Name)) {
                        sb.Append(Invariant($"#' @param {p.Name}{lineBreak}"));
                    }
                }

                if (sb.Length > length) {
                    sb.Append(Invariant($"#'{lineBreak}"));
                }

                sb.Append(Invariant($"#' @return{lineBreak}"));
                sb.Append(Invariant($"#' @export{lineBreak}"));
                sb.Append(Invariant($"#'{lineBreak}"));
                sb.Append("#' @examples");
            }

            return sb.ToString();
        }

        private static ITextSnapshotLine FindFirstNonEmptyLine(ITextSnapshot snapshot, int lineNumber) {
            for (int i = lineNumber; i < snapshot.LineCount; i++) {
                var line = snapshot.GetLineFromLineNumber(i);
                if (!string.IsNullOrWhiteSpace(line.GetText())) {
                    return line;
                }
            }
            return null;
        }
    }
}

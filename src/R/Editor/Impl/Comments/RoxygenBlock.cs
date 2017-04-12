// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Editor.Functions;
using static System.FormattableString;

namespace Microsoft.R.Editor.Comments {
    public static class RoxygenBlock {
        /// <summary>
        /// Attempts to insert Roxygen documentation block based
        /// on the user function signature.
        /// </summary>
        public static bool TryInsertBlock(IEditorBuffer editorBuffer, AstRoot ast, int position) {
            // First determine if position is right before the function declaration
            var snapshot = editorBuffer.CurrentSnapshot;
            IEditorLine line = null;
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

            var fd = ast.FindFunctionDefinition(line.Start + offset, out v);
            if (fd != null && v != null && !string.IsNullOrEmpty(v.Name)) {

                int definitionStart = Math.Min(v.Start, fd.Start);
                var insertionSpan = GetRoxygenBlockPosition(snapshot, definitionStart);
                if (insertionSpan != null) {
                    string lineBreak = snapshot.GetLineFromPosition(position).LineBreak;
                    if (string.IsNullOrEmpty(lineBreak)) {
                        lineBreak = "\n";
                    }
                    string block = GenerateRoxygenBlock(v.Name, fd, lineBreak);
                    if (block.Length > 0) {
                        if (insertionSpan.Length == 0) {
                            editorBuffer.Insert(insertionSpan.Start, block + lineBreak);
                        } else {
                            editorBuffer.Replace(insertionSpan, block);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private static ITextRange GetRoxygenBlockPosition(IEditorBufferSnapshot snapshot, int definitionStart) {
            var line = snapshot.GetLineFromPosition(definitionStart);
            for (int i = line.LineNumber - 1; i >= 0; i--) {
                var currentLine = snapshot.GetLineFromLineNumber(i);
                string lineText = currentLine.GetText().TrimStart();
                if (lineText.Length > 0) {
                    if (lineText.EqualsOrdinal("##")) {
                        return currentLine;
                    } else if (lineText.EqualsOrdinal("#'")) {
                        return null;
                    }
                    break;
                }
            }
            return new TextRange(line.Start, 0);
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

        private static IEditorLine FindFirstNonEmptyLine(IEditorBufferSnapshot snapshot, int lineNumber) {
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

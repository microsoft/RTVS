// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Functions.Definitions;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Support.Help.Definitions;
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
            ITextSnapshotLine currentLine = textBuffer.CurrentSnapshot.GetLineFromPosition(position);
            var line = FindFirstNonEmptyLine(snapshot, currentLine.LineNumber + 1);
            if (line != null) {
                var exp = ast.GetNodeOfTypeFromPosition<IExpressionStatement>(line.Start + line.Length / 2);
                if (exp != null) {
                    Variable v;

                    IFunctionDefinition fd = exp.GetFunctionDefinition(out v);
                    if (fd != null && v != null && !string.IsNullOrEmpty(v.Name)) {

                        int definitionStart = Math.Min(v.Start, fd.Start);
                        if (!HasRoxygenBlock(snapshot, currentLine.LineNumber, definitionStart)) {

                            string lineBreak = currentLine.GetLineBreakText();
                            if (string.IsNullOrEmpty(lineBreak)) {
                                lineBreak = "\n";
                            }
                            string block = GenerateRoxygenBlock(v.Name, fd, lineBreak);
                            if (block.Length > 0) {
                                textBuffer.Replace(new Span(currentLine.Start, currentLine.Length), block);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static bool HasRoxygenBlock(ITextSnapshot snapshot, int currentlineNumber, int definitionStart) {
            var line = snapshot.GetLineFromPosition(definitionStart);
            for (int i = line.LineNumber - 1; i >= 0; i--) {
                if(i == currentlineNumber) {
                    // Skip line where user just typed ###
                    continue;
                }
                line = snapshot.GetLineFromLineNumber(i);
                if (line.Length > 0) {
                    var lineText = line.GetText().TrimStart();
                    if (lineText.Length > 0) {
                        if (lineText.StartsWith("#'", StringComparison.Ordinal)) {
                            return true;
                        } else {
                            break;
                        }
                    }
                }
            }
            return false;
        }

        private static string GenerateRoxygenBlock(string functionName, IFunctionDefinition fd, string lineBreak) {
            var sb = new StringBuilder();

            IFunctionInfo fi = fd.MakeFunctionInfo(functionName);
            if (fi != null && fi.Signatures.Count > 0) {

                sb.Append(Invariant($"#' Title {lineBreak}"));
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

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Extensions;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Operators;
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
            for (var i = lineNumber; i < snapshot.LineCount; i++) {
                line = snapshot.GetLineFromLineNumber(i);
                if (line.Length > 0) {
                    break;
                }
            }

            if (line == null || line.Length == 0) {
                return false;
            }

            var offset = line.Length - line.GetText().TrimStart().Length + 1;
            var linePosition = line.Start + offset;
            if (linePosition >= snapshot.Length) {
                return false;
            }


            var fd = ast.FindFunctionDefinition(linePosition, out var v);
            FunctionCall fc = null;

            if (fd == null) {
                fc = ast.GetNodeOfTypeFromPosition<FunctionCall>(linePosition);
                var name = fc.GetFunctionName();
                if (string.IsNullOrEmpty(name) || !name.EqualsOrdinal("setClass")) {
                    fc = null;
                }
            }

            if (fd == null && fc == null) {
                return false;
            }

            int definitionStart;
            if (fd != null && v != null) {
                definitionStart = Math.Min(v.Start, fd.Start);
            } else if (fc != null) {
                definitionStart = fc.Start;
            } else {
                return false;
            }

            var insertionSpan = GetRoxygenBlockPosition(snapshot, definitionStart);
            if (insertionSpan == null) {
                return false;
            }

            var lineBreak = snapshot.GetLineFromPosition(position).LineBreak;
            if (string.IsNullOrEmpty(lineBreak)) {
                lineBreak = "\n";
            }

            var block = fd != null
                ? GenerateRoxygenBlock(v.Name, fd, lineBreak)
                : GenerateRoxygenBlock(fc, lineBreak);

            if (block.Length == 0) {
                return false;
            }

            if (insertionSpan.Length == 0) {
                editorBuffer.Insert(insertionSpan.Start, block + lineBreak);
            } else {
                editorBuffer.Replace(insertionSpan, block);
            }
            return true;
        }

        private static ITextRange GetRoxygenBlockPosition(IEditorBufferSnapshot snapshot, int definitionStart) {
            var line = snapshot.GetLineFromPosition(definitionStart);
            for (var i = line.LineNumber - 1; i >= 0; i--) {
                var currentLine = snapshot.GetLineFromLineNumber(i);
                var lineText = currentLine.GetText().TrimStart();
                if (lineText.Length > 0) {
                    if (lineText.EqualsOrdinal("##")) {
                        return currentLine;
                    }
                    if (lineText.EqualsOrdinal("#'")) {
                        return null;
                    }
                    break;
                }
            }
            return new TextRange(line.Start, 0);
        }

        /// <summary>
        /// Generates Roxygen block from function definition.
        /// <seealso cref="IFunctionDefinition"/>
        /// </summary>
        private static string GenerateRoxygenBlock(string functionName, IFunctionDefinition fd, string lineBreak) {
            var sb = new StringBuilder();

            var fi = fd.MakeFunctionInfo(functionName);
            if (fi != null && fi.Signatures.Count > 0) {

                sb.Append(Invariant($"#' Title{lineBreak}"));
                sb.Append(Invariant($"#'{lineBreak}"));

                var length = sb.Length;
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

        /// <summary>
        /// Generates Roxygen block from S3 setClass() call
        /// </summary>
        private static string GenerateRoxygenBlock(FunctionCall fc, string lineBreak) {
            //setClass("Test",
            //    representation(
            //        a = "character",
            //        b = "character",
            //        c = "character"
            //    ))

            // Locate either 'slots' or 'representation' arguments
            var slotsCall = fc.Arguments.Select(x => {
                if (x is ExpressionArgument ea) {
                    var f = ea.ArgumentValue.GetFunctionCall();
                    var name = f.GetFunctionName();
                    if (!string.IsNullOrEmpty(name) &&
                        (name.EqualsOrdinal("slots") || name.EqualsOrdinal("representation"))) {
                        return f;
                    }
                }
                return null;
            }).ExcludeDefault().FirstOrDefault();

            if (slotsCall == null) {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append(Invariant($"#' Title{lineBreak}"));
            sb.Append(Invariant($"#'{lineBreak}"));

            var length = sb.Length;
            foreach (var a in slotsCall.Arguments) {
                var na = a as NamedArgument;
                if (na == null) {
                    continue;
                }

                sb.Append(Invariant($"#' @slot {na.Name}"));
                if (na.DefaultValue != null) {
                    var value = na.Root.TextProvider.GetText(na.DefaultValue);
                    if (!string.IsNullOrEmpty(value)) {
                        sb.Append(Invariant($" {value}"));
                    }
                }
                sb.Append(lineBreak);
            }

            if (sb.Length > length) {
                sb.Append(Invariant($"#'{lineBreak}"));
            }

            sb.Append(Invariant($"#' @return{lineBreak}"));
            sb.Append(Invariant($"#' @export{lineBreak}"));
            sb.Append(Invariant($"#'{lineBreak}"));
            sb.Append("#' @examples");

            return sb.ToString();
        }
    }
}

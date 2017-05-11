// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Arguments;

namespace Microsoft.R.Core.Utility {
    public sealed class AstWriter {
        private int _indent = 0;
        private StringBuilder _sb;
        private AstRoot _ast;

        public string WriteTree(AstRoot ast) {
            _sb = new StringBuilder();
            _indent = 0;
            _ast = ast;

            foreach (IAstNode node in ast.Children) {
                WriteNode(node);
            }

            if (_ast.Errors.Count > 0) {
                _sb.AppendLine();

                foreach (var error in _ast.Errors) {
                    _sb.AppendFormat(CultureInfo.InvariantCulture,
                        "{0} {1} [{2}...{3})\r\n",
                        error.ErrorType.ToString(), error.Location.ToString(),
                        error.Start, error.End);
                }
            }

            string text = _sb.ToString();

            _sb = null;
            _ast = null;

            return text;
        }

        private void WriteNode(IAstNode node) {
            Indent();

            string type = node.GetType().ToString();
            string name = type.Substring(type.LastIndexOf('.') + 1);

            _sb.Append(name);

            string innerType = node.ToString();

            innerType = innerType.Replace("\n", "\\n");
            innerType = innerType.Replace("\r", "\\r");

            int ms = innerType.IndexOf("Microsoft", StringComparison.Ordinal);
            if (ms >= 0) {
                innerType = innerType.Substring(type.LastIndexOf('.') + 1);
            }

            if (innerType != name) {
                _sb.Append("  [");
                _sb.Append(innerType);
                _sb.AppendLine("]");
            } else {
                _sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "  [{0}...{1})", node.Start, node.End));
            }

            _indent++;

            foreach (IAstNode child in node.Children) {
                WriteNode(child);
            }

            if (node is CommaSeparatedList) {
                var csl = node as CommaSeparatedList;
                if (csl.Count > 0 && csl[csl.Count - 1] is StubArgument) {
                    WriteNode(csl[csl.Count - 1]);
                }
            }

            _indent--;
        }

        private void Indent() {
            _sb.Append(' ', _indent * 4);
        }
    }
}

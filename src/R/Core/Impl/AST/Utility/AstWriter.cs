using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Core.Utility
{
    [ExcludeFromCodeCoverage]
    public sealed class AstWriter
    {
        private int _indent = 0;
        private StringBuilder _sb;
        private AstRoot _ast;

        public string WriteTree(AstRoot ast)
        {
            _sb = new StringBuilder();
            _indent = 0;
            _ast = ast;

            foreach (IAstNode node in ast.Children)
            {
                WriteNode(node);
            }

            if(_ast.Errors.Count > 0)
            {
                _sb.AppendLine();

                foreach(var error in _ast.Errors)
                {
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

        private void WriteNode(IAstNode node)
        {
            Indent();

            string type = node.GetType().ToString();
            string name = type.Substring(type.LastIndexOf('.') + 1);

            _sb.Append(name);
            _sb.Append("  [");

            string innerType = node.ToString();

            innerType = innerType.Replace("\n", "\\n");
            innerType = innerType.Replace("\r", "\\r");

            int ms = innerType.IndexOf("Microsoft", StringComparison.Ordinal);
            if (ms >= 0)
            {
                innerType = innerType.Substring(type.LastIndexOf('.') + 1);
            }

            _sb.Append(innerType);
            _sb.AppendLine("]");

            _indent++;

            foreach (IAstNode child in node.Children)
            {
                WriteNode(child);
            }

            _indent--;
        }

        private void Indent()
        {
            _sb.Append(' ', _indent * 4);
        }
    }
}

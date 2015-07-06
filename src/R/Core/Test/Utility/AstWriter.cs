using System;
using System.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Core.Test.Utility
{
    internal class AstWriter
    {
        private int indent = 0;
        private StringBuilder sb;
        private AstRoot ast;

        public string WriteTree(AstRoot ast)
        {
            this.sb = new StringBuilder();
            this.indent = 0;
            this.ast = ast;

            foreach (IAstNode node in ast.Children)
            {
                WriteNode(node);
            }

            string text = this.sb.ToString();

            this.sb = null;
            this.ast = null;

            return text;
        }

        private void WriteNode(IAstNode node)
        {
            Indent();

            string type = node.GetType().ToString();
            string name = type.Substring(type.LastIndexOf('.') + 1);

            this.sb.Append(name);
            this.sb.Append("  [");

            string innerType = node.ToString();

            int ms = innerType.IndexOf("Microsoft", StringComparison.Ordinal);
            if (ms >= 0)
            {
                innerType = innerType.Substring(type.LastIndexOf('.') + 1);
            }

            this.sb.Append(innerType);
            this.sb.AppendLine("]");

            this.indent++;

            foreach (IAstNode child in node.Children)
            {
                WriteNode(child);
            }

            this.indent--;
        }

        private void Indent()
        {
            this.sb.Append(' ', this.indent * 4);
        }
    }
}

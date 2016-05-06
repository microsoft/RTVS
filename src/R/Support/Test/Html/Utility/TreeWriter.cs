// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Html.Core.Tree;
using Microsoft.Html.Core.Tree.Nodes;

namespace Microsoft.Html.Core.Test.Utility {
    [ExcludeFromCodeCoverage]
    internal sealed class TreeWriter {
        private int _indent = 0;
        private StringBuilder _sb;
        private HtmlTree _tree;

        public string WriteTree(HtmlTree tree) {
            _sb = new StringBuilder();
            _indent = 0;
            _tree = tree;

            RootNode root = tree.RootNode;

            foreach (TreeNode node in root.Children) {
                WriteNode(node);
            }

            string text = _sb.ToString();

            _sb = null;
            _tree = null;

            return text;
        }

        private void WriteNode(TreeNode node) {
            if (node is ElementNode)
                WriteElement(node as ElementNode);
            else
                Debug.Assert(false, "Unknown node type");
        }

        private void WriteElement(ElementNode node) {
            WriteTag(node, node.StartTag, false);
            _indent++;

            foreach (ElementNode child in node.Children)
                WriteElement(child);

            _indent--;

            if (node.EndTag != null)
                WriteTag(node, node.EndTag, true);
        }

        private void WriteTag(ElementNode element, TagNode tag, bool endTag) {
            Indent();

            string prefix = _tree.Text.GetText(tag.PrefixRange);
            string colon = (tag.NameToken != null && tag.NameToken.HasColon) ? _tree.Text.GetText(tag.NameToken.ColonRange) : String.Empty;
            string name = _tree.Text.GetText(tag.NameRange);

            if (endTag)
                _sb.Append("</");
            else
                _sb.Append('<');

            _sb.Append(prefix);
            _sb.Append(colon);
            _sb.Append(name);

            foreach (AttributeNode a in tag.Attributes)
                WriteAttribute(a);

            if (tag.IsClosed) {
                if (String.Compare(element.Name, "?xml") == 0)
                    _sb.Append(" ?>");
                else if (tag.IsShorthand)
                    _sb.Append(" />");
                else
                    _sb.Append('>');
            }

            _sb.Append("\r\n");
        }

        private void WriteAttribute(AttributeNode at) {
            _sb.Append(' ');

            string prefix = _tree.Text.GetText(at.PrefixRange);
            string colon = (at.NameToken != null && at.NameToken.HasColon) ? _tree.Text.GetText(at.NameToken.ColonRange) : String.Empty;
            string name = _tree.Text.GetText(at.NameRange);

            _sb.Append(prefix);
            _sb.Append(colon);
            _sb.Append(name);

            if (at.EqualsSignRange.Length > 0)
                _sb.Append('=');

            if (at.HasValue()) {
                string value = _tree.Text.GetText(at.ValueRange);
                _sb.Append(value);
            }

        }

        private void Indent() {
            _sb.Append(' ', _indent * 4);
        }
    }
}

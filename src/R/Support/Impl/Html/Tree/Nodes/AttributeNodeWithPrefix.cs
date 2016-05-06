// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Html.Core.Parser.Tokens;

namespace Microsoft.Html.Core.Tree.Nodes {
    public sealed class AttributeNodeWithPrefix : AttributeNode {
        public AttributeNodeWithPrefix(ElementNode parent, AttributeToken token) :
            base(parent, token) {
            var nameToken = token.NameToken as NameToken;
            Prefix = parent != null ? parent.GetText(nameToken.PrefixRange) : string.Empty;
        }

        /// <summary>
        /// Node prefix
        /// </summary>
        public override string Prefix { get; }

        /// <summary>
        /// Node fully qialified name (prefix:name)
        /// </summary>
        public override string QualifiedName => Prefix + ":" + Name;
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Html.Core.Parser.Tokens;

namespace Microsoft.Html.Core.Tree.Nodes {
    public sealed class AttributeNodeWithPrefix : AttributeNode {
        string _prefix;

        public AttributeNodeWithPrefix(ElementNode parent, AttributeToken token) :
            base(parent, token) {
            var nameToken = token.NameToken as NameToken;

            if (parent != null)
                _prefix = parent.GetText(nameToken.PrefixRange);
            else
                _prefix = String.Empty;
        }

        /// <summary>
        /// Node prefix
        /// </summary>
        public override string Prefix { get { return _prefix; } }

        /// <summary>
        /// Node fully qialified name (prefix:name)
        /// </summary>
        public override string QualifiedName { get { return _prefix + ":" + Name; } }
    }
}

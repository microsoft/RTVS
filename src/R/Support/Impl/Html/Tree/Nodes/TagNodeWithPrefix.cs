// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Html.Core.Parser.Tokens;

namespace Microsoft.Html.Core.Tree.Nodes {
    public class TagNodeWithPrefix : TagNode {
        string _prefix;

        public override string Prefix { get { return _prefix; } }
        public override string QualifiedName { get { return _prefix + ":" + Name; } }

        public TagNodeWithPrefix(ElementNode parent, int openAngleBracketPosition, NameToken nameToken, int maxEnd)
            : base(parent, openAngleBracketPosition, nameToken, maxEnd) {
            _prefix = nameToken.HasPrefix() ? parent.GetText(nameToken.PrefixRange) : String.Empty;
        }
    }
}

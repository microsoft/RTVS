// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.Definitions;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Navigation.Peek {
    internal sealed class UserDefinedPeekItem : PeekItemBase {
        public UserDefinedPeekItem(string fileName, IAstNode definitionNode, string name, IPeekResultFactory peekResultFactory) :
            base(name, peekResultFactory) {
            DefinitionNode = definitionNode;
            FileName = fileName;
        }

        public override IPeekResultSource GetOrCreateResultSource(string relationshipName) {
            return new UserDefinedItemPeekResultSource(this);
        }

        internal IAstNode DefinitionNode { get; }
        internal string FileName { get; }
    }
}

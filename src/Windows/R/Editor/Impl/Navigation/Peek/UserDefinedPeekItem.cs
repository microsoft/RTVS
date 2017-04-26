// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.R.Core.AST;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Navigation.Peek {
    /// <summary>
    /// Peekable item for user-defined functions and variables
    /// </summary>
    internal sealed class UserDefinedPeekItem : PeekItemBase {
        public UserDefinedPeekItem(string fileName, IAstNode definitionNode, string name, IPeekResultFactory peekResultFactory, IServiceContainer services) :
            base(name, peekResultFactory, services) {
            DefinitionNode = definitionNode;
            FileName = fileName;
        }

        public override IPeekResultSource GetOrCreateResultSource(string relationshipName) => new UserDefinedItemPeekResultSource(this);

        internal IAstNode DefinitionNode { get; }
        internal string FileName { get; }
    }
}

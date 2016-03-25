// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Navigation.Peek {
    internal sealed class PeekItem : IPeekableItem {
        public PeekItem(string fileName, IAstNode definitionNode, string name, IPeekResultFactory peekResultFactory) {
            DefinitionNode = definitionNode;
            PeekResultFactory = peekResultFactory;
            DisplayName = name;
            FileName = fileName;
        }

        public string DisplayName { get; }

        public IEnumerable<IPeekRelationship> Relationships {
            get { yield return PredefinedPeekRelationships.Definitions; }
        }

        public IPeekResultSource GetOrCreateResultSource(string relationshipName) {
            return new PeekResultSource(this);
        }

        internal IPeekResultFactory PeekResultFactory { get; }
        internal IAstNode DefinitionNode { get; }
        internal string FileName { get; }
    }
}

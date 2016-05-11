// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Navigation.Peek {
    internal abstract class PeekItemBase : IPeekableItem {
        public PeekItemBase(string name, IPeekResultFactory peekResultFactory) {
            PeekResultFactory = peekResultFactory;
            DisplayName = name;
        }

        public string DisplayName { get; }

        public IEnumerable<IPeekRelationship> Relationships {
            get { yield return PredefinedPeekRelationships.Definitions; }
        }

        public abstract IPeekResultSource GetOrCreateResultSource(string relationshipName);

        internal IPeekResultFactory PeekResultFactory { get; }
    }
}

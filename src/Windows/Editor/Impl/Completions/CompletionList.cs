// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Microsoft.Languages.Editor.Completions {
    using Completion = Microsoft.VisualStudio.Language.Intellisense.Completion;
    public sealed class CompletionList : List<Completion>, INotifyCollectionChanged {
        // Once this list starts being used by completion, it never changes, so don't fire the event
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public CompletionList(IEnumerable<Completion> completions)
            : base(completions) {
            // This makes the compiler happy about the unused event
            Debug.Assert(CollectionChanged == null);
        }

        public void FireCollectionChanged() => CollectionChanged?.Invoke(this, null);
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.Languages.Editor.Completions {
    public sealed class CompletionList : List<Completion>, INotifyCollectionChanged {
        // Once this list starts being used by completion, it never changes, so don't fire the event
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public CompletionList(IEnumerable<ICompletionEntry> completions) :
            base(completions.Select(c => new Completion(c.DisplayText, c.InsertionText, c.Description, (ImageSource)c.ImageSource, c.AccessibleText))) { }

        public void FireCollectionChanged() => CollectionChanged?.Invoke(this, null);
    }
}

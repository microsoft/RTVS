// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Languages.Editor.Completions {
    public sealed class CompletionEntryComparer : IComparer<ICompletionEntry> {
        private readonly StringComparison _comparison;

        public CompletionEntryComparer(StringComparison comparison) {
            _comparison = comparison;
        }
        public int Compare(ICompletionEntry x, ICompletionEntry y) {
            if (x == null || y == null) {
                return -1;
            }

            return string.Compare(x.DisplayText, y.DisplayText, _comparison);
        }
    }
}

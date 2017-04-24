// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.History.Implementation {
    internal sealed partial class RHistory {
        private interface IEntrySelector {
            void EntriesSelected();
            void TextSelected();
        }
    }
}
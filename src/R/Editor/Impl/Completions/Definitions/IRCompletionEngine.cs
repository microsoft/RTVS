// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Editor.Completions;

namespace Microsoft.R.Editor.Completions.Engine {
    public interface IRCompletionEngine {
        IReadOnlyCollection<IRCompletionListProvider> GetCompletionForLocation(IRIntellisenseContext context);
    }
}

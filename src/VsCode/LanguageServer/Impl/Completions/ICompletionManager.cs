// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LanguageServer.VsCode.Contracts;
using Microsoft.R.LanguageServer.Documents;

namespace Microsoft.R.LanguageServer.Completions {
    internal interface ICompletionManager {
        CompletionList GetCompletions(DocumentEntry entry, int position);
    }
}

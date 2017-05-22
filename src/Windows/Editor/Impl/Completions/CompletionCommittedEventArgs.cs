// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.Languages.Editor.Completions {
    public sealed class CompletionCommittedEventArgs {
        public ICompletionSession Session { get; }
        public CompletionCommittedEventArgs(ICompletionSession session) => Session = session;
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using LanguageServer.VsCode.Contracts;
using Microsoft.R.LanguageServer.Server.Documents;

namespace Microsoft.R.LanguageServer.Text {
    internal interface ITextManager {
        void ProcessTextChanges(DocumentEntry entry, ICollection<TextDocumentContentChangeEvent> contentChanges);
    }
}

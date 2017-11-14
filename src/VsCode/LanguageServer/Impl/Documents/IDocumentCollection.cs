// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.LanguageServer.Documents {
    /// <summary>
    /// Represents collection of R documents opened in VS Code
    /// </summary>
    internal interface IDocumentCollection {
        void AddDocument(string content, Uri uri);
        void RemoveDocument(Uri uri);
        DocumentEntry GetDocument(Uri uri);
    }
}

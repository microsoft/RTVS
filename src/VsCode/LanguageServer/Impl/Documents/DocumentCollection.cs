// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;

namespace Microsoft.R.LanguageServer.Documents {
    /// <inheritdoc />
    internal sealed class DocumentCollection: IDocumentCollection {
        private readonly ConcurrentDictionary<Uri, DocumentEntry> _documents = new ConcurrentDictionary<Uri, DocumentEntry>();
        private readonly IServiceContainer _services;

        public DocumentCollection(IServiceContainer services) {
            Check.ArgumentNull(nameof(services), services);
            _services = services;
        }

        public void AddDocument(string content, Uri uri) {
            Check.InvalidOperation(() => !_documents.ContainsKey(uri));
            var entry = new DocumentEntry(content, uri, _services);
            _documents[uri] = entry;
        }

        public void RemoveDocument(Uri uri) {
            if (_documents.TryGetValue(uri, out var doc)) {
                doc.Dispose();
                _documents.TryRemove(uri, out var _);
            }
        }

        public DocumentEntry GetDocument(Uri uri) 
            => _documents.TryGetValue(uri, out var doc) ? doc : null;
    }
}

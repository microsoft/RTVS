// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;

namespace Microsoft.R.LanguageServer.Server.Documents {
    internal sealed class DocumentCollection {
        private static DocumentCollection _instance;

        private readonly Dictionary<Uri, DocumentEntry> _documents = new Dictionary<Uri, DocumentEntry>();
        private readonly IServiceContainer _services;

        public static DocumentCollection GetOrCreate(IServiceContainer services)
            => _instance ?? (_instance = new DocumentCollection(services));

        private DocumentCollection(IServiceContainer services) {
            Check.ArgumentNull(nameof(services), services);
            _services = services;
        }

        public void AddDocument(string content, Uri uri) {
            Check.InvalidOperation(() => !_documents.ContainsKey(uri));
            var entry = new DocumentEntry(content, _services);
            _documents[uri] = entry;
        }

        public void RemoveDocument(Uri uri) {
            if (_documents.TryGetValue(uri, out DocumentEntry doc)) {
                doc.Dispose();
                _documents.Remove(uri);
            }
        }

        public DocumentEntry GetDocument(Uri uri) 
            => _documents.TryGetValue(uri, out DocumentEntry doc) ? doc : null;
    }
}

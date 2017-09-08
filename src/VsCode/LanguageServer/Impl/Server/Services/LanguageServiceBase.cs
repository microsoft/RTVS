// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using JsonRpc.Standard.Server;
using LanguageServer.VsCode.Contracts;
using LanguageServer.VsCode.Contracts.Client;
using LanguageServer.VsCode.Server;

namespace Microsoft.R.LanguageServer.Server {
    public abstract class LanguageServiceBase : JsonRpcService {
        protected LanguageServerSession Session => RequestContext.Features.Get<LanguageServerSession>();

        protected ClientProxy Client => Session.Client;

        protected TextDocument GetDocument(Uri uri) 
            => Session.Documents.TryGetValue(uri, out var sd) ? sd.Document : null;

        protected TextDocument GetDocument(TextDocumentIdentifier id) => GetDocument(id.Uri);

    }
}

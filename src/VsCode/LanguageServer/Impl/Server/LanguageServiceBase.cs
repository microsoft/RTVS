// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using JsonRpc.Standard.Server;
using LanguageServer.VsCode.Contracts.Client;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.R.LanguageServer.Services;

namespace Microsoft.R.LanguageServer.Server {
    public abstract class LanguageServiceBase : JsonRpcService {
        protected LanguageServerSession LanguageServerSession => RequestContext.Features.Get<LanguageServerSession>();

        protected ClientProxy Client => LanguageServerSession.Client;
         protected IServiceContainer Services => CoreShell.Current.Services;
        protected IMainThread MainThread => Services.MainThread();
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Test.Utility;

namespace Microsoft.VisualStudio.R.Package.Test {
    public class HostBasedInteractiveTest : InteractiveTest {
        protected VsRHostScript HostScript { get; }

        public HostBasedInteractiveTest(IServiceContainer services, IRSessionCallback callback = null): base(services) {
            HostScript = new VsRHostScript(SessionProvider, callback);
        }

        public HostBasedInteractiveTest(IServiceContainer services, bool async, IRSessionCallback callback = null): base(services) {
            HostScript = new VsRHostScript(SessionProvider, async, callback);
        }

        protected Task InitializeAsync() => HostScript.InitializeAsync();

        protected override void Dispose(bool disposing) {
            HostScript.Dispose();
            base.Dispose(disposing);
        }
    }
}

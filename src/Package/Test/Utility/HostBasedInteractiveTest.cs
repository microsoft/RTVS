// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Test.Utility;

namespace Microsoft.VisualStudio.R.Package.Test {
    public class HostBasedInteractiveTest : InteractiveTest {
        protected VsRHostScript HostScript { get; }

        public HostBasedInteractiveTest(IRSessionCallback callback = null) {
            HostScript = new VsRHostScript(SessionProvider, callback);
        }

        public HostBasedInteractiveTest(bool async, IRSessionCallback callback = null) {
            HostScript = new VsRHostScript(SessionProvider, async, callback);
        }

        protected Task InitializeAsync() {
            return HostScript.InitializeAsync();
        }

        protected override void Dispose(bool disposing) {
            HostScript.Dispose();
            base.Dispose(disposing);
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.VisualStudio.R.Package.Test.Utility;

namespace Microsoft.VisualStudio.R.Package.Test {
    public class HostBasedInteractiveTest : InteractiveTest {
        private readonly IRSessionCallback _callback;

        protected RHostScript HostScript { get; }
        protected T GetScript<T>() where T : RHostScript => HostScript as T;

        public HostBasedInteractiveTest(IServiceContainer services, IRSessionCallback callback = null) : base(services) {
            _callback = callback;
            HostScript = new VsRHostScript(services, callback);
        }

        public HostBasedInteractiveTest(RHostScript script, IServiceContainer services, IRSessionCallback callback = null): base(services) {
            _callback = callback;
            HostScript = script;
        }

        public HostBasedInteractiveTest(IServiceContainer services, bool async, IRSessionCallback callback = null): base(services) {
            _callback = callback;
            HostScript = new VsRHostScript(services, async);
        }

        public override Task InitializeAsync() => HostScript.InitializeAsync(_callback);

        public override Task DisposeAsync() {
            HostScript.Dispose();
            return base.DisposeAsync();
        }
    }
}

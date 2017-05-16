// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Script;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Utility {
    [ExcludeFromCodeCoverage]
    public sealed class VsRHostScript : RHostScript {
        public VsRHostScript(IServiceContainer services, IRSessionCallback clientApp = null)
            : base(services, clientApp) { }

        public VsRHostScript(IServiceContainer services, bool async)
            : base(services, async) { }

        public static void DoIdle(int ms) {
            UIThreadHelper.Instance.Invoke(() => {
                var idle = VsAppShell.Current.GetService<IIdleTimeSource>();
                int time = 0;
                while (time < ms) {
                    TestScript.DoEvents();
                    idle.DoIdle();

                    Thread.Sleep(20);
                    time += 20;
                }
            });
        }
    }
}

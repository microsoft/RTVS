// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Common.Core.Test.Script;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Utility {
    [ExcludeFromCodeCoverage]
    public sealed class VsRHostScript : RHostScript {
        public VsRHostScript(IRSessionProvider sessionProvider, IRSessionCallback clientApp = null)
            : base(sessionProvider, clientApp) { }

        public VsRHostScript(IRSessionProvider sessionProvider, bool async, IRSessionCallback clientApp)
            : base(sessionProvider, async, clientApp) { }

        public static void DoIdle(int ms) {
            UIThreadHelper.Instance.Invoke(() => {
                int time = 0;
                while (time < ms) {
                    TestScript.DoEvents();
                    VsAppShell.Current.DoIdle();

                    Thread.Sleep(20);
                    time += 20;
                }
            });
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Utility;

namespace Microsoft.VisualStudio.R.Package.Test {
    public class InteractiveTest: IDisposable {
        protected IRSessionProvider SessionProvider { get; }

        public InteractiveTest() {
            SessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing) { }

        public static void DoIdle(int ms) {
            UIThreadHelper.Instance.Invoke(() => {
                int time = 0;
                while (time < ms) {
                    IdleTime.DoIdle();
                    Thread.Sleep(20);
                    time += 20;
                }
            });
        }
    }
}

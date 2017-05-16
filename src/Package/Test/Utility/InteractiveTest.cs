// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test {
    public class InteractiveTest: IAsyncLifetime {
        protected IRSessionProvider SessionProvider { get; }
        protected IServiceContainer Services { get; }

        public InteractiveTest(IServiceContainer services) {
            Services = services;
            var workflow = services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
            SessionProvider = workflow.RSessions;
        }

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

        public virtual Task InitializeAsync() => Task.CompletedTask;
        public virtual Task DisposeAsync() => Task.CompletedTask;
    }
}

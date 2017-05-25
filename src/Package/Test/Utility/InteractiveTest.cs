// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Threading;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test {
    public class InteractiveTest: IAsyncLifetime {
        protected IRInteractiveWorkflow Workflow { get; }
        protected IRSessionProvider SessionProvider { get; }
        protected IServiceContainer Services { get; }

        public InteractiveTest(IServiceContainer services) {
            Services = services;
            Workflow = services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
            SessionProvider = Workflow.RSessions;
        }

        protected void DoIdle(int ms) {
            UIThreadHelper.Instance.Invoke(() => {
                var idle = Services.GetService<IIdleTimeSource>();
                int time = 0;
                while (time < ms) {
                    idle.DoIdle();
                    Thread.Sleep(20);
                    time += 20;
                }
            });
        }

        public virtual Task InitializeAsync() => Task.CompletedTask;
        public virtual Task DisposeAsync() => Task.CompletedTask;
    }
}

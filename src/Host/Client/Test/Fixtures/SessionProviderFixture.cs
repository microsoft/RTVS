// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.R.Host.Client.Session;
using Xunit;

namespace Microsoft.R.Host.Client.Test.Fixtures {
    [ExcludeFromCodeCoverage]
    // Fixture doesn't import itself. Use AssemblyFixtureImportAttribute
    public sealed class SessionProviderFixture: IAsyncLifetime {
        public IRSessionProvider SessionProvider { get; }

        public SessionProviderFixture() {
            SessionProvider = new RSessionProvider();
        }

        public async Task InitializeAsync() {
            await SessionProvider.TrySwitchBrokerAsync(nameof(SessionProviderFixture));
        }

        public Task DisposeAsync() {
            SessionProvider.Dispose();
            return Task.CompletedTask;
        }
    }
}

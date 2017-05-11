// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public static class VsSettingsManagerMock {
        public static IVsSettingsManager Create() {
            IVsSettingsManager sm = Substitute.For<IVsSettingsManager>();

            string s;
            sm.GetApplicationDataFolder(Arg.Any<uint>(), out s).ReturnsForAnyArgs(x => {
                x[1] = Path.GetTempPath();
                return VSConstants.S_OK;
            });

            IVsSettingsStore store;
            sm.GetReadOnlySettingsStore(Arg.Any<uint>(), out store).ReturnsForAnyArgs(x => {
                x[1] = VsSettingsStoreMock.Create();
                return VSConstants.S_OK;
            });

            IVsWritableSettingsStore writable;
            sm.GetWritableSettingsStore(Arg.Any<uint>(), out writable).ReturnsForAnyArgs(x => {
                x[1] = VsSettingsStoreMock.Create();
                return VSConstants.S_OK;
            });

            return sm;
        }
    }
}

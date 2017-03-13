// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.OS {
    public class Win32ProcessExitEventArgs : EventArgs {
        private readonly uint _exitCode;
        public uint ExitCode => _exitCode;
        public Win32ProcessExitEventArgs(uint exitCode) {
            _exitCode = exitCode;
        }

        public bool HasError() {
            return _exitCode != 0;
        }
    }
}

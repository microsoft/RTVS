// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;

namespace Microsoft.Common.Core.OS {
    public interface IProcess : IDisposable {
        int Id { get; }
        StreamWriter StandardInput { get; }
        StreamReader StandardOutput { get; }
        StreamReader StandardError { get; }
        bool HasExited { get; }
        int ExitCode { get; }

        event EventHandler Exited;

        void Kill();
        bool WaitForExit(int milliseconds);
    }
}
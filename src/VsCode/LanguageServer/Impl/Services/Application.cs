// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class Application : IApplication, IDisposable {
        public string Name => "VsCode-R";
        public int LocaleId => 1033;
        public void Dispose() => Terminating?.Invoke(this, EventArgs.Empty);

        #pragma warning disable 67
        public event EventHandler Terminating;
        public event EventHandler Started;
    }
}

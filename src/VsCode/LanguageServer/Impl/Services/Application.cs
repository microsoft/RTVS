// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class Application : IApplication {
        public string Name => "VsCode-R";
        public int LocaleId => 1033;
        public string ApplicationFolder => ".";
        public string ApplicationDataFolder => ".";

        public event EventHandler Started;
        public event EventHandler Terminating;
    }
}

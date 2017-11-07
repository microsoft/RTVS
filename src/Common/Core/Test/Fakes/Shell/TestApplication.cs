// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using Microsoft.Common.Core.Shell;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    public sealed class TestApplication : IApplication {
        public string Name => "RTVS_Test";
        public int LocaleId => 1033;

#pragma warning disable 67
        public event EventHandler Started;
        public event EventHandler Terminating;
    }
}

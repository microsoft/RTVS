// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using Microsoft.Common.Core.Shell;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    internal sealed class TestApplication : IApplication {
        public string Name => "RTVS_Test";
        public int LocaleId => 1033;

        public string ApplicationDataFolder {
            get {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(appData, @"Microsoft\RTVS");
            }
        }

        public string ApplicationFolder {
            get {
                var asmPath = Assembly.GetExecutingAssembly().GetAssemblyPath();
                return Path.GetDirectoryName(asmPath);
            }
        }

#pragma warning disable 67
        public event EventHandler Started;
        public event EventHandler Terminating;
    }
}

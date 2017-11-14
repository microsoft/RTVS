// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Microsoft.Common.Core.Shell;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    [ExcludeFromCodeCoverage]
    public sealed class TestPlatformServices : IPlatformServices {
        public IntPtr ApplicationWindowHandle => IntPtr.Zero;
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
    }
}

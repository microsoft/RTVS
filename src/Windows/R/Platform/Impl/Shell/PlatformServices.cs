// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Platform.Shell {
    internal sealed class PlatformServices : IPlatformServices {
        private string _appDataFolder;

        public IntPtr ApplicationWindowHandle => IntPtr.Zero;

        public string ApplicationDataFolder {
            get {
                if (_appDataFolder == null) {
                    string folder = @".\";
                    NativeMethods.SHGetKnownFolderPath(NativeMethods.KNOWNFOLDERID_LocalAppData, 0, IntPtr.Zero, out var path);
                    if (path != IntPtr.Zero) {
                        folder = Marshal.PtrToStringUni(path);
                        Marshal.FreeCoTaskMem(path);
                    }
                    _appDataFolder = Path.Combine(folder, @"Microsoft\RTVS");
                }
                return _appDataFolder;
            }
        }

        public string ApplicationFolder {
            get {
                var asmPath = typeof(PlatformServices).GetTypeInfo().Assembly.GetAssemblyPath();
                return Path.GetDirectoryName(asmPath);
            }
        }
    }
}

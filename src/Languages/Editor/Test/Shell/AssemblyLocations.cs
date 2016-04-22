// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.Win32;

namespace Microsoft.Languages.Editor.Test.Shell {
    public static class AssemblyLocations {
        private static string _idePath;

        public static string IdePath {
            get {
                if(_idePath == null) {
                    _idePath = GetHostExePath();
                }
                return _idePath;
            }
        }

        public static string EditorPath {
            get { return Path.Combine(IdePath, @"CommonExtensions\Microsoft\Editor"); }
        }

        public static string PrivatePath {
            get { return Path.Combine(IdePath, @"PrivateAssemblies\"); }
        }

        public static string CpsPath {
            get { return Path.Combine(IdePath, @"CommonExtensions\Microsoft\Project"); }
        }

        public static string SharedPath {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Common Files\Microsoft Shared\MsEnv\PublicAssemblies"); }
        }

        private static string GetHostExePath() {
            string path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\" + GetHostVersion(), "InstallDir", string.Empty) as string;
            return path;
        }

        private static string GetHostVersion() {
            string version = Environment.GetEnvironmentVariable("ExtensionsVSVersion");
            foreach (string checkVersion in new string[] { VsVersion.Version }) {
                if (string.IsNullOrEmpty(version)) {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\" + checkVersion)) {
                        if (key != null) {
                            version = checkVersion;
                        }
                    }
                }
            }
            return version;
        }
    }
}

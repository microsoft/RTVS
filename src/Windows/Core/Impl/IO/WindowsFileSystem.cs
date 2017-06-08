// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Common.Core.IO {
    public sealed class WindowsFileSystem : FileSystem {
        public override string GetDownloadsPath(string fileName) => 
            string.IsNullOrWhiteSpace(fileName) 
                ? GetKnownFolderPath(KnownFolderGuids.Downloads) 
                : Path.Combine(GetKnownFolderPath(KnownFolderGuids.Downloads), fileName);

        private string GetKnownFolderPath(string knownFolder) {
            var flags = (uint)NativeMethods.KnownFolderflags.KF_FLAG_DEFAULT_PATH;
            var result = NativeMethods.SHGetKnownFolderPath(new Guid(knownFolder), flags, IntPtr.Zero, out IntPtr knownFolderPath);
            return result >= 0 ? Marshal.PtrToStringUni(knownFolderPath) : string.Empty;
        }

        private static class NativeMethods {
            public const int MAX_PATH = 260;

            [Flags]
            public enum KnownFolderflags : uint {
                KF_FLAG_DEFAULT = 0x00000000,
                KF_FLAG_SIMPLE_IDLIST = 0x00000100,
                KF_FLAG_NOT_PARENT_RELATIVE = 0x00000200,
                KF_FLAG_DEFAULT_PATH = 0x00000400,
                KF_FLAG_INIT = 0x00000800,
                KF_FLAG_NO_ALIAS = 0x00001000,
                KF_FLAG_DONT_UNEXPAND = 0x00002000,
                KF_FLAG_DONT_VERIFY = 0x00004000,
                KF_FLAG_CREATE = 0x00008000,
                KF_FLAG_NO_APPCONTAINER_REDIRECTION = 0x00010000,
                KF_FLAG_ALIAS_ONLY = 0x80000000,
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern uint GetLongPathName([MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
                                                      [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuffer,
                                                      int nBufferLength);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern uint GetShortPathName([MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
                                                       [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuffer,
                                                       int nBufferLength);

            [DllImport("Shell32.dll")]
            public static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
                                                          uint dwFlags,
                                                          IntPtr hToken,
                                                          out IntPtr ppszPath);
        }
    }
}
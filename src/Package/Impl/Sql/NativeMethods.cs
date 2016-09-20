// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal static class NativeMethods {
        [DllImport("ODBCCP32.DLL", CharSet = CharSet.Unicode)]
        public static extern bool SQLConfigDataSource(IntPtr hwndParent, RequestFlags fRequest, string lpszDriver, string lpszAttributes);

        [DllImport("ODBCCP32.DLL", CharSet = CharSet.Unicode)]
        public static extern bool SQLManageDataSources(IntPtr hwndParent);

        public enum RequestFlags : ushort {
            ODBC_ADD_DSN = 1,
            ODBC_CONFIG_DSN = 2,
            ODBC_REMOVE_DSN = 3,
            ODBC_ADD_SYS_DSN = 4,
            ODBC_CONFIG_SYS_DSN = 5,
            ODBC_REMOVE_SYS_DSN = 6,
            ODBC_REMOVE_DEFAULT_DSN = 7
        }
    }
}

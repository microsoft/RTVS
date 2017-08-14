// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Common.Core;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Client {
    public static class AboutHostExtensions {
        private static readonly Version _localVersion;

        static AboutHostExtensions() {
            _localVersion = typeof(AboutHost).GetTypeInfo().Assembly.GetName().Version;
        }

        public static string IsHostVersionCompatible(this AboutHost aboutHost) {
            if (_localVersion.MajorRevision != 0 || _localVersion.MinorRevision != 0) { // Filter out debug builds
                var serverVersion = new Version(aboutHost.Version.Major, aboutHost.Version.Minor);
                var clientVersion = new Version(_localVersion.Major, _localVersion.Minor);
                if (serverVersion > clientVersion) {
                    return Resources.Error_RemoteVersionHigher.FormatInvariant(aboutHost.Version, _localVersion);
                }

                if (serverVersion < clientVersion) {
                    return Resources.Error_RemoteVersionLower.FormatInvariant(aboutHost.Version, _localVersion);
                }
            }

            return null;
        }
    }
}
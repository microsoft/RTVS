// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using static System.FormattableString;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    internal class Connection : ConnectionInfo, IConnection {
        private const int DefaultPort = 5444;

        public Connection(IConnectionInfo ci) :
            this(ci.Name, ci.UserProvidedPath, ci.RCommandLineArguments, ci.LastUsed, ci.IsUserCreated) { }

        public Connection(string name, string userProvidedPath, string rCommandLineArguments, DateTime lastUsed, bool isUserCreated) :
            base(name, userProvidedPath, rCommandLineArguments, lastUsed, isUserCreated) {

            if (string.IsNullOrEmpty(UserProvidedPath) && !string.IsNullOrEmpty(Path)) {
                UserProvidedPath = Path;
            } else {
                Path = ToCompletePath(userProvidedPath);
            }

            Id = new Uri(Path);
            IsRemote = !Id.IsFile;
        }

        public Uri Id { get; }

        /// <summary>
        /// If true, the connection is to a remote machine
        /// </summary>
        public bool IsRemote { get; }

        public override string UserProvidedPath {
            get { return base.UserProvidedPath; }
            set {
                base.UserProvidedPath = value;
                Path = ToCompletePath(value);
            }
        }

        public static string ToCompletePath(string path) {
            // https://foo:5444 -> https://foo:5444 (no change)
            // https://foo -> https://foo (no change)
            // http://foo -> http://foo (no change)
            // foo->https://foo:5444

            Uri uri = null;
            try {
                Uri.TryCreate(path, UriKind.Absolute, out uri);
            } catch (InvalidOperationException) { } catch (ArgumentException) { } catch (UriFormatException) { }

            if (uri == null || !(uri.IsFile || string.IsNullOrEmpty(uri.Host))) {
                bool hasScheme = uri != null && !string.IsNullOrEmpty(uri.Scheme);
                bool hasPort = uri != null && uri.Port >= 0;

                if (hasScheme) {
                    if (hasPort) {
                        return Invariant($"{uri.Scheme}{Uri.SchemeDelimiter}{uri.Host}:{uri.Port}");
                    }
                    return Invariant($"{uri.Scheme}{Uri.SchemeDelimiter}{uri.Host}");
                } else {
                    if (Uri.CheckHostName(path) != UriHostNameType.Unknown) {
                        var port = hasPort ? uri.Port : DefaultPort;
                        return Invariant($"{Uri.UriSchemeHttps}{Uri.SchemeDelimiter}{path}:{port}");
                    }
                }
            }
            return path;
        }
    }
}
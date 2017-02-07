// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Common.Core.Security {
    public sealed class Credentials : ICredentials {
        // Although NetworkCredential does support SecureString, it still exposes
        // plain text password via property and it is visible in a debugger.
        public string UserName { get; set; }
        public SecureString Password { get; set; }

        private CredentialSource Source { get; set; }

        private enum CredentialSource {
            Saved, // Obtained from Credential manager
            NewSave, // Obtained from user with save flag set
            NewNoSave // Obtained from user with out save flag set
        }

        public bool CanSave() {
            return Source != CredentialSource.NewNoSave;
        }

        public bool IsSaved() {
            return Source == CredentialSource.Saved;
        }

        /// <summary>
        /// Used for credentials obtained from the user via prompt, with 'save'.
        /// </summary>
        public static Credentials CreateCredentials(string userName, SecureString password, bool save = false) {
            return Create(userName, password, save ? CredentialSource.NewSave : CredentialSource.NewNoSave);
        }

        private static Credentials Create(string userName, SecureString password, CredentialSource source) {
            var creds = new Credentials { UserName = userName, Password = password, Source = source };
            creds.Password.MakeReadOnly();
            return creds;
        }

        public NetworkCredential GetCredential(Uri uri, string authType) {
            return new NetworkCredential(UserName, Password);
        }
    }
}

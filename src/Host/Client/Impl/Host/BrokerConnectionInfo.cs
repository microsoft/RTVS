// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Security;

namespace Microsoft.R.Host.Client.Host {
    [DebuggerDisplay("{Uri}, IsUrlBased={IsUrlBased}, InterpreterId={InterpreterId}")]
    public struct BrokerConnectionInfo {
        public string Name { get; }
        public Uri Uri { get; }
        public bool IsValid { get; }
        public bool IsUrlBased { get; }
        public string ParametersId { get; }
        public string RCommandLineArguments { get; }
        public string InterpreterId { get; }
        public string CredentialAuthority => GetCredentialAuthority(Name);
        public bool FetchHostLoad { get; }

        public static BrokerConnectionInfo Create(ISecurityService securityService, string name, string path, string rCommandLineArguments, bool fetchHostLoad) {
            rCommandLineArguments = rCommandLineArguments ?? string.Empty;

            if (!Uri.TryCreate(path, UriKind.Absolute, out Uri uri)) {
                return new BrokerConnectionInfo();
            }

            return uri.IsFile 
                ? new BrokerConnectionInfo(name, uri, rCommandLineArguments, string.Empty, false, string.Empty, fetchHostLoad) 
                : CreateRemote(name, uri, securityService, rCommandLineArguments, fetchHostLoad);
        }

        private static BrokerConnectionInfo CreateRemote(string name, Uri uri, ISecurityService securityService, string rCommandLineArguments, bool fetchHostLoad) {
            var fragment = uri.Fragment;
            var interpreterId = string.IsNullOrEmpty(fragment) ? string.Empty : fragment.Substring(1);
            var ub = new UriBuilder(uri) {
                Fragment = null,
                UserName = null,
                Password = null
            };
            uri = ub.Uri;
            var (username, _) = securityService.ReadUserCredentials(GetCredentialAuthority(name));
            return new BrokerConnectionInfo(name, uri, rCommandLineArguments, interpreterId, true, username, fetchHostLoad);
        }

        private BrokerConnectionInfo(string name, Uri uri, string rCommandLineArguments, string interpreterId, bool isUrlBased, string username, bool fetchHostLoad) {
            Name = name;
            IsValid = true;
            Uri = uri;
            RCommandLineArguments = rCommandLineArguments?.Trim() ?? string.Empty;
            InterpreterId = interpreterId;
            ParametersId = string.IsNullOrEmpty(rCommandLineArguments) && string.IsNullOrEmpty(interpreterId) && string.IsNullOrEmpty(username)
                ? string.Empty 
                : $"{rCommandLineArguments}/{interpreterId}/{username}".GetSHA256FileSystemSafeHash();
            IsUrlBased = isUrlBased;
            FetchHostLoad = fetchHostLoad;
        }

        public static string GetCredentialAuthority(string name) => $"RTVS:{name}";

        public override bool Equals(object obj) => obj is BrokerConnectionInfo && Equals((BrokerConnectionInfo)obj);

        public bool Equals(BrokerConnectionInfo other) => other.ParametersId.EqualsOrdinal(ParametersId) && Equals(other.Uri, Uri);

        public override int GetHashCode() {
            unchecked {
                return ((ParametersId?.GetHashCode() ?? 0)*397) ^ (Uri != null ? Uri.GetHashCode() : 0);
            }
        }

        public static bool operator ==(BrokerConnectionInfo a, BrokerConnectionInfo b) => a.Equals(b);

        public static bool operator !=(BrokerConnectionInfo a, BrokerConnectionInfo b) => !a.Equals(b);
    }
}
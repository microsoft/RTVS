// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core;

namespace Microsoft.R.Host.Client.Host {
    [DebuggerDisplay("{Uri}, IsRemote={IsRemote}, InterpreterId={InterpreterId}")]
    public struct BrokerConnectionInfo {
        public Uri Uri { get; }
        public bool IsValid { get; }
        public bool IsRemote { get; }
        public string ParametersId { get; }
        public string RCommandLineArguments { get; }
        public string InterpreterId { get; }

        public static BrokerConnectionInfo Create(string path, string rCommandLineArguments = null) {
            rCommandLineArguments = rCommandLineArguments ?? string.Empty;

            Uri uri;
            if (!Uri.TryCreate(path, UriKind.Absolute, out uri)) {
                return new BrokerConnectionInfo();
            }

            if (uri.IsFile) {
                return new BrokerConnectionInfo(uri, rCommandLineArguments, string.Empty, false);
            }

            var fragment = uri.Fragment;
            var interpreterId = string.IsNullOrEmpty(fragment) ? string.Empty : fragment.Substring(1);
            uri = new Uri(uri.GetLeftPart(UriPartial.Query));
            return new BrokerConnectionInfo(uri, rCommandLineArguments, interpreterId, true);
        }

        private BrokerConnectionInfo(Uri uri, string rCommandLineArguments, string interpreterId, bool isRemote) {
            IsValid = true;
            Uri = uri;
            RCommandLineArguments = rCommandLineArguments?.Trim() ?? string.Empty;
            InterpreterId = interpreterId;
            ParametersId = string.IsNullOrEmpty(rCommandLineArguments) && string.IsNullOrEmpty(interpreterId) 
                ? string.Empty 
                : $"{rCommandLineArguments}/{interpreterId}".GetSHA256FileSystemSafeHash();
            IsRemote = isRemote;
        }

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
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Protocol {
    public class RUserProfileCreateRequest : IUserCredentials {
        [JsonConstructor]
        private RUserProfileCreateRequest() { }

        public string Username { get; set; }
        public string Domain { get; set; }

        [JsonConverter(typeof(SecureStringJsonConverter))]
        public SecureString Password { get; set; }

        public static RUserProfileCreateRequest Create(string username, string domain, string password) {
            return new RUserProfileCreateRequest() { Username = username, Domain = domain, Password = password.ToSecureString() };
        }
    }
}

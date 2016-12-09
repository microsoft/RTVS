// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security;
using Microsoft.Common.Core.OS;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Protocol {
    public class RUserProfileServiceRequest : IUserCredentials {
        [JsonConstructor]
        private RUserProfileServiceRequest() { }

        public RUserProfileServiceRequest(string username, string domain, SecureString password) {
            Username = username;
            Domain = domain;
            Password = password;
        }

        public string Username { get; set; }
        public string Domain { get; set; }

        [JsonConverter(typeof(SecureStringJsonConverter))]
        public SecureString Password { get; set; }
    }
}

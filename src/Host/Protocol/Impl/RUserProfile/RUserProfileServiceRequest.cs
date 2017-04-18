// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.OS;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Protocol {
    public class RUserProfileServiceRequest : IUserCredentials {
        [JsonConstructor]
        private RUserProfileServiceRequest() { }

        public RUserProfileServiceRequest(string username, string domain, string sid) {
            Username = username;
            Domain = domain;
            Sid = sid;
        }

        public string Username { get; set; }
        public string Domain { get; set; }
        public string Sid { get; set; }
    }
}

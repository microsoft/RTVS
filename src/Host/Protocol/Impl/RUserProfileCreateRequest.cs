// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.OS;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Protocol {
    public class RUserProfileCreateRequest : IUserCredentials {
        [JsonConstructor]
        private RUserProfileCreateRequest() { }

        public string Username { get; set; }
        public string Domain { get; set; }
        public string Password { get; set; }

        public static RUserProfileCreateRequest Create(string username, string domain, string password) {
            return new RUserProfileCreateRequest() { Username = username, Domain = domain, Password = password };
        }
    }
}

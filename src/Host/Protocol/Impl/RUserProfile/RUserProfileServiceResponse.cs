// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.OS;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Protocol {
    public class RUserProfileServiceResponse : IUserProfileServiceResult {
        [JsonConstructor]
        private RUserProfileServiceResponse() { }

        public uint Error { get; set; }
        public bool ProfileExists { get; set; }
        public string ProfilePath { get; set; }

        public static RUserProfileServiceResponse Blank => new RUserProfileServiceResponse() { Error = 13, ProfileExists = false, ProfilePath = string.Empty };

        public static RUserProfileServiceResponse Create(uint error, bool profileExists, string profilePath) {
            return new RUserProfileServiceResponse() { Error = error, ProfileExists = profileExists, ProfilePath = profilePath };
        }
    }

    public static class RUserProfileCreateResponseExtension {
        public static bool IsInvalidResponse(this RUserProfileServiceResponse response) {
            return response.Error == 13;
        }
    }
}

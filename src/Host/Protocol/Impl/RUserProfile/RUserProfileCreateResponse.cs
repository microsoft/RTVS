// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.OS;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Protocol {
    public class RUserProfileCreateResponse : IUserProfileCreatorResult {
        [JsonConstructor]
        private RUserProfileCreateResponse() { }

        public uint Error { get; set; }
        public bool ProfileExists { get; set; }
        public string ProfilePath { get; set; }

        public static RUserProfileCreateResponse Blank => new RUserProfileCreateResponse() { Error = 13, ProfileExists = false, ProfilePath = string.Empty };

        public static RUserProfileCreateResponse Create(uint error, bool profileExists, string profilePath) {
            return new RUserProfileCreateResponse() { Error = error, ProfileExists = profileExists, ProfilePath = profilePath };
        }
    }

    public static class RUserProfileCreateResponseExtension {
        public static bool IsInvalidResponse(this RUserProfileCreateResponse response) {
            return response.Error == 13;
        }
    }
}

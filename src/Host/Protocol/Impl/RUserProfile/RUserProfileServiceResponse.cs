// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.OS;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Protocol {
    public class RUserProfileServiceResponse : IUserProfileServiceResult {
        [JsonConstructor]
        private RUserProfileServiceResponse() { }

        public RUserProfileServiceResponse(uint error, bool profileExists, string profilePath) {
            Error = error;
            ProfileExists = profileExists;
            ProfilePath = profilePath;
        }

        public uint Error { get; set; }
        public bool ProfileExists { get; set; }
        public string ProfilePath { get; set; }

        public static RUserProfileServiceResponse Blank => new RUserProfileServiceResponse() { Error = 13, ProfileExists = false, ProfilePath = string.Empty };
    }

    public static class IUserProfileServiceResultExtension {
        public static bool IsInvalidResponse(this IUserProfileServiceResult response) => response.Error == 13;
    }
}

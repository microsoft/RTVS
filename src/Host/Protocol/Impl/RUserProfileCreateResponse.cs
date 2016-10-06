using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Protocol {
    public struct RUserProfileCreateResponse {
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

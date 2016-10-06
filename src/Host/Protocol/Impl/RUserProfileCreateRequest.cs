using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Protocol {
    public struct RUserProfileCreateRequest {
        public string Username { get; set; }
        public string Domain { get; set; }
        public string Password { get; set; }

        public static RUserProfileCreateRequest Create(string username, string domain, string password) {
            return new RUserProfileCreateRequest() { Username = username, Domain = domain, Password = password };
        }
    }
}

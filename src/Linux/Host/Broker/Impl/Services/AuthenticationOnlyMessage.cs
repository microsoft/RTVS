using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.R.Host.Broker.Services {
    internal class AuthenticationOnlyMessage {
        public string Name { get; } = "AuthOnly";
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

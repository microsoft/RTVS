// From https://github.com/Kukkimonsuta/Odachi/tree/master/src/Odachi.AspNetCore.Authentication.Basic

namespace Odachi.AspNetCore.Authentication.Basic {
    public class BasicCredential {
        public string Username { get; set; }
        public string Password { get; set; }
        public BasicCredentialClaim[] Claims { get; set; } = new BasicCredentialClaim[0];
    }

    public class BasicCredentialClaim {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
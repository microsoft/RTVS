// From https://github.com/Kukkimonsuta/Odachi/blob/master/src/Odachi.AspNetCore.Authentication.Basic/Events/BasicSignInContext.cs

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Odachi.AspNetCore.Authentication.Basic {
    /// <summary>
    /// Context object used to control flow of basic authentication.
    /// </summary>
    public class BasicSignInContext : ResultContext<BasicOptions> {
        /// <summary>
        /// Creates a new instance of the context object.
        /// </summary>
        public BasicSignInContext(HttpContext context, AuthenticationScheme scheme, BasicOptions options)
            : base(context, scheme, options) {
        }

        /// <summary>
        /// Contains the username used in basic authentication.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Contains the password used in basic authentication.
        /// </summary>
        public string Password { get; set; }
    }
}

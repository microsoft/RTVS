// From https://github.com/Kukkimonsuta/Odachi/blob/master/src/Odachi.AspNetCore.Authentication.Basic/Events/AuthenticationFailedContext%20.cs

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;

namespace Odachi.AspNetCore.Authentication.Basic {
    /// <summary>
    /// Context object used to control flow of basic authentication.
    /// </summary>
    public class AuthenticationFailedContext : ResultContext<BasicOptions> {
        /// <summary>
        /// Creates a new instance of the context object.
        /// </summary>
        public AuthenticationFailedContext(HttpContext context, AuthenticationScheme scheme, BasicOptions options)
            : base(context, scheme, options) {
        }

        /// <summary>
        /// The exception thrown.
        /// </summary>
        public Exception Exception { get; set; }
    }
}
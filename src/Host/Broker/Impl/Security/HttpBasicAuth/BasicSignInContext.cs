// From https://github.com/Kukkimonsuta/Odachi/tree/master/src/Odachi.AspNetCore.Authentication.Basic

using Microsoft.AspNetCore.Http;

namespace Odachi.AspNetCore.Authentication.Basic
{
    /// <summary>
    /// Context object used to control flow of basic authentication.
    /// </summary>
    public class BasicSignInContext : BaseBasicContext
    {
        /// <summary>
        /// Creates a new instance of the context object.
        /// </summary>
        /// <param name="context">The HTTP request context</param>
        /// <param name="options">The middleware options</param>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        public BasicSignInContext(
            HttpContext context,
            BasicOptions options,
            string username,
            string password
        )
            : base(context, options)
        {
            Username = username;
            Password = password;
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
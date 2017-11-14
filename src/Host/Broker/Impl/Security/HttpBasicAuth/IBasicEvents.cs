// From https://github.com/Kukkimonsuta/Odachi/tree/master/src/Odachi.AspNetCore.Authentication.Basic

using System.Threading.Tasks;

namespace Odachi.AspNetCore.Authentication.Basic {
    /// <summary>
    /// Specifies callback methods which the <see cref="BasicMiddleware"></see> invokes to enable developer control over the authentication process.
    /// </summary>
    public interface IBasicEvents {
        /// <summary>
        /// Called when an exception occurs during request or response processing.
        /// </summary>
        /// <param name="context">Contains information about the exception that occurred</param>
        Task AuthenticationFailed(AuthenticationFailedContext context);

        /// <summary>
        /// Called when a request came with basic authentication credentials. By implementing this method the credentials can be converted to
        /// a principal.
        /// </summary>
        /// <param name="context">Contains information about the sign in request.</param>
        Task SignIn(BasicSignInContext context);
    }
}
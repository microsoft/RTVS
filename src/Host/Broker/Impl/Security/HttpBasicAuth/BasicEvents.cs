// From https://github.com/Kukkimonsuta/Odachi/tree/master/src/Odachi.AspNetCore.Authentication.Basic

using System;
using System.Threading.Tasks;

namespace Odachi.AspNetCore.Authentication.Basic {
    /// <summary>
    /// This default implementation of the <see cref="IBasicEvents"/> may be used if the
    /// application only needs to override a few of the interface methods. This may be used as a base class
    /// or may be instantiated directly.
    /// </summary>
    public class BasicEvents : IBasicEvents {
        /// <summary>
        /// Create a new instance of the default notifications.
        /// </summary>
        public BasicEvents() {
        }

        /// <summary>
        /// Invoked if exceptions are thrown during request processing. The exceptions will be re-thrown after this event unless suppressed.
        /// </summary>
        public Func<AuthenticationFailedContext, Task> OnAuthenticationFailed { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// A delegate assigned to this property will be invoked when the related method is called
        /// </summary>
        public Func<BasicSignInContext, Task> OnSignIn { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// Implements the interface method by invoking the related delegate method
        /// </summary>
        /// <param name="context">Contains information about the event</param>
        public virtual Task AuthenticationFailed(AuthenticationFailedContext context) => OnAuthenticationFailed(context);

        /// <summary>
        /// Implements the interface method by invoking the related delegate method
        /// </summary>
        /// <param name="context"></param>
        public virtual Task SignIn(BasicSignInContext context) => OnSignIn(context);
    }
}
// From https://github.com/Kukkimonsuta/Odachi/tree/master/src/Odachi.AspNetCore.Authentication.Basic

namespace Odachi.AspNetCore.Authentication.Basic
{
    /// <summary>
    /// Default values used by <see cref="BasicMiddleware"/> when not defined in <see cref="BasicOptions"/>.
    /// </summary>
    public static class BasicDefaults
    {
        /// <summary>
        /// The default authentication scheme used by basic authentication.
        /// </summary>
        public const string AuthenticationScheme = "Basic";

        /// <summary>
        /// The default realm used by basic authentication.
        /// </summary>
        public const string Realm = "Protected Area";
    }
}
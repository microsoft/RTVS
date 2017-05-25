// From https://github.com/Kukkimonsuta/Odachi/tree/master/src/Odachi.AspNetCore.Authentication.Basic

using Microsoft.Extensions.Options;
using Odachi.AspNetCore.Authentication.Basic;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Basic authentication extensions for <see cref="IApplicationBuilder"/>.
    /// </summary>
    public static class BasicAppBuilderExtensions
    {
        /// <summary>
        /// Adds a basic authentication middleware to your web application pipeline. This method attempts to obtain options though `IOptions`.
        /// </summary>
        /// <param name="app">The IApplicationBuilder passed to your configuration method</param>
        /// <returns>The original app parameter</returns>
        public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app)
        {
            if (app == null) {
                throw new ArgumentNullException(nameof(app));
            }

            var options = app.ApplicationServices.GetService<IOptions<BasicOptions>>();

			return app.UseBasicAuthentication(options?.Value ?? new BasicOptions());
        }

		/// <summary>
		/// Adds a basic authentication middleware to your web application pipeline. This method does not attempt to obtain options though `IOptions`.
		/// </summary>
		/// <param name="app">The IApplicationBuilder passed to your configuration method</param>
		/// <param name="configureOptions">Used to configure the options for the middleware</param>
		/// <returns>The original app parameter</returns>
		public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app, Action<BasicOptions> configureOptions)
        {
            if (app == null) {
                throw new ArgumentNullException(nameof(app));
            }

            var options = new BasicOptions();
            if (configureOptions != null) {
                configureOptions(options);
            }

            return app.UseBasicAuthentication(options);
        }

		/// <summary>
		/// Adds a basic authentication middleware to your web application pipeline.  This method does not attempt to obtain options though `IOptions`.
		/// </summary>
		/// <param name="app">The IApplicationBuilder passed to your configuration method</param>
		/// <param name="options">Used to configure the middleware</param>
		/// <returns>The original app parameter</returns>
		public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app, BasicOptions options)
        {
            if (app == null) {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null) {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<BasicMiddleware>(options);
        }
    }
}
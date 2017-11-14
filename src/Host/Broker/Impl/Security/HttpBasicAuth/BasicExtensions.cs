using System;
using Microsoft.AspNetCore.Authentication;
// From https://raw.githubusercontent.com/Kukkimonsuta/Odachi/master/src/Odachi.AspNetCore.Authentication.Basic/BasicExtensions.cs

using Odachi.AspNetCore.Authentication.Basic;

namespace Microsoft.Extensions.DependencyInjection {
    public static class BasicExtensions {
        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder)
            => builder.AddBasic(BasicDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder, Action<BasicOptions> configureOptions)
            => builder.AddBasic(BasicDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder, string authenticationScheme, Action<BasicOptions> configureOptions)
            => builder.AddBasic(authenticationScheme, displayName: null, configureOptions: configureOptions);

        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<BasicOptions> configureOptions)
            => builder.AddScheme<BasicOptions, BasicHandler>(authenticationScheme, displayName, configureOptions);
    }
}
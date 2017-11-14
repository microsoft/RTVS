// From https://github.com/Kukkimonsuta/Odachi/tree/master/src/Odachi.AspNetCore.Authentication.Basic

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System;
using Microsoft.AspNetCore.Builder;

namespace Odachi.AspNetCore.Authentication.Basic {
    /// <summary>
    /// Contains the options used by the <see cref="BasicMiddleware"/>.
    /// </summary>
    public class BasicOptions : AuthenticationSchemeOptions {
        /// <summary>
        /// The default realm used by basic authentication.
        /// </summary>
        public string Realm { get; set; } = BasicDefaults.Realm;

        /// <summary>
        /// Allowed credentials.
        /// </summary>
        public BasicCredential[] Credentials { get; set; } = new BasicCredential[0];

        /// <summary>
        /// The object provided by the application to process events raised by the bearer authentication handler.
        /// The application may implement the interface fully, or it may create an instance of JwtBearerEvents
        /// and assign delegates only to the events it wants to process.
        /// </summary>
        public new BasicEvents Events {
            get => (BasicEvents)base.Events;
            set => base.Events = value;
        }
    }
}
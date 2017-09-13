// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.R.LanguageServer.Services;

namespace Microsoft.R.LanguageServer.Server {
    internal sealed class Session: ISession, IDisposable {
        private static Session _instance;
        public static ISession Current => _instance;

        private readonly ServiceContainer _services = new ServiceContainer();

        public IServiceContainer Services => _services;

        public static Session Create() {
            Check.InvalidOperation(() => _instance == null);
            _instance = new Session();
            return _instance;
        }

        public void Dispose() => _services?.Dispose();
    }
}

﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;

namespace Microsoft.R.LanguageServer.Server {
    internal interface ISession {
        IServiceContainer Services { get; }
    }
}
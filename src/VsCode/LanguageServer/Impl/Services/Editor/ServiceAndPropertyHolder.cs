// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;

namespace Microsoft.R.LanguageServer.Services.Editor {
    /// <summary>
    /// Base class for objects that have properties and services
    /// such as <see cref="IEditorBuffer" /> and <see cref="IEditorView"/>.
    /// </summary>
    internal abstract class ServiceAndPropertyHolder : PropertyHolder {
        protected ServiceManager ServiceManager { get; } = new ServiceManager();
        public IServiceManager Services => ServiceManager;
    }
}

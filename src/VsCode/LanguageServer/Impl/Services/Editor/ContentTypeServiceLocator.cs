// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Languages.Editor.Services;

namespace Microsoft.R.LanguageServer.Services.Editor {
    internal sealed class ContentTypeServiceLocator: IContentTypeServiceLocator {
        public T GetService<T>(string contentType) where T : class => default(T);
        public IEnumerable<T> GetAllServices<T>(string contentType) where T : class => Enumerable.Empty<T>();
        public IEnumerable<Lazy<T>> GetAllOrderedServices<T>(string contentType) where T : class => Enumerable.Empty<Lazy<T>>();
    }
}

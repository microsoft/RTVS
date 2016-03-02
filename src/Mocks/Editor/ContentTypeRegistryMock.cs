// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Editor.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class ContentTypeRegistryServiceMock : IContentTypeRegistryService
    {
        private List<IContentType> _contentTypes = new List<IContentType>();

        public ContentTypeRegistryServiceMock(IEnumerable<string> contentTypeNames)
        {
            foreach(string name in contentTypeNames)
            {
                _contentTypes.Add(new ContentTypeMock(name));
            }
        }

        public IEnumerable<IContentType> ContentTypes
        {
            get { return _contentTypes; }
        }

        public IContentType UnknownContentType
        {
            get { return new ContentTypeMock("unknown"); }
        }

        public IContentType AddContentType(string typeName, IEnumerable<string> baseTypeNames)
        {
            throw new NotImplementedException();
        }

        public IContentType GetContentType(string typeName)
        {
            return _contentTypes.FirstOrDefault((IContentType ct) => { return ct.TypeName == typeName; });
        }

        public void RemoveContentType(string typeName)
        {
            throw new NotImplementedException();
        }
    }
}

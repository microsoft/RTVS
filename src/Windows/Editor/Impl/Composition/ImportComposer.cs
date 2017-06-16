// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Microsoft.Languages.Editor.Composition {
    public class ImportComposer<TInterface, TMetadata>
        where TInterface : class
        where TMetadata : IContentTypeMetadata {
        [ImportMany]
        protected IEnumerable<Lazy<TInterface, TMetadata>> Imports { get; set; }

        public ImportComposer(ICompositionService cs) {
            cs.SatisfyImportsOnce(this);
        }

        public virtual TInterface GetImport(string attributeValue)
            => Imports.FirstOrDefault(x => x.Metadata.ContentTypes.Contains(attributeValue, StringComparer.OrdinalIgnoreCase))?.Value;


        public virtual ICollection<TInterface> GetAll(string attributeValue)
            => Imports.Where(x => x.Metadata.ContentTypes.Contains(attributeValue, StringComparer.OrdinalIgnoreCase))
                      .Select(x => x.Value)
                      .ToList();

        public virtual ICollection<Lazy<TInterface, TMetadata>> GetAllLazy(string attributeValue)
            => Imports.Where(x => x.Metadata.ContentTypes.Contains(attributeValue, StringComparer.OrdinalIgnoreCase))
                      .ToList();
    }
}

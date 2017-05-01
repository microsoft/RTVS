// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Core.Composition;

namespace Microsoft.Languages.Editor.Composition {
    public class ImportComposer<TInterface, TMetadata>
        where TInterface : class
        where TMetadata : IContentTypeMetadata {
        [ImportMany]
        protected IEnumerable<Lazy<TInterface, TMetadata>> Imports { get; set; }

        public ImportComposer(ICompositionService cs) {
            cs.SatisfyImportsOnce(this);
        }

        public virtual TInterface GetImport(string attributeValue) {
            foreach (var import in Imports) {
                foreach (var value in import.Metadata.ContentTypes) {
                    if (String.Compare(value, attributeValue, StringComparison.OrdinalIgnoreCase) == 0) {
                        return import.Value;
                    }
                }
            }

            return null;
        }

        public virtual ICollection<TInterface> GetAll(string attributeValue) {
            var list = new List<TInterface>();

            foreach (var import in Imports) {
                foreach (var value in import.Metadata.ContentTypes) {
                    if (String.Compare(value, attributeValue, StringComparison.OrdinalIgnoreCase) == 0) {
                        list.Add(import.Value);
                    }
                }
            }

            return list;
        }

        public virtual ICollection<Lazy<TInterface, TMetadata>> GetAllLazy(string attributeValue) {
            var list = new List<Lazy<TInterface, TMetadata>>();

            foreach (var import in Imports) {
                foreach (var value in import.Metadata.ContentTypes) {
                    if (String.Compare(value, attributeValue, StringComparison.OrdinalIgnoreCase) == 0) {
                        list.Add(import);
                    }
                }
            }

            return list;
        }
    }
}

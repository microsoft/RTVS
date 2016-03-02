// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.VisualStudio.R.Package.Snippets {
    internal class SnippetInfoList : List<SnippetInfo> {
        public string Path {
            get { return this.Count > 0 ? this[0].Path : null; }
            set {
                foreach (var snippetInfo in this) {
                    snippetInfo.Path = value;
                }
            }
        }

        public string Kind {
            get { return this.Count > 0 ? this[0].Kind : null; }
        }
    }
}

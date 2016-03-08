// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Editor.Snippets;

namespace Microsoft.Languages.Editor.Application.Snippets {
    [Export(typeof(ISnippetInformationSourceProvider))]
    public sealed class SnippetInformationSourceProvider : ISnippetInformationSourceProvider {
        public ISnippetInformationSource InformationSource {
            get { return new SnippetInformationSource(); }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core;
using Microsoft.R.Editor.Snippets;

namespace Microsoft.Languages.Editor.Application.Snippets {
    [Export(typeof(ISnippetInformationSourceProvider))]
    public sealed class SnippetInformationSourceProvider : ISnippetInformationSourceProvider {
        private Lazy<SnippetInformationSource> _source = Lazy.Create(() => new SnippetInformationSource());
        public ISnippetInformationSource InformationSource => _source.Value;
    }
}

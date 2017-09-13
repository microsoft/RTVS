// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using LanguageServer.VsCode.Contracts;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Editor.Completions;
using Microsoft.R.Editor.Completions.Engine;
using Microsoft.R.LanguageServer.Documents;

namespace Microsoft.R.LanguageServer.Completions {
    internal sealed class CompletionManager: ICompletionManager {
        private readonly Guid _treeUserGuid = new Guid("DF3595E3-579C-48BD-9931-3E31F9FA7F46");
        private readonly IServiceContainer _services;

        public CompletionManager(IServiceContainer services) {
            Check.ArgumentNull(nameof(services), services);
            _services = services;
        }

        public CompletionList GetCompletions(DocumentEntry entry, int position) {
            IReadOnlyCollection<IRCompletionListProvider> providers;
            RIntellisenseContext context;
            try {
                var root = entry.Document.EditorTree.AcquireReadLock(_treeUserGuid);
                var session = new EditorIntellisenseSession(entry.View);
                context = new RIntellisenseContext(session, entry.EditorBuffer, root, position);

                var completionEngine = new RCompletionEngine(_services);
                providers = completionEngine.GetCompletionForLocation(context);
            } finally {
                entry.Document.EditorTree.ReleaseReadLock(_treeUserGuid);
            }

            if (providers == null || providers.Count == 0) {
                return new CompletionList();
            }

            var completions = new List<ICompletionEntry>();
            var sort = true;

            foreach (var provider in providers) {
                var entries = provider.GetEntries(context);
                if (entries.Count > 0) {
                    completions.AddRange(entries);
                }
                sort &= provider.AllowSorting;
            }

            if (sort) {
                completions.Sort(new CompletionEntryComparer(StringComparison.OrdinalIgnoreCase));
                completions.RemoveDuplicates(new CompletionEntryComparer(StringComparison.Ordinal));
            }

            var items = completions.Select(c => new CompletionItem(c.InsertionText, CompletionItemKind.Function, c.InsertionText, null));
            var list = new CompletionList(items);

            return list;
        }
    }
}

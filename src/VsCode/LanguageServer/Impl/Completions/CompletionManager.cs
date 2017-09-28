// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using LanguageServer.VsCode.Contracts;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Editor.Completions.Engine;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.LanguageServer.Completions {
    internal sealed class CompletionManager {
        private readonly RCompletionEngine _completionEngine;

        public CompletionManager(IServiceContainer services) {
            _completionEngine = new RCompletionEngine(services);
        }
        public CompletionList GetCompletions(IRIntellisenseContext context) {
            using (context.AstReadLock()) {
                var providers = _completionEngine.GetCompletionForLocation(context);
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

                var items = completions.Select(c => new CompletionItem {
                    Label = c.DisplayText,
                    InsertText = c.InsertionText,
                    Kind = (CompletionItemKind) c.ImageSource,
                    Documentation = c.Description,
                    Data = c.Data is string ? JToken.FromObject((string) c.Data) : null
                }).ToList();

                return new CompletionList(isIncomplete: true, items: items);
            }
        }
    }
}

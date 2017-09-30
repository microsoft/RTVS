// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageServer.VsCode.Contracts;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.Languages.Core;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Completions.Engine;
using Microsoft.R.Editor.Document;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.LanguageServer.Completions {
    internal sealed class CompletionManager {
        private readonly IServiceContainer _services;
        private readonly RCompletionEngine _completionEngine;

        public CompletionManager(IServiceContainer services) {
            _services = services;
            _completionEngine = new RCompletionEngine(services);
        }

        public async Task<CompletionList> GetCompletions(IRIntellisenseContext context) {
            await _services.MainThread().SwitchToAsync();
            context.EditorBuffer.GetEditorDocument<IREditorDocument>().EditorTree.EnsureTreeReady();

            var providers = _completionEngine.GetCompletionForLocation(context);
            if (providers == null || providers.Count == 0) {
                return new CompletionList();
            }

            // Do not generate thousands of items, VSCode cannot handle that.
            // Filter based on the text typed so far right away.
            var prefix = GetFilterPrefix(context);

            var completions = new List<ICompletionEntry>();
            var sort = true;

            foreach (var provider in providers) {
                var entries = provider.GetEntries(context, prefix);
                if (entries.Count > 0) {
                    completions.AddRange(entries);
                }
                sort &= provider.AllowSorting;
            }

            if (sort) {
                completions.Sort(new CompletionEntryComparer(StringComparison.OrdinalIgnoreCase));
                completions.RemoveDuplicates(new CompletionEntryComparer(StringComparison.Ordinal));
            }

            var sorted = new List<ICompletionEntry>();
            sorted.AddRange(completions.Where(c => c.DisplayText.EndsWith("=")));
            sorted.AddRange(completions.Where(c => char.IsLetter(c.DisplayText[0]) && !c.DisplayText.EndsWith("=")));

            var items = sorted
                .Select(c => new CompletionItem {
                    Label = c.DisplayText,
                    InsertText = c.InsertionText,
                    Kind = (CompletionItemKind)c.ImageSource,
                    Documentation = c.Description,
                    Data = c.Data is string ? JToken.FromObject((string)c.Data) : null
                }).ToList();

            return new CompletionList(items);
        }

        private static string GetFilterPrefix(IRIntellisenseContext context) {
            var snapshot = context.EditorBuffer.CurrentSnapshot;
            var line = snapshot.GetLineFromPosition(context.Position);
            var text = line.GetText();

            int i;
            var offset = context.Position - line.Start;
            for (i = offset - 1; i >= 0 && RTokenizer.IsIdentifierCharacter(text[i]); i--) { }

            i = Math.Min(offset, i + 1);
            return text.Substring(i, offset - i);
        }
    }
}

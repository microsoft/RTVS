﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Completions {
    internal sealed class RCompletionSet : CompletionSet {
        private readonly CompletionList _completions;
        private readonly FilteredObservableCollection<Completion> _filteredCompletions;

        public RCompletionSet(ITrackingSpan trackingSpan, List<ICompletionEntry> completions) :
            base("R Completion", "R Completion", trackingSpan, Enumerable.Empty<RCompletion>(), Enumerable.Empty<RCompletion>()) {
            _completions = OrderList(completions);
            _filteredCompletions = new FilteredObservableCollection<Completion>(_completions);
        }

        public override IList<Completion> Completions => _filteredCompletions;

        /// <summary>
        /// Performs filtering based on the potential commit character
        /// such as when user completes partially typed function argument
        /// with = and we need to pick exactly entry with = and not plain one.
        /// </summary>
        /// <param name="commitChar"></param>
        public void Filter(char commitChar) {
            UpdateVisibility(commitChar);
            _filteredCompletions.Filter(x => ((RCompletion)x).IsVisible);
        }

        public override void Filter() {
            UpdateVisibility();
            _filteredCompletions.Filter(x => ((RCompletion)x).IsVisible);
        }

        private void UpdateVisibility(char commitChar = '\0') {
            var matches = new Dictionary<int, List<Completion>>();
            var maxKey = 0;

            var typedText = GetTypedText();
            if (typedText.Length == 0) {
                return;
            }

            foreach (var c in _completions) {
                var key = Match(typedText, c.DisplayText, commitChar);
                if (key > 0) {
                    List<Completion> list;
                    if (!matches.TryGetValue(key, out list)) {
                        list = new List<Completion>();
                        matches[key] = list;
                        maxKey = Math.Max(maxKey, key);
                    }
                    list.Add(c);
                }
            }

            if (maxKey > 0) {
                _completions.ForEach(x => ((RCompletion)x).IsVisible = false);
                matches[maxKey].ForEach(x => ((RCompletion)x).IsVisible = true);
            }
        }

        private int Match(string typedText, string compText, char commitChar) {
            if (compText[compText.Length-1] == commitChar) { // like 'name ='
                if (compText.StartsWithIgnoreCase(typedText)) {
                    return compText.Length;
                }
            }

            // Match at least something
            int i = 0;
            for (i = 0; i < Math.Min(typedText.Length, compText.Length); i++) {
                if (char.ToLowerInvariant(typedText[i]) != char.ToLowerInvariant(compText[i])) {
                    return i;
                }
            }

            return i;
        }

        private string GetTypedText() {
            var snapshot = ApplicableTo.TextBuffer.CurrentSnapshot;
            return ApplicableTo.GetText(snapshot);
        }

        private static CompletionList OrderList(IReadOnlyCollection<ICompletionEntry> completions) {
            // Place 'name =' at the top prioritizing argument names
            // Place items starting with non-alpha characters like .Call and &&
            // at the end of the list.
            var argumentNames = completions.Where(x => RTokenizer.IsIdentifierCharacter(x.DisplayText[0]) && x.DisplayText.EndsWith("=", StringComparison.Ordinal)).ToArray();
            var rtvsNames = completions.Where(x => x.DisplayText.IndexOfIgnoreCase(".rtvs") >= 0).ToArray();
            var specialNames = completions.Where(x => !char.IsLetter(x.DisplayText[0])).Except(rtvsNames).ToArray();

            var generalEntries = completions.Except(argumentNames);
            generalEntries = generalEntries.Except(rtvsNames);
            generalEntries = generalEntries.Except(specialNames);

            var orderedCompletions = new List<ICompletionEntry>();
            orderedCompletions.AddRange(argumentNames);
            orderedCompletions.AddRange(generalEntries);
            orderedCompletions.AddRange(specialNames);

            return new CompletionList(orderedCompletions);
        }
    }
}
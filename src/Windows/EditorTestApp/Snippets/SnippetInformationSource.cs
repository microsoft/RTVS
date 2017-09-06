// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.R.Editor.Snippets;

namespace Microsoft.Languages.Editor.Application.Snippets {
    /// <summary>
    /// Test implementation of the snippet information.
    /// Provides limited set of snippets for test purposes.
    /// </summary>
    public sealed class SnippetInformationSource : ISnippetInformationSource {
        private string[] _testSnippetNames = new string[] { "if", "for", "while" };
        public IEnumerable<ISnippetInfo> Snippets {
            get {
                foreach (var n in _testSnippetNames) {
                    yield return new SnippetInfo(n, string.Empty);
                }
            }
        }

        public bool IsSnippet(string name) {
            return _testSnippetNames.Contains(name);
        }

        class SnippetInfo : ISnippetInfo {
            public string Description { get; }
            public string Name { get; }

            public SnippetInfo(string name, string description) {
                Name = name;
                Description = description;
            }
        }
    }
}

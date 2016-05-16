// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    /// <summary>
    /// Arguments for language block collection events
    /// </summary>
    public sealed class LanguageBlockCollectionEventArgs : EventArgs {
        /// <summary>
        /// Language block that was added or removed
        /// </summary>
        public LanguageBlock LanguageBlock { get; }

        public LanguageBlockCollectionEventArgs(LanguageBlock languageBlock) {
            LanguageBlock = languageBlock;
        }
    }
}

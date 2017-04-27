﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Editor.Functions;
using static System.FormattableString;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// Completion function entry in the R intellisense completion set
    /// </summary>
    [DebuggerDisplay("{DisplayText}")]
    public class RFunctionCompletionEntry : EditorCompletionEntry {
        private readonly IFunctionIndex _functionIndex;
        private readonly IEditorIntellisenseSession _session;

        public RFunctionCompletionEntry(string displayText, string insertionText, string description, object iconSource, IFunctionIndex functionIndex, IEditorIntellisenseSession session) :
            base(displayText, insertionText, description, iconSource) {
            _functionIndex = functionIndex;
            _session = session;
        }

        public override string Description {
            get {
                if (string.IsNullOrEmpty(base.Description) && !_session.IsDismissed) {
                    TryFetchDescription();
                }
                return base.Description;
            }
        }

        private void TryFetchDescription() {
            Task.Run(async () => {
                SetDescription(await _functionIndex.GetFunctionInfoAsync(this.DisplayText));
            }).Wait(500);
        }

        private void SetDescription(IFunctionInfo fi) {
            if (fi != null) {
                string sig = (fi.Signatures.Count > 0) ? fi.Signatures[0].GetSignatureString(DisplayText) : null;
                this.Description = (sig != null) ? Invariant($"{sig}{Environment.NewLine}{Environment.NewLine}{fi.Description}") : fi.Description;
            }
        }
    }
}
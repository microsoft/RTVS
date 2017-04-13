// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.R.Editor.Functions;
using Microsoft.VisualStudio.Language.Intellisense;
using static System.FormattableString;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// Completion function entry in the R intellisense completion set
    /// </summary>
    [DebuggerDisplay("{DisplayText}")]
    public class RFunctionCompletion : RCompletion {
        private readonly IFunctionIndex _functionIndex;
        private readonly ICompletionSession _session;

        public RFunctionCompletion(string displayText, string insertionText, string description, ImageSource iconSource, IFunctionIndex functionIndex, ICompletionSession session) :
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
            set { base.Description = value; }
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

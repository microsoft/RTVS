// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Editor.Functions;
using static System.FormattableString;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// Completion function entry in the R intellisense completion set
    /// </summary>
    [DebuggerDisplay("{" + nameof(DisplayText) + "}")]
    public sealed class RFunctionCompletionEntry : EditorCompletionEntry {
        private readonly IFunctionIndex _functionIndex;
        private readonly IEditorIntellisenseSession _session;
        private readonly string _packageName;

        public RFunctionCompletionEntry(string displayText, string insertionText, string description, object iconSource, string packageName, IFunctionIndex functionIndex, IEditorIntellisenseSession session) :
            base(displayText, insertionText, description, iconSource) {
            Data = _packageName = packageName;
            _functionIndex = functionIndex;
            _session = session;
        }

        public override string Description {
            get {
                if (string.IsNullOrEmpty(base.Description) && !_session.IsDismissed) {
                    _functionIndex.GetFunctionInfoAsync(DisplayText, _packageName, (fi, o) => SetDescription(fi), null);
                }
                return base.Description;
            }
        }

        private void SetDescription(IFunctionInfo fi) {
            if (fi != null && !_session.IsDismissed) {
                var sig = (fi.Signatures.Count > 0) ? fi.Signatures[0].GetSignatureString(DisplayText) : null;
                Description = (sig != null) ? Invariant($"{sig}{Environment.NewLine}{Environment.NewLine}{fi.Description.RemoveLineBreaks()}") : fi.Description;
            }
        }
    }
}

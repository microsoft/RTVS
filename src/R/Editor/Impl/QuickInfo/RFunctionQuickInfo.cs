// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.QuickInfo;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.Signatures;

namespace Microsoft.R.Editor.QuickInfo {
    public sealed class RFunctionQuickInfo : EditorQuickInfo, IRFunctionQuickInfo {
        public string FunctionName { get; }

        public RFunctionQuickInfo(string functionName, IEnumerable<string> content, ITrackingTextRange applicableRange) :
            base(content, applicableRange) {
            FunctionName = functionName;
        }

        public static IRFunctionQuickInfo Create(IRFunctionSignatureHelp sig, ITrackingTextRange applicableRange) {
            var signatureString = sig.Content;
            if (!string.IsNullOrWhiteSpace(sig.Documentation)) {
                signatureString = signatureString + Environment.NewLine + sig.Documentation;
            }
            return new RFunctionQuickInfo(sig.FunctionName, new[] { signatureString }, applicableRange);
        }
    }
}

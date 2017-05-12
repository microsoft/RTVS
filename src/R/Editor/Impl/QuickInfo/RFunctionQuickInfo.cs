// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Languages.Editor.QuickInfo;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.Utility;
using Microsoft.R.Editor.Functions;
using Microsoft.R.Editor.Signatures;

namespace Microsoft.R.Editor.QuickInfo {
    public sealed class RFunctionQuickInfo : EditorQuickInfo, IRFunctionQuickInfo {
        public string FunctionName { get; }

        public RFunctionQuickInfo(string functionName, IEnumerable<string> content, ITrackingTextRange applicableRange) : 
            base(content, applicableRange) {
            FunctionName = functionName;
        }

        public static IRFunctionQuickInfo Create(IRFunctionSignatureHelp sig) {
            var signatureString = sig.Content;
            var wrapLength = Math.Min(SignatureInfo.MaxSignatureLength, signatureString.Length);

            if (!string.IsNullOrWhiteSpace(sig.Documentation)) {
                // VS may end showing very long tooltip so we need to keep 
                // description reasonably short: typically about
                // same length as the function signature.
                return new RFunctionQuickInfo(sig.FunctionName, 
                    new [] { signatureString + "\r\n" + sig.Documentation.Wrap(wrapLength)},
                    sig.ApplicableToRange);
            }
            return null;
        }
    }
}

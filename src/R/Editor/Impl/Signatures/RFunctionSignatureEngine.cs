// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Editor.Functions;
using Microsoft.R.Editor.QuickInfo;

namespace Microsoft.R.Editor.Signatures {
    /// <summary>
    /// Implements engine that provides information on function signature
    /// given intellisense context (text buffer, position, AST).
    /// </summary>
    public sealed class RFunctionSignatureEngine : IRFunctionSignatureEngine {
        private readonly IServiceContainer _services;
        private readonly IFunctionIndex _functionIndex;

        public RFunctionSignatureEngine(IServiceContainer services) {
            _services = services;
            _functionIndex = _services.GetService<IFunctionIndex>();
        }

        #region IFunctionSignatureEngine
        public void GetSignaturesAsync(IRIntellisenseContext context, Action<IEnumerable<IRFunctionSignatureHelp>> callback) {
            var snapshot = context.EditorBuffer.CurrentSnapshot;
            var position = context.Position;
            // Retrieve parameter positions from the current text buffer snapshot
            var signatureInfo = context.AstRoot.GetSignatureInfoFromBuffer(snapshot, context.Position);
            if (signatureInfo == null) {
                return;
            }

            position = Math.Min(signatureInfo.FunctionCall.SignatureEnd, position);
            var start = Math.Min(position, snapshot.Length);
            var end = Math.Min(signatureInfo.FunctionCall.SignatureEnd, snapshot.Length);

            var applicableToSpan = snapshot.CreateTrackingRange(TextRange.FromBounds(start, end));
            IFunctionInfo functionInfo = null;
            string packageName = null;

            // First try user-defined function
            if (string.IsNullOrEmpty(signatureInfo.PackageName)) {
                functionInfo = context.AstRoot.GetUserFunctionInfo(signatureInfo.FunctionName, position);
            } else {
                packageName = signatureInfo.PackageName;
            }

            if (functionInfo != null) {
                InvokeSignaturesCallback(functionInfo, applicableToSpan, context, callback);
                return;
            }

            // Get collection of function signatures from documentation (parsed RD file)
            _functionIndex.GetFunctionInfoAsync(signatureInfo.FunctionName, packageName, (fi, o) => {
                InvokeSignaturesCallback(fi, applicableToSpan, context, callback);
            });
            return;
        }

        public void GetQuickInfosAsync(IRIntellisenseContext context, Action<IEnumerable<IRFunctionQuickInfo>> callback) {
            if (context.Session.IsDismissed) {
                return;
            }

            // Get function name from the AST. We don't use signature support here since
            // when caret or mouse is inside function arguments such as in abc(de|f(x)) 
            // it gives information of the outer function since signature is about help
            // on the function arguments.
            var functionName = context.AstRoot.GetFunctionName(context.Position, out ITextRange nameRange, out FunctionCall fc);
            if(!string.IsNullOrEmpty(functionName) && fc != null) {
                context.Position = fc.OpenBrace.End;
                GetSignaturesAsync(context, sigs => {
                    callback(sigs.Select(s => RFunctionQuickInfo.Create(s)).ExcludeDefault());
                });
            }
        }

        #endregion

        private void InvokeSignaturesCallback(IFunctionInfo functionInfo, ITrackingTextRange applicableToSpan, IRIntellisenseContext context, Action<IEnumerable<IRFunctionSignatureHelp>> callback) {
            var signatures = new List<IRFunctionSignatureHelp>();
            if (functionInfo?.Signatures != null) {
                signatures.AddRange(functionInfo.Signatures.Select(s => RFunctionSignatureHelp.Create(context, functionInfo, s, applicableToSpan)));
                context.Session.Properties["functionInfo"] = functionInfo;
                callback(signatures);
            }
        }
    }
}

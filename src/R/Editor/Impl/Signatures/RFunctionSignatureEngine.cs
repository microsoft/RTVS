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
using Microsoft.R.Editor.Functions;
using Microsoft.R.Editor.QuickInfo;

namespace Microsoft.R.Editor.Signatures {
    /// <summary>
    /// Implements engine that provides information on function signature
    /// given intellisense context (text buffer, position, AST).
    /// </summary>
    public sealed class RFunctionSignatureEngine : IRFunctionSignatureEngine {
        private readonly IFunctionIndex _functionIndex;

        public RFunctionSignatureEngine(IServiceContainer services) {
            _functionIndex = services.GetService<IFunctionIndex>();
        }

        #region IFunctionSignatureEngine
        public IEnumerable<IRFunctionSignatureHelp> GetSignaturesAsync(IRIntellisenseContext context, Action<IEnumerable<IRFunctionSignatureHelp>> callback) {
            var snapshot = context.EditorBuffer.CurrentSnapshot;
            // Retrieve parameter positions from the current text buffer snapshot
            var signatureInfo = context.AstRoot.GetSignatureInfoFromBuffer(snapshot, context.Position);
            if (signatureInfo == null) {
                callback?.Invoke(Enumerable.Empty<IRFunctionSignatureHelp>());
                return null;
            }
            return GetSignaturesAsync(signatureInfo, context, callback);
        }

        private IEnumerable<IRFunctionSignatureHelp> GetSignaturesAsync(
              RFunctionSignatureInfo signatureInfo
            , IRIntellisenseContext context
            , Action<IEnumerable<IRFunctionSignatureHelp>> callback) {
            var snapshot = context.EditorBuffer.CurrentSnapshot;
            var position = Math.Min(Math.Min(signatureInfo.FunctionCall.SignatureEnd, context.Position), snapshot.Length);

            var applicableToSpan = GetApplicableSpan(signatureInfo, context);
            IFunctionInfo functionInfo = null;
            string packageName = null;

            // First try user-defined function
            if (string.IsNullOrEmpty(signatureInfo.PackageName)) {
                functionInfo = context.AstRoot.GetUserFunctionInfo(signatureInfo.FunctionName, position);
            } else {
                packageName = signatureInfo.PackageName;
            }

            if (functionInfo != null) {
                return MakeSignatures(functionInfo, applicableToSpan, context);
            }

            if (callback != null) {
                // Get collection of function signatures from documentation (parsed RD file)
                _functionIndex.GetFunctionInfoAsync(signatureInfo.FunctionName, packageName, (fi, o) => {
                    InvokeSignaturesCallback(fi, applicableToSpan, context, callback);
                });
            }
            return null;
        }

        public IEnumerable<IRFunctionQuickInfo> GetQuickInfosAsync(IRIntellisenseContext context, Action<IEnumerable<IRFunctionQuickInfo>> callback) {
            // Get function name from the AST. We don't use signature support here since
            // when caret or mouse is inside function arguments such as in abc(de|f(x)) 
            // it gives information of the outer function since signature is about help
            // on the function arguments.
            var functionName = context.AstRoot.GetFunctionName(context.Position, out var fc, out var fv);
            if (!string.IsNullOrEmpty(functionName) && fc != null) {
                var signatureInfo = context.AstRoot.GetSignatureInfo(fc, fv, fc.OpenBrace.End);
                var applicableRange = context.EditorBuffer.CurrentSnapshot.CreateTrackingRange(fv);
                var sigs = GetSignaturesAsync(signatureInfo, context, null);
                if (sigs != null) {
                    return MakeQuickInfos(sigs, applicableRange);
                }
                if (callback != null) {
                    GetSignaturesAsync(signatureInfo, context,
                        signatures => callback(MakeQuickInfos(signatures, applicableRange)));
                }
            } else {
                callback?.Invoke(Enumerable.Empty<IRFunctionQuickInfo>());
            }
            return null;
        }

        #endregion

        private ITrackingTextRange GetApplicableSpan(RFunctionSignatureInfo signatureInfo, IRIntellisenseContext context) {
            var snapshot = context.EditorBuffer.CurrentSnapshot;
            var start = signatureInfo.FunctionCall.OpenBrace.End;
            var end = Math.Min(signatureInfo.FunctionCall.SignatureEnd, snapshot.Length);
            return snapshot.CreateTrackingRange(TextRange.FromBounds(start, end));
        }

        private static void InvokeSignaturesCallback(IFunctionInfo functionInfo, ITrackingTextRange applicableToSpan, IRIntellisenseContext context, Action<IEnumerable<IRFunctionSignatureHelp>> callback) {
            var signatures = MakeSignatures(functionInfo, applicableToSpan, context);
            callback(signatures);
        }
        private static IEnumerable<IRFunctionSignatureHelp> MakeSignatures(IFunctionInfo functionInfo, ITrackingTextRange applicableToSpan, IRIntellisenseContext context) {
            var signatures = new List<IRFunctionSignatureHelp>();
            if (functionInfo?.Signatures != null) {
                using (context.AstReadLock()) {
                    signatures.AddRange(functionInfo.Signatures.Select(s => RFunctionSignatureHelp.Create(context, functionInfo, s, applicableToSpan)));
                    context.Session.Properties["functionInfo"] = functionInfo;
                }
            }
            return signatures;
        }

        private static IEnumerable<IRFunctionQuickInfo> MakeQuickInfos(IEnumerable<IRFunctionSignatureHelp> sigs, ITrackingTextRange applicableRange)
            => sigs.Select(s => RFunctionQuickInfo.Create(s, applicableRange)).ExcludeDefault();
    }
}
